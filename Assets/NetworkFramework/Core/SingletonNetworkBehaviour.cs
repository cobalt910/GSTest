using UnityEngine;

namespace NetworkFramework.Core
{
    public class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : Object
    {
        #region singleton
        private static T _instance;
        public static T Instance()
        {
            if (_instance == null)
                _instance = FindObjectOfType<T>();
            return _instance;
        }

        protected virtual void Awake()
        {
            _instance = FindObjectOfType<T>();
        }
        #endregion
    }
}
