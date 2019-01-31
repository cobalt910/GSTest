using System;
using GameSparks.Api.Messages;
using UnityEngine;

namespace NetworkFramework.Data
{
    [Serializable]
    public class NetworkPlayer : IEquatable<NetworkPlayer>
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _peerId;
        [SerializeField] private string _gamesparksId;
//        [SerializeField] private bool _isReady;
        [SerializeField] private bool _isOnline = true;
        
        #region incapsulate
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }

        public int PeerId
        {
            get => _peerId;
            set => _peerId = value;
        }

        public string GamesparksId
        {
            get => _gamesparksId;
            set => _gamesparksId = value;
        }

        /*public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }*/

        public bool IsOnline
        {
            get { return _isOnline; }
            set { _isOnline = value; }
        }
        #endregion

        public NetworkPlayer(MatchFoundMessage._Participant participant)
        {
            if(participant == null) return;
            _displayName  = participant.DisplayName;
            _peerId  = participant.PeerId.GetValueOrDefault();
            _gamesparksId = participant.Id;
        }

        public override string ToString() =>
            $"Name: {_displayName}, Index: {_peerId}, GameSparks ID: {_gamesparksId}";

        public bool Equals(NetworkPlayer player)
        {
            if (player == null) return false;
            return player._gamesparksId.Equals(_gamesparksId) &&
                   player._peerId.Equals(_peerId);

        }
    }
}