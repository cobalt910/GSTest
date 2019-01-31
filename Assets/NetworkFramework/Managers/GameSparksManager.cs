using System;
using System.Diagnostics.CodeAnalysis;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using NetworkFramework.Data;
using NetworkFramework.Parser.ServerMessageParser;
using NetworkFramework.RequestSystem;
using Newtonsoft.Json;
using UnityEngine;
using ScriptMessageParserUtility = NetworkFramework.Parser.ScriptMessageParserCore.ScriptMessageParserUtility;

namespace NetworkFramework.Managers
{
    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    public sealed partial class GameSparksManager : Singleton<GameSparksManager>
    {
        #region variables
        [SerializeField] private GameSparksSettings _settings;

        [SerializeField] private string _gameSparksId;
        [SerializeField] private string _gameSparksDisplayName;

        [SerializeField] private bool _isDebug = true;
        [SerializeField] private bool _isSearchingBattle;

        // maybe unused
        public const int PlayerSkill = 2;

        public ScriptMessageParserUtility ScriptMessageParserUtility = new ScriptMessageParserUtility();
        public ServerMessageParserUtility ServerMessageParserUtility = new ServerMessageParserUtility();
        #endregion

        #region incapsulate
        public GameSparksSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public bool IsDebug
        {
            get { return _isDebug; }
            set { _isDebug = value; }
        }

        public string GameSparksId
        {
            get { return _gameSparksId; }
            set { _gameSparksId = value; }
        }

        public string GameSparksDisplayName
        {
            get { return _gameSparksDisplayName; }
            set { _gameSparksDisplayName = value; }
        }

        public bool IsSearchingBattle
        {
            get { return _isSearchingBattle; }
            set { _isSearchingBattle = value; }
        }
        #endregion

        #region intialize
        protected override void Awake()
        {
            base.Awake();
            gameObject.AddComponent<GameSparksUnity>().settings = _settings;
            gameObject.AddComponent<NetworkManager>().IsDebug = _isDebug;
            if(NetworkEventManager.Instance() == null)
                DontDestroyOnLoad(new GameObject(nameof(NetworkEventManager)).AddComponent<NetworkEventManager>());

            GS.GameSparksAvailable = null;
            GS.GameSparksAvailable += isAvailable =>
            {
                Debug.Log("GSM| Status: " + (isAvailable ? "Is Online" : "Is Offline"));
                if (isAvailable) NetworkEventManager.Instance().OnGsAvaliableInvoke();
                if (!isAvailable) NetworkEventManager.Instance().OnGsNotAvaliableInvoke();
            };

            SessionTerminatedMessage.Listener = null;
            SessionTerminatedMessage.Listener += x =>
            {
                Debug.Log("GSM| Disconnect By GS Server");
                if (IsDebug) Debug.Log(x.JSONString);
                NetworkEventManager.Instance().OnHttpServerDisconnectInvoke(x);
                Debug.LogError("Ops, session is closed =)");
                GS.Disconnect();
            };

            ScriptMessage.Listener = null;
            ScriptMessage.Listener += x =>
            {
                Debug.Log("GSM| Get Server Message");
                if (IsDebug) Debug.Log(x.JSONString);
                if(!ScriptMessageParserUtility.Contains(x.ExtCode))
                    NetworkEventManager.Instance().OnHttpMessageRecivedInvoke(x);
                else ScriptMessageParserUtility.Parse(x);

                if (ServerMessageParserUtility.Contains(x.ExtCode))
                    ServerMessageParserUtility.Parse(x);
            };

            MatchFoundMessage.Listener = null;
            MatchFoundMessage.Listener += x =>
            {
                Debug.Log("GSM| Match Found!");
                if (IsDebug) Debug.Log(x.JSONString);
                NetworkManager.Instance().ConnectToRuntimeServer(new NetworkSessionInfo(x));
                NetworkEventManager.Instance().OnMatchFoundInvoke(x); 
            };

            MatchUpdatedMessage.Listener = null;
            MatchUpdatedMessage.Listener += x =>
            {
                Debug.Log("GSM| Match Info Updated!");
                if (IsDebug) Debug.Log(x.JSONString);
                NetworkEventManager.Instance().OnMatchInfoUpdatedInvoke(x);
            };

            MatchNotFoundMessage.Listener = null;
            MatchNotFoundMessage.Listener += x =>
            { 
                Debug.Log("GSM| Match Not Found!");
                if (IsDebug) Debug.Log(x.JSONString);
                NetworkEventManager.Instance().OnMatchNotFoundInvoke(x);
            };
        }
        #endregion

        #region login system
        public void LoginUser(string userName, string password, Action<RegistrationResponse> reg, Action<AuthenticationResponse> auth)
        {
            reg += OnRegister;
            auth += OnAuthenticated;
            RegistrationSystem.AuthenticateUser(userName, password, reg, auth);
        }

        private void OnRegister(RegistrationResponse response)
        {
            OnRegOrAuth(response.UserId, response.DisplayName);
            if (IsDebug) Debug.Log(response.JSONString);
            NetworkEventManager.Instance().OnRegisteredInvoke(response);
        }

        private void OnAuthenticated(AuthenticationResponse response)
        {
            OnRegOrAuth(response.UserId, response.DisplayName);
            if (IsDebug) Debug.Log(response.JSONString);
            NetworkEventManager.Instance().OnAuthenticatedInvoke(response);
        }

        private void OnRegOrAuth(string gsId, string userName)
        {
            // save some data
            GameSparksId          = gsId;
            GameSparksDisplayName = userName;
        }
        #endregion

        #region matchmaking system
        public void FindMatch(string battleShortCode, bool isCancel = false)
        {
            Debug.Log(isCancel ? "GSM| Cancel Matchmaking!" : "GSM| Attempting Matchmaking...");
            var matchRequest = new MatchmakingRequest();
            matchRequest.SetMatchShortCode(battleShortCode);
            matchRequest.SetSkill(PlayerSkill);
            IsSearchingBattle = !isCancel;

            if (isCancel)
            {
                matchRequest.SetAction("cancel");
                NetworkEventManager.Instance().OnCancelSearchMatchInvoke();//?.Invoke();
            }
            else NetworkEventManager.Instance().OnStartSearchMatchInvoke();//?.Invoke();

            matchRequest.Send(x =>
            {
                if (x.HasErrors) throw new Exception("GSM| MatchMaking Error \n" + x.Errors.JSON);
                if (IsDebug) Debug.Log(x.JSONString);
            });
        }
        #endregion

        #region http messaging
        public void SendDataToPlayer<T>(string targetId, string extCode, T msg, Action onGet)
        {
            var request = CreateRequest("SendDataToPlayer");
            request.SetEventAttribute("targetId", targetId);
            request.SetEventAttribute("extCode", extCode);
            request.SetEventAttribute("data", JsonConvert.SerializeObject(msg));
            request.Send(x =>
            {
                if(x.HasErrors) Debug.Log(x.Errors.JSON);
                onGet?.Invoke();
            });
        }
        #endregion

        #region block ui
        public GameObject UiBlocker;
        public void LockUi()
        {
            if (!UiBlocker.activeInHierarchy)
            {
                if(_isDebug) Debug.Log("Ui is locked");
                UiBlocker.SetActive(true);
            }
        }

        public void UnlockUi()
        {
            if(_isDebug) Debug.Log("Ui is unlocked");
            if(UiBlocker.activeInHierarchy) UiBlocker.SetActive(false);            
        }
        #endregion
        
        public LogEventRequest CreateRequest(string eventKey) =>
            new LogEventRequest().SetEventKey(eventKey);

        #if UNITY_ANDROID
        private DateTime _pauseStart;
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) _pauseStart = DateTime.UtcNow;
            else if(_pauseStart == default) _pauseStart = DateTime.UtcNow;
            else if((DateTime.UtcNow - _pauseStart).TotalSeconds > 10) Application.Quit();
        }
        #endif
    }
}

