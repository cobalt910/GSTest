using System;
using System.Collections.Generic;
using System.Linq;
using GameSparks.Api.Messages;
using NetworkFramework.Managers;
using UnityEngine;

namespace NetworkFramework.Data
{
    [Serializable]
    public class NetworkSessionInfo
    {
        [SerializeField] private string _hostUrl;
        [SerializeField] private string _accessToken;
        [SerializeField] private int _portId;
        [SerializeField] private string _matchId;
        [SerializeField] private List<NetworkPlayer> _playerList = new List<NetworkPlayer>();
        [SerializeField] private NetworkPlayer _player;
        [SerializeField] private int _matchOwnerPeerId = 1;
        [SerializeField] private MatchFoundMessage _matchFoundMessage;
        
        #region incapsulate
        public string HostUrl
        {
            get { return _hostUrl; }
            set { _hostUrl = value; }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        public int PortId
        {
            get { return _portId; }
            set { _portId = value; }
        }

        public string MatchId
        {
            get { return _matchId; }
            set { _matchId = value; }
        }

        public List<NetworkPlayer> PlayerList
        {
            get { return _playerList; }
            set { _playerList = value; }
        }

        public NetworkPlayer Player
        {
            get { return _player; }
            set { _player = value; }
        }

        public int MatchOwnerPeerId
        {
            get { return _matchOwnerPeerId; }
            set { _matchOwnerPeerId = value; }
        }

        public MatchFoundMessage MatchFoundMessage
        {
            get { return _matchFoundMessage; }
            set { _matchFoundMessage = value; }
        }
        #endregion

        public NetworkSessionInfo(MatchFoundMessage message)
        {
            _matchFoundMessage = message;
            _matchId     = message.MatchId;
            _hostUrl     = message.Host;
            _accessToken = message.AccessToken;

            if (message.Port != null)
                _portId = message.Port.Value;
            else
                Debug.LogError("GameSparks doesent has portId!");

            foreach (var participant in message.Participants)
            {
                if (participant.PeerId != null)
                {
                    var player = new NetworkPlayer(participant);
                    if (player.GamesparksId == GameSparksManager.Instance().GameSparksId)
                        _player = player;
                    _playerList.Add(player);
                }
                else Debug.LogError("GameSparks doesnt has peerId!");
            }
        }

        public NetworkPlayer GetPlayerByPeerId(int peerId)
        {
            if (peerId == 0)
                return new NetworkPlayer(null)
                {
                    DisplayName  = "Server",
                    GamesparksId = "0",
                    IsOnline     = true,
//                    IsReady      = true,
                    PeerId       = 0
                };
            return _playerList.Where(x => x.PeerId == peerId).ElementAt(0);
        }
    }
}