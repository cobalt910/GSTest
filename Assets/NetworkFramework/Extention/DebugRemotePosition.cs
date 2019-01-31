using NetworkFramework.Core;
using NetworkFramework.Data;
using NetworkFramework.Managers;
using UnityEngine;

namespace GameSparks.NetworkFramework.Extention
{
    public class DebugRemotePosition : NetworkBehaviour
    {
        [Separator("Script Settings")]
        public Color Color;
        public float Radius;
        public bool UsePingCorrection = true;

        [SerializeField] private Vector3 _remotePosition;
        private Vector3 _previouslyPosition;
        private Vector3 _velocity;

        private void OnDrawGizmos()
        {
            if(IsMine) return;
            _remotePosition = _remotePosition == Vector3.zero ? transform.position : _remotePosition;
            Gizmos.color = Color;
            Gizmos.DrawWireSphere(_remotePosition, Radius);
        }

        private void Update()
        {
            if(!IsNetworkMatch || !GameSparksManager.Instance() || !IsMine) return;
            
            if (_previouslyPosition != transform.position)
            {
                _velocity           =  transform.position - _previouslyPosition;
                _velocity           /= Time.deltaTime;
                _previouslyPosition =  transform.position;

                SendNetworkPackage(new NetworkPackage()
                                  .SetVector3(transform.position)
                                  .SetVector3(_velocity)
                                  .SetInt(NetworkManager.Latency));
            }
        }

        public override void OnNetworkPackage(NetworkPackage package, int index)
        {
            var remotePosition = package.GetVector3();
            var remoteVelocity = package.GetVector3();
            var remotePing = package.GetInt();
            var localPing = NetworkManager.Latency;
            if(UsePingCorrection)
                _remotePosition = remotePosition + remoteVelocity * ((remotePing + localPing) / 1000f);
            else _remotePosition = remotePosition;
        }
    }
}
