using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameSparks.RT;
using NetworkFramework.Data;
using NetworkFramework.Enums;
using NetworkFramework.Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;
using NetworkPlayer = NetworkFramework.Data.NetworkPlayer;

namespace NetworkFramework.Core
{
    public abstract class NetworkBehaviour : MonoBehaviour, IEquatable<NetworkBehaviour>
    {
        #region variables
        [SerializeField] private int _instanceId;
        [SerializeField] private NetworkPlayer _networkOwner;
        [SerializeField] private NetworkOwnerType _ownerType = NetworkOwnerType.Mine;
        [SerializeField] private bool _reciveNetworkMessage = true;
        
        public event Action<NetworkPlayer> OnNetworkOwnerChanged;
        public event Action<NetworkOwnerType> OnOwnerTypeChanged;
        public enum NetworkOwnerType { Mine, Remote, StaticInScene, Server }
        #endregion

        #region incapsulate
        public int InstanceId
        {
            get { return _instanceId; }
            set { _instanceId = value; }
        }

        public NetworkOwnerType OwnerType
        {
            get { return _ownerType; }
            set
            {
                if (!_ownerType.Equals(value))
                    OnOwnerTypeChanged?.Invoke(value);
                _ownerType = value;
            }
        }

        public bool ReciveNetworkMessage
        {
            get { return _reciveNetworkMessage; }
            set { _reciveNetworkMessage = value; }
        }

        public NetworkPlayer NetworkOwner
        {
            get { return _networkOwner; }
            set
            {
                if (!_networkOwner.Equals(value))
                    OnNetworkOwnerChanged?.Invoke(value);
                _networkOwner = value;
            }
        }

        public bool IsMine => _ownerType == NetworkOwnerType.Mine;
        public NetworkManager NetworkManager => NetworkManager.Instance();
        public bool IsNetworkMatch => NetworkManager.Instance() != null && NetworkManager.Instance().NetworkStream != null;
        #endregion

        #region engine methods
        protected virtual void OnDestroy() => NetworkManager.Instance()?.DeleteBehaviour(this);
        #endregion

        #region network events
        public virtual void OnNetworkPackage(NetworkPackage package, int index)
        {
        }
        #endregion

        #region network methods
        protected void SendNetworkPackage(NetworkPackage data)
        {
            if (!ReciveNetworkMessage || NetworkManager == null || NetworkManager.NetworkStream == null) return;
            if (InstanceId == 0) throw new Exception(GetType().Name + " InstanceId = 0");
            NetworkManager.Instance().NetworkStream.SendData(InstanceId, data.DeliveryIntent, data.Data);
            data.Data.Dispose();
//            Tracer();
        }

        protected void SendNetworkPackage(NetworkPackage data, params int[] peerIds)
        {
            if (!ReciveNetworkMessage || NetworkManager == null || NetworkManager.NetworkStream == null) return;
            if (InstanceId == 0) throw new Exception(GetType().Name + " InstanceId = 0");
            NetworkManager.Instance().NetworkStream.SendData(InstanceId, data.DeliveryIntent, data.Data, peerIds);
            data.Data.Dispose();
//            Tracer();
        }
        
        protected void SendNetworkPackage(NetworkPackage data, List<int> peerIds)
        {
            if (!ReciveNetworkMessage || NetworkManager == null || NetworkManager.NetworkStream == null) return;
            if (InstanceId == 0) throw new Exception(GetType().Name + " InstanceId = 0");
            NetworkManager.Instance().NetworkStream.SendData(InstanceId, data.DeliveryIntent, data.Data, peerIds.ToArray());
            data.Data.Dispose();
//            Tracer();
        }
        
        protected GameObject NetworkInstantiate(
            string     resourcesPath,
            Vector3    position = default(Vector3),
            Quaternion rotation = default(Quaternion)) =>
            NetworkManager.NetworkInstantiate(resourcesPath, position, rotation, true);

        protected void NetworkDestroy(bool destroyWholeObject)
        {
            if (NetworkManager == null)
            {
                if (destroyWholeObject)
                    Destroy(gameObject);
                else
                    Destroy(this);
                return;
            }

            NetworkManager.NetworkDestroy(InstanceId, destroyWholeObject);
        }

        private void Tracer()
        {
            var frames    = new StackTrace().GetFrames();
            var debugText = "";
            if (frames != null)
                for (int i = 2; i < frames.Length; i++)
                {
                    debugText += frames[i].GetMethod().Name;
                    if (i != frames.Length - 1) debugText += " >> ";
                    else debugText = frames[frames.Length - 1].GetFileName() + "::" + debugText;
                }
            
            Debug.Log("Trace: " + debugText);
        }
        #endregion

        #region rpc 
        // ReSharper disable once InconsistentNaming
        public void RPCInvoke(string methodName, RpcTargetTypes target, params object[] args)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new Exception("SendRpc -> methodName can't be empty!");

            if (NetworkManager.Instance() == null) return;

            var classCallName = GetType().FullName; 
            methodName = classCallName + "/" + methodName;

            if (!NetworkManager.Instance().MethodsWithRpc.ContainsKey(methodName))
                throw new Exception("Method: \"" + methodName + "\" not found. Missing \"RPCMethodAttribute\"?");

            var rpcMethodCallInfo = new RpcMethodCallInfo(methodName, args, InstanceId);

            if (target == RpcTargetTypes.Me)
                InvokeMethod(rpcMethodCallInfo);
            else if (target == RpcTargetTypes.All)
            {
                InvokeMethod(rpcMethodCallInfo);
                SendRpcBytes(rpcMethodCallInfo);
            }
            else if (target == RpcTargetTypes.Other)
                SendRpcBytes(rpcMethodCallInfo);
        }

        private void SendRpcBytes(RpcMethodCallInfo info)
        {
            // get op code and send protocol
            var opCode = (int) NetworkCommandTypes.Rpc;
            var intent = GameSparksRT.DeliveryIntent.RELIABLE;

            // convet struct to bytes and get stream
            var bytes  = new ArraySegment<byte>(info.ToByteArray());
            var stream = NetworkManager.Instance().NetworkStream;

            // send data
            stream.SendBytes(opCode, intent, bytes);
        }

        private void InvokeMethod(RpcMethodCallInfo info) =>
            NetworkManager.Instance().MethodsWithRpc[info.MethodName]
                          .Invoke(this, info.Arguments);
        #endregion

        #region interfaces
        public virtual void StartSync() { }
        public virtual void StopSync() { }

        public bool Equals(NetworkBehaviour other)
        {
            if (other == null) return false;
            
            return other._instanceId == _instanceId && 
                other.IsMine == IsMine &&
                other._networkOwner == _networkOwner;
        }
        #endregion
    }
}