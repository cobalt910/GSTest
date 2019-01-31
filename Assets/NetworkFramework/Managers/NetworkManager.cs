using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.NetworkFramework.Extention;
using GameSparks.RT;
using NetworkFramework.Attributes;
using NetworkFramework.Attributes.EasyButtons;
using NetworkFramework.Core;
using NetworkFramework.Data;
using NetworkFramework.Enums;
using NetworkFramework.Parser.RTParserCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using DeliveryIntent = GameSparks.RT.GameSparksRT.DeliveryIntent;
using Random = UnityEngine.Random;
using Resources = UnityEngine.Resources;

namespace NetworkFramework.Managers
{
    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    public class NetworkManager : Singleton<NetworkManager>
    {
        #region variables
        [SerializeField] private GameSparksRTUnity _networkStream;
        [SerializeField] private List<NetworkBehaviour> _networkBehaviours = new List<NetworkBehaviour>();
        [SerializeField] private NetworkSessionInfo _networkSessionInfo;
        [SerializeField] private bool _receiveMessages = true;
        [SerializeField] private bool _isMatchOwner;
        [SerializeField] private bool _isDebug = true;
        [SerializeField] private Dictionary<string, GameObject> _spawnCache = new Dictionary<string, GameObject>();
        
        private Dictionary<string, MethodInfo> _methodsWithRpc = new Dictionary<string, MethodInfo>();
        private List<int> _ignoreInts = new List<int>();
        private List<int> _intsReserverdBySystem = new List<int>();
        private RTPacketParserUtility _rtParser = new RTPacketParserUtility();

        private const float SafeInjectTime = 1f;
        private const float PeriodicInjectTime = 0.2f;
        private bool _isConnected;

        public bool AllPlayersIsLoaded;
        public int  RealPlayersCount => _networkSessionInfo.PlayerList.Count;

        private DateTime _matchStartTime;
        #endregion

        #region incapsulate
        public GameSparksRTUnity NetworkStream
        {
            get { return _networkStream; }
            set { _networkStream = value; }
        }

        public List<NetworkBehaviour> NetworkBehaviours
        {
            get { return _networkBehaviours; }
            set { _networkBehaviours = value; }
        }

        public NetworkSessionInfo NetworkSessionInfo
        {
            get { return _networkSessionInfo; }
            set { _networkSessionInfo = value; }
        }

        public bool ReceiveMessages
        {
            get { return _receiveMessages; }
            set { _receiveMessages = value; }
        }

        public Dictionary<string, MethodInfo> MethodsWithRpc
        {
            get { return _methodsWithRpc; }
            set { _methodsWithRpc = value; }
        }

        public bool IsMatchOwner
        {
            get { return _isMatchOwner; }
            set { _isMatchOwner = value; }
        }
        
        public bool IsDebug
        {
            get { return _isDebug; }
            set { _isDebug = value; }
        }

        public List<int> IgnoreInts
        {
            get { return _ignoreInts; }
        }

        public List<int> IntsReserverdBySystem
        {
            get { return _intsReserverdBySystem; }
        }

        public RTPacketParserUtility RtParser
        {
            get { return _rtParser; }
        }

        public DateTime MatchStartTime
        {
            get => _matchStartTime;
        }
        #endregion

        #region initialize
        protected override void Awake()
        {
            base.Awake();
            foreach (var command in (NetworkCommandTypes[]) Enum.GetValues(typeof(NetworkCommandTypes)))
            {
                _ignoreInts.Add((int) command);
                _intsReserverdBySystem.Add((int) command);
            }
            
            // server instantiate id buffer
            for (int i = -10000; i < 0; i++)
                _ignoreInts.Add(i);
        }

        private void Start()
        {
            SaveAssemblies();
            SceneManager.sceneLoaded += (scene, mode) => NetworkEventManager.Instance().OnSceneLoadedInvoke(scene);
            NetworkEventManager.Instance().OnSceneLoaded += scene =>
            {
                _networkBehaviours.Clear();

                if (InstanceIdManager.Instance() == null)
                {
                    Debug.Log("========= InstanceIdManager could be found! scene.name: " + scene.name + " =========");
                    return;
                }

                var list = InstanceIdManager.Instance().NetworkBehaviours.Where(x => x != null).ToList();
                if (list.Count != 0) _networkBehaviours.AddRange(list);
            };
        }

        [Button]
        private void SaveAssemblies()
        {
            _methodsWithRpc.Clear();

            Assembly.GetAssembly(typeof(NetworkBehaviour))
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(NetworkBehaviour)))
                    .Select(x => new Tuple<Type, MethodInfo[]>(x, x.GetMethods().Where(m => m.GetCustomAttributes(typeof(RPCMethod), true).Length > 0).ToArray()))
                    .Where(x => x.Item2.Length > 0).ToList()
                    .ForEach(x => Array.ForEach(x.Item2, m => _methodsWithRpc.Add(x.Item1.FullName + "/" + m.Name, m)));
        }

        public void ConnectToRuntimeServer(NetworkSessionInfo info)
        {
            if(!ReferenceEquals(_networkStream, null)) return;
            Debug.Log("GSM| Creating New RT Session Instance...");
            _networkSessionInfo = info;
            _networkStream      = gameObject.AddComponent<GameSparksRTUnity>();

            var mockedResponse = new GSRequestData()
                                .AddNumber("port", _networkSessionInfo.PortId)
                                .AddString("host", _networkSessionInfo.HostUrl)
                                .AddString("accessToken", _networkSessionInfo.AccessToken);

            var matchResponse = new FindMatchResponse(mockedResponse);
            _networkStream.Configure(matchResponse, PlayerConnected, i => { /*this SHIT does`nt work!*/ }, RuntimeConnect, OnPacket);
            _networkStream.Connect();
        }

        public void Disconnect()
        {
            Debug.Log("Disconnect()");
            if (_networkStream != null)
            {
                _networkStream.Disconnect();
                Destroy(_networkStream);
            }

            StopPingUpdate();
            
            AllPlayersIsLoaded = false;
            _isConnected = false;
            _networkSessionInfo = null;
            _matchStartTime = default;
            NetworkEventManager.Instance().OnRuntimeDisconnectInvoke(); 
        }
        #endregion

        #region behaviours observe system
        public void SaveBehaviour(NetworkBehaviour b)
        {
            _networkBehaviours.Add(b);
        }

        public void DeleteBehaviour(NetworkBehaviour b)
        {
            if (_networkBehaviours == null || NetworkBehaviours.Count == 0 || !Instance() || !_networkStream) return;
            if (!_networkBehaviours.Exists(x => x != null && b != null && x.InstanceId == b.InstanceId))
            {
                Debug.LogError("Object with id " + b.InstanceId + " not found!");
                return;
            }

            var index = _networkBehaviours.FindIndex(x => x.InstanceId == b.InstanceId);
            _networkBehaviours.RemoveAt(index);
        }

        public NetworkBehaviour GetBehaviour(int instanceId)
        {
            return _networkBehaviours.Find(x => x.InstanceId == instanceId);
        }

        public T GetBehaviour<T>(int instanceId) where T : NetworkBehaviour
        {
            var networkBehaviour = _networkBehaviours.Find(x => x.InstanceId == instanceId);
            return networkBehaviour == null ? null : (T) networkBehaviour;
        }
        #endregion

        #region recive data
        private void PlayerConnected(int peerId)
        {
            if (!_receiveMessages) return;
            
            var connectedPlayer = NetworkSessionInfo.PlayerList.Find(x => x.PeerId == peerId);
            if(connectedPlayer == null) return;
            connectedPlayer.IsOnline = true;
            NetworkEventManager.Instance().OnRuntimePlayerConnectedInvoke(peerId, connectedPlayer); 
            if (_isDebug) Debug.Log("PlayerConnected: " + connectedPlayer);
        }

        private void PlayerDisconnected(int peerId)
        {
            if (!_receiveMessages) return;

            Debug.Log("PlayerDisconnected: Get Disconnected Player");
            var disconnectedPlayer = NetworkSessionInfo.PlayerList.Find(x => x != null && x.PeerId == peerId);

            if (disconnectedPlayer != null)
            {
                Debug.LogWarning("PlayerDisconnected: Inverse disconnected player status");
                disconnectedPlayer.IsOnline = false;
                NetworkEventManager.Instance()?.OnRuntimePlayerDisconnectedInvoke(peerId, disconnectedPlayer);
            }

            if (peerId == NetworkSessionInfo.MatchOwnerPeerId)
            {
                Debug.LogWarning("PlayerDisconnected: Sort players list");
                NetworkSessionInfo.PlayerList = NetworkSessionInfo.PlayerList.OrderBy(x => x.PeerId).ToList();

                Debug.LogWarning("PlayerDisconnected: get online player");
                var player = NetworkSessionInfo.PlayerList.Find(x => x.IsOnline);
                
                Debug.LogWarning("PlayerDisconnected: switch match owner");
                NetworkSessionInfo.MatchOwnerPeerId = player?.PeerId ?? NetworkSessionInfo.Player.PeerId;

                Debug.LogWarning("PlayerDisconnected: check match owner");
                if (NetworkSessionInfo.MatchOwnerPeerId == NetworkSessionInfo.Player.PeerId)
                {
                    Debug.LogWarning("PlayerDisconnected: root set");
                    NetworkEventManager.Instance().OnSetAsRootClientInvoke(); 
                    _isMatchOwner = true;
                }
                else
                {
                    Debug.LogWarning("PlayerDisconnected: root migrate");
                    NetworkEventManager.Instance().OnRootMigrateInvoke(NetworkSessionInfo.MatchOwnerPeerId); 
                }
            }
            if (_isDebug) Debug.Log("PlayerDisconnected: " + disconnectedPlayer ?? "null");
        }
        
        private void RuntimeConnect(bool status)
        {
            // game sparks is bullshit, this method sometimes invoke randomly
            if (_isDebug) Debug.Log("RuntimeConnect: " + status);
            if (!_receiveMessages) return;
            if (_isConnected || !status) return;
            _isConnected = true;

            _matchStartTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(90));
            
            TrackLevelLoading();
            StartPingUpdate();
            
            _isMatchOwner   = NetworkSessionInfo.Player.PeerId == NetworkSessionInfo.MatchOwnerPeerId;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            NetworkEventManager.Instance().OnRuntimeConnectedInvoke(status);
        }

        private void OnPacket(RTPacket packet)
        {
            if (!_receiveMessages) return;
            _lastPackageReceived = DateTime.UtcNow;
            
            if (_rtParser.Contains(packet.OpCode))
            {
                _rtParser.ExecuteParser(packet);
                return;
            }
            
            var command = (NetworkCommandTypes) packet.OpCode;
            
            switch (command)
            {
                case NetworkCommandTypes.Instantiate:
                    if (_isDebug) Debug.Log("<color=green>Network Instantiate!</color>");
                    NetworkInstantiate(packet.Data, packet.Sender);
                    return;
                case NetworkCommandTypes.Destroy:
                    if (_isDebug) Debug.Log("<color=orange>Network Destroy!</color>");
                    NetworkDestroy(packet.Data);
                    return;
                case NetworkCommandTypes.Rpc:
                    if (_isDebug) Debug.Log("<color=yellow>Network Rpc!</color>");
                    ParseRpc(packet);
                    return;
                case NetworkCommandTypes.UserDisconnected:
                    var peerId = packet.Data.GetInt(1);
                    if(peerId != null) PlayerDisconnected(peerId.GetValueOrDefault());
                    else Debug.LogError("RootClientSetException: packet is broken!");
                    return;
                case NetworkCommandTypes.Ping:
                    PingResponse(packet);
                    return;
                case NetworkCommandTypes.SyncClock:
                    SyncClock(packet);
                    return;
            }
            
            if (Enum.IsDefined(typeof(NetworkCommandTypes), packet.OpCode))
            {
                if (_isDebug) Debug.Log("Get Unhandled Message");
                NetworkEventManager.Instance().OnGetRtServerMessageInvoke(packet);
                return;
            }

            StartCoroutine(TryInjectPacket(packet));
        }

        private IEnumerator TryInjectPacket(RTPacket packet)
        {
            float timeRef = SafeInjectTime; // 1f
            while (timeRef > 0)
            {
                var script = _networkBehaviours.Find(x => x.InstanceId == packet.OpCode);

                if (ReferenceEquals(script, null))
                {
                    timeRef -= PeriodicInjectTime; // -.2f
                    yield return new WaitForSecondsRealtime(PeriodicInjectTime);
                    continue;
                }
                
                if (script.ReciveNetworkMessage)
                    script.OnNetworkPackage(new NetworkPackage(packet.Data), packet.Data.GetInt(1).GetValueOrDefault());
                else
                {
                    Debug.LogError("Object: " + packet.OpCode + "disable message recive!");
                    yield break;
                }
                yield break;
            }
        }
        #endregion

        #region instance id
        private int GetUniqueInstanceId()
        {
            while (true)
            {
                var value = Random.Range(0, 1000000000);
                if (_ignoreInts.Contains(value) || _networkBehaviours.Exists(x =>
                {
                    if (x == null) return false;
                    return x.InstanceId == value;
                })) continue;
                return value;
            }
        }
        #endregion

        #region network instantiate and destroy
        private static void NetworkInstantiate(RTData data, int sender)
        {
            var resourcesPath = data.GetString(1);
            var arrayLength   = data.GetInt(2).GetValueOrDefault();
            var ids           = new int[arrayLength];
            var peers         = new int[arrayLength];

            for (var i = 0; i < arrayLength; i++)
            {
                ids[i]   = data.GetInt((uint) (i + 3)).GetValueOrDefault();
                peers[i] = data.GetInt((uint) (i + 4)).GetValueOrDefault();
            }

            var offset            = (uint) (arrayLength + 5);
            var position          = data.GetVector3(offset++).GetValueOrDefault();
            var rotation          = Quaternion.Euler(data.GetVector3(offset).GetValueOrDefault());
            var obj               = NetworkInstantiate(resourcesPath, position, rotation, false);
            var networkBehaviours = obj.GetComponentsInChildren<NetworkBehaviour>(true);

            if (networkBehaviours.Length != arrayLength)
            {
                Debug.LogError("<color=red>Counts of components on peer-to-peer objects not equals!</color>");
                return;
            }

            for (var i = 0; i < arrayLength; i++)
            {
                networkBehaviours[i].InstanceId   = ids[i];
                networkBehaviours[i].NetworkOwner = Instance()._networkSessionInfo.GetPlayerByPeerId(sender);
                networkBehaviours[i].OwnerType = NetworkBehaviour.NetworkOwnerType.Remote;
                Instance().SaveBehaviour(networkBehaviours[i]);
            }

            if(Instance()._isDebug) Debug.Log("<color=green>Object Spawn Success</color>");
            NetworkEventManager.Instance().OnNetworkInstantiateInvoke(resourcesPath, obj);//?.Invoke(resourcesPath, obj);
        }

        private static void NetworkDestroy(RTData data)
        {
            var instanceId         = data.GetInt(1).GetValueOrDefault();
            var destroyWholeObject = data.GetInt(2).GetValueOrDefault() == 1;
            NetworkEventManager.Instance().OnNetworkDestroyInvoke(instanceId, destroyWholeObject);//?.Invoke(instanceId, destroyWholeObject);
            NetworkDestroy(instanceId, destroyWholeObject, false);
        }

        public static GameObject NetworkInstantiate(
            string     resourcesPath,
            Vector3    position      = default(Vector3),
            Quaternion rotation      = default(Quaternion),
            bool       invokeNetwork = true)
        {
            var obj = Instantiate(GetObjectFromCache(resourcesPath), position, rotation);

            if (obj == null)
            {
                Debug.LogError("Invalid Path: " + resourcesPath + ", Object Not Found!");
                return null;
            }

            if (!invokeNetwork || Instance() == null || Instance()._networkStream == null)
                return obj;

            var networkBehaviours = obj.GetComponentsInChildren<NetworkBehaviour>(true);

            foreach (var behaviour in networkBehaviours)
            {
                behaviour.InstanceId   = Instance().GetUniqueInstanceId();
                behaviour.OwnerType    = NetworkBehaviour.NetworkOwnerType.Mine;
                behaviour.NetworkOwner = Instance().NetworkSessionInfo.Player;
                Instance().SaveBehaviour(behaviour);
            }

            using (var data = RTData.Get())
            {
                data.SetString(1, resourcesPath);
                data.SetInt(2, networkBehaviours.Length);

                for (var i = 0; i < networkBehaviours.Length; i++)
                {
                    data.SetInt((uint) (i + 3), networkBehaviours[i].InstanceId);
                    data.SetInt((uint) (i + 4), networkBehaviours[i].NetworkOwner.PeerId);
                }

                var offset = (uint) (networkBehaviours.Length + 5);
                data.SetVector3(offset++, position);
                data.SetVector3(offset, rotation.eulerAngles);
                Instance().NetworkStream.SendData((int) NetworkCommandTypes.Instantiate, DeliveryIntent.RELIABLE, data);
            }

            return obj;
        }

        public static void NetworkDestroy(int instanceId, bool destroyWholeObject, bool invokeNetwork = true)
        {
            if (Instance() == null) throw new Exception("Network Manager is NULL, object not be destroyed.");

            var obj = Instance()._networkBehaviours.Find(x => x.InstanceId == instanceId);

            if (obj != null && invokeNetwork)
            {
                using (var data = RTData.Get())
                {
                    data.SetInt(1, obj.InstanceId);
                    data.SetInt(2, destroyWholeObject ? 1 : 0);
                    Instance().NetworkStream.SendData((int) NetworkCommandTypes.Destroy, DeliveryIntent.RELIABLE, data);
                }
            }

            if (obj != null && destroyWholeObject)
                Destroy(obj.gameObject);
            else if (obj != null)
                Destroy(obj);
        }

        private static GameObject GetObjectFromCache(string path)
        {
            if (Instance() == null) return Resources.Load<GameObject>(path);
            if (Instance()._spawnCache.ContainsKey(path)) return Instance()._spawnCache[path];
            var obj = Resources.Load<GameObject>(path);
            if(obj == null) throw new Exception("Path Incorrect!");
            Instance()._spawnCache.Add(path,obj);
            return obj;
        }

        public static void PreloadObjects(params string[] paths)
        {
            if (Instance() == null)
            {
                Debug.LogError("Cannot cache objects, NetworkManager is null.");
                return;
            }

            for (int i = 0; i < paths.Length; i++)
            {
                var obj = Resources.Load<GameObject>(paths[i]);
                if(obj == null) throw new Exception("Path Incorrect!");
                
                if(Instance()._spawnCache.ContainsKey(paths[i])) continue;
                Instance()._spawnCache.Add(paths[i], obj);
            }
        }
        #endregion

        #region network rpc
        private void ParseRpc(RTPacket packet)
        {
            var deserializeInfo = packet.Stream.ReadFully();
            var methodCallInfo  = RpcMethodCallInfo.FromByteArray(deserializeInfo);
            var script          = _networkBehaviours.Find(x => x.InstanceId == methodCallInfo.InstanceId);

            if (script != null)
            {
                var key = methodCallInfo.MethodName;

                if (_methodsWithRpc.ContainsKey(key))
                {
                    var method     = _methodsWithRpc[key];
                    var parameters = method.GetParameters();

                    if (methodCallInfo.Arguments.Length != parameters.Length)
                        throw new Exception("RPC| Invalid count of arguments!");

                    var args = new object[parameters.Length];

                    for (var i = 0; i < methodCallInfo.Arguments.Length; i++)
                    {
                        var arg    = methodCallInfo.Arguments[i];
                        var toType = parameters[i].ParameterType;
                        args[i] = Convert.ChangeType(arg, toType);
                    }

                    method.Invoke(script, args);
                }
                else throw new Exception("Object: \"" + key + "\" is missing!");
            }
            else Debug.LogError("Object: " + methodCallInfo.InstanceId + " missing!");
        }
        #endregion

        #region track level loading
        private void TrackLevelLoading()
        {
            if(_trackLevelLoad != null) return;
            StartCoroutine(_trackLevelLoad = TrackLevelLoad());
        }

        public void AbortTrackLevelLoading() // would be invoke when match level loaded
        {
            if (_trackLevelLoad == null) return;
            StopCoroutine(_trackLevelLoad);
            _trackLevelLoad = null;
        }

        private IEnumerator _trackLevelLoad;
        private IEnumerator TrackLevelLoad()
        {
            while (true)
            {
                var timeToStartMatch = (_matchStartTime - DateTime.UtcNow).TotalSeconds;
                if (timeToStartMatch <= 6)
                {
                    NetworkEventManager.Instance().OnTooLongLoadBattleLevelInvoke();
                    yield break;
                }
                yield return null;
            }
        }
        #endregion

        #region ping
        [Header("Ping Settings")]
        [SerializeField] private float _pingUpdateRateSec = 0.2f;
        [SerializeField] private int _roundTrip;
        [SerializeField] private int _latency;
        [SerializeField] private int _localLatency;
        [SerializeField] private int _serverTimeDelta;
        [SerializeField] private DateTime _serverClock;
        [SerializeField] private DateTime _lastPackageReceived;

        public int RoundTrip => _roundTrip;
        public int Latency => _latency;
        public int LocalLatency => _localLatency;
        public int ServerTimeDelta => _serverTimeDelta;
        public DateTime ServerClock => _serverClock;

        public void StartPingUpdate()
        {
            _lastPackageReceived = DateTime.UtcNow;
            StopPingUpdate();
            StartCoroutine(_pingUpdateLoop = PingUpdateLoop());
        }

        public void StopPingUpdate()
        {
            if (_pingUpdateLoop != null)
            {
                StopCoroutine(_pingUpdateLoop);
                _pingUpdateLoop = null;
            }
        }

        private IEnumerator _pingUpdateLoop;
        private IEnumerator PingUpdateLoop()
        {
            while (true)
            {
                PingRequest();
//                if (!Instance().AllPlayersIsLoaded) _lastPackageReceived = DateTime.UtcNow;
                _localLatency = (int)(DateTime.UtcNow - _lastPackageReceived).TotalMilliseconds;
                if (_localLatency >= 20000) NetworkEventManager.Instance().OnRuntimeDisconnectByPingInvoke(_localLatency);
                yield return new WaitForSecondsRealtime(_pingUpdateRateSec);
            }
        }

        private void PingRequest()
        {
            using (var data = RTData.Get())
            {
                data.SetLong(1, (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                _networkStream.SendData((int) NetworkCommandTypes.Ping, DeliveryIntent.UNRELIABLE, data, 0);
            }
        }

        private void PingResponse(RTPacket packet)
        {
            var currentTime = (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
            _roundTrip = (int) (currentTime - packet.Data.GetLong(1).GetValueOrDefault());
            _latency   = _roundTrip / 2;
            var serverDelta = (int) (packet.Data.GetLong(2).GetValueOrDefault() - currentTime);
            _serverTimeDelta = serverDelta + _latency;
            _lastPackageReceived = DateTime.UtcNow;
        }

        private void SyncClock(RTPacket packet)
        {
            var dateNow    = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var serverTime = packet.Data.GetLong(1).GetValueOrDefault();
            _serverClock = dateNow.AddMilliseconds(serverTime + _serverTimeDelta).ToLocalTime();
            _lastPackageReceived = DateTime.UtcNow;
        }
        
        /*void OnGUI()
        {
            if(ReferenceEquals(_networkStream, null)) return;
            GUI.Label(new Rect(10, 50, 400, 30), "Server Time: " + _serverClock.TimeOfDay);
            GUI.Label(new Rect(10, 70, 400, 30), "Latency: " + _latency + "ms");
            GUI.Label(new Rect(10, 90, 400, 30), "Local Latency: " + _localLatency + "ms");
            GUI.Label(new Rect(10, 110, 400, 30), "Time Delta: " + _serverTimeDelta + "ms");
        }*/
        #endregion
    }
}
