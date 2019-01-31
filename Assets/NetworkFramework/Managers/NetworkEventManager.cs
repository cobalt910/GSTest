using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GameSparks.Api.Messages;
using GameSparks.Api.Responses;
using GameSparks.RT;
using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkPlayer = NetworkFramework.Data.NetworkPlayer;

namespace NetworkFramework.Managers
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public partial class NetworkEventManager : Singleton<NetworkEventManager>
    {
        #region http events
        /// <summary>
        /// Call when GameSpasrks successful connect plugin to HTTP Server.
        /// </summary>
        public event Action GsAvaliable;
        public virtual void OnGsAvaliableInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs = GsAvaliable?.GetInvocationList();
                var debugStr = "GsAvaliable InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            GsAvaliable?.Invoke();
        }

        /// <summary>
        /// Call when GameSpasrks failed connect plugin to HTTP Server.
        /// </summary>
        public event Action GsNotAvaliable;
        public virtual void OnGsNotAvaliableInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = GsNotAvaliable?.GetInvocationList();
                var debugStr = "GsNotAvaliable InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            GsNotAvaliable?.Invoke();
        }

        /// <summary>
        /// Call when user auth.
        /// </summary>
        public event Action<AuthenticationResponse> OnAuthenticated;
        public virtual void OnAuthenticatedInvoke(AuthenticationResponse x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnAuthenticated?.GetInvocationList();
                var debugStr = "OnAuthenticated InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnAuthenticated?.Invoke(x);
        }

        /// <summary>
        /// Call when user register.
        /// </summary>
        public event Action<RegistrationResponse> OnRegistered;
        public virtual void OnRegisteredInvoke(RegistrationResponse x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRegistered?.GetInvocationList();
                var debugStr = "OnRegistered InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRegistered?.Invoke(x);
        }

        /// <summary>
        /// Call when user start search match.
        /// </summary>
        public event Action OnStartSearchMatch;
        public virtual void OnStartSearchMatchInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnStartSearchMatch?.GetInvocationList();
                var debugStr = "OnStartSearchMatch InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnStartSearchMatch?.Invoke();
        }

        /// <summary>
        /// Call when user is cancel search match.
        /// </summary>
        public event Action OnCancelSearchMatch;
        public virtual void OnCancelSearchMatchInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnCancelSearchMatch?.GetInvocationList();
                var debugStr = "OnCancelSearchMatch InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnCancelSearchMatch?.Invoke();
        }

        /// <summary>
        /// Call when match not found -_-
        /// </summary>
        public event Action<MatchFoundMessage> OnMatchFound;
        public virtual void OnMatchFoundInvoke(MatchFoundMessage x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnMatchFound?.GetInvocationList();
                var debugStr = "OnMatchFound InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnMatchFound?.Invoke(x);
        }

        /// <summary>
        /// Call when match info is updated. This info update only when you in search match.
        /// </summary>
        public event Action<MatchUpdatedMessage> OnMatchInfoUpdated;
        public virtual void OnMatchInfoUpdatedInvoke(MatchUpdatedMessage x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnMatchInfoUpdated?.GetInvocationList();
                var debugStr = "OnMatchInfoUpdated InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnMatchInfoUpdated?.Invoke(x);
        }

        /// <summary>
        /// Call when match is found.
        /// *arg - store some data (like a port, host, etc.) for create GameSparks RT Connection.
        /// </summary>
        public event Action<MatchNotFoundMessage> OnMatchNotFound;
        public virtual void OnMatchNotFoundInvoke(MatchNotFoundMessage x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnMatchNotFound?.GetInvocationList();
                var debugStr = "OnMatchNotFound InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnMatchNotFound?.Invoke(x);
        }

        /// <summary>
        /// Call when recive some message from GameSparks HTTP Server.
        /// </summary>
        public event Action<ScriptMessage> OnHttpMessageRecived;
        public virtual void OnHttpMessageRecivedInvoke(ScriptMessage x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnHttpMessageRecived?.GetInvocationList();
                var debugStr = "OnHttpMessageRecived InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnHttpMessageRecived?.Invoke(x);
        }

        /// <summary>
        /// Call when GameSparks HTTP Server kill you connection.
        /// Basically it is called when someone enters an account from another device.
        /// </summary>
        public event Action<SessionTerminatedMessage> OnHttpServerDisconnect;
        public virtual void OnHttpServerDisconnectInvoke(SessionTerminatedMessage x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnHttpServerDisconnect?.GetInvocationList();
                var debugStr = "OnHttpServerDisconnect InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnHttpServerDisconnect?.Invoke(x);
        }
        #endregion

        #region runtime events
        /// <summary>
        /// Call when you connected to GameSparks RT Server.
        /// *arg - connection is succes?
        /// </summary>
        public event Action<bool> OnRuntimeConnected;
        public virtual void OnRuntimeConnectedInvoke(bool x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRuntimeConnected?.GetInvocationList();
                var debugStr = "OnRuntimeConnected InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRuntimeConnected?.Invoke(x);
        }

        /// <summary>
        /// Call when pint >= 1000ms (1sec).
        /// *arg - ping amount
        /// </summary>
        public event Action<int> OnRuntimeDisconnectByPing;
        public virtual void OnRuntimeDisconnectByPingInvoke(int x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRuntimeDisconnectByPing?.GetInvocationList();
                var debugStr = "OnRuntimeDisconnectByPing InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRuntimeDisconnectByPing?.Invoke(x);
        }

        /// <summary>
        /// Call when you disconnected from runtime server.
        /// </summary>
        public event Action OnRuntimeDisconnect;
        public virtual void OnRuntimeDisconnectInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRuntimeDisconnect?.GetInvocationList();
                var debugStr = "OnRuntimeDisconnect InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRuntimeDisconnect?.Invoke();
        }

        /// <summary>
        /// Call when someone connected to GameSparks RT Server.
        /// *arg0 - peer id
        /// *arg1 - some player info
        /// </summary>
        public event Action<int, NetworkPlayer> OnRuntimePlayerConnected;
        public virtual void OnRuntimePlayerConnectedInvoke(int x, NetworkPlayer y)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRuntimePlayerConnected?.GetInvocationList();
                var debugStr = "OnRuntimePlayerConnected InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRuntimePlayerConnected?.Invoke(x, y);
        }

        /// <summary>
        /// Call when someone disconnected in GameSparks RT Server.
        /// *arg0 - peer id
        /// *arg1 - some player info
        /// </summary>
        public event Action<int, NetworkPlayer> OnRuntimePlayerDisconnected;
        public virtual void OnRuntimePlayerDisconnectedInvoke(int x, NetworkPlayer y)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRuntimePlayerDisconnected?.GetInvocationList();
                var debugStr = "OnRuntimePlayerDisconnected InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRuntimePlayerDisconnected?.Invoke(x, y);
        }

        /// <summary>
        /// Call when recive specific network message.
        /// *arg0 - resources path
        /// *arg1 - instance of spawn object
        /// </summary>
        public event Action<string, GameObject> OnNetworkInstantiate;
        public virtual void OnNetworkInstantiateInvoke(string x, GameObject y)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnNetworkInstantiate?.GetInvocationList();
                var debugStr = "OnNetworkInstantiate InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnNetworkInstantiate?.Invoke(x, y);
        }

        /// <summary>
        /// Call when recive specific network msg.
        /// Call before object will be destroyed.
        /// *arg0 - objectInstanceId 
        /// *arg1 - destroyWholeObject?
        /// </summary>
        public event Action<int, bool> OnNetworkDestroy;
        public virtual void OnNetworkDestroyInvoke(int x, bool y)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnNetworkDestroy?.GetInvocationList();
                var debugStr = "OnNetworkDestroy InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnNetworkDestroy?.Invoke(x, y);
        }

        public event Action<int> OnRootMigrate;
        public virtual void OnRootMigrateInvoke(int x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnRootMigrate?.GetInvocationList();
                var debugStr = "OnRootMigrate InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnRootMigrate?.Invoke(x);
        }

        /// <summary>
        /// Call when toy set as main client (like a server in peer-to-peer connection)
        /// </summary>
        public event Action OnSetAsRootClient;
        public virtual void OnSetAsRootClientInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnSetAsRootClient?.GetInvocationList();
                var debugStr = "OnSetAsRootClient InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnSetAsRootClient?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public event Action<RTPacket> OnGetRtServerMessage;
        public virtual void OnGetRtServerMessageInvoke(RTPacket x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnGetRtServerMessage?.GetInvocationList();
                var debugStr = "OnGetRtServerMessage InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnGetRtServerMessage?.Invoke(x);
        }
        #endregion

        /// <summary>
        /// Just for help =)
        /// </summary>
        public event Action<Scene> OnSceneLoaded;
        public virtual void OnSceneLoadedInvoke(Scene x)
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnSceneLoaded?.GetInvocationList();
                var debugStr = "OnSceneLoaded InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnSceneLoaded?.Invoke(x);
        }

        public event Action OnTooLongLoadBattleLevel;
        public virtual void OnTooLongLoadBattleLevelInvoke()
        {
            if (GameSparksManager.Instance().IsDebug)
            {
                var subs     = OnTooLongLoadBattleLevel?.GetInvocationList();
                var debugStr = "OnTooLongLoadBattleLevel InvokationList: ";
                subs?.ToList().ForEach(m => debugStr += "\n" + m.Method.Name);
                Debug.Log(debugStr);
            }
            OnTooLongLoadBattleLevel?.Invoke();
        }
    }
}
