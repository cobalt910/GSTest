using System;
using GameSparks.RT;
using UnityEngine;

namespace NetworkFramework.Data
{
    public class NetworkPackage
    {
        #region vatiables
        private RTData                      _data;
        private GameSparksRT.DeliveryIntent _deliveryIntent;
        private uint                        _indexer = 2;
        #endregion

        #region incapsulate
        public RTData Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public GameSparksRT.DeliveryIntent DeliveryIntent
        {
            get { return _deliveryIntent; }
            set { _deliveryIntent = value; }
        }

        public uint Indexer
        {
            get { return _indexer; }
            set { _indexer = value; }
        }
        #endregion

        #region constructors
        public NetworkPackage(GameSparksRT.DeliveryIntent deliveryIntent = GameSparksRT.DeliveryIntent.RELIABLE, int packageIndex = 0)
        {
            _deliveryIntent = deliveryIntent;
            _data           = RTData.Get();
            _data.SetInt(1, packageIndex);
        }

        public NetworkPackage(RTData data)
        {
            _data = data;
        }
        #endregion
        
        #region setters
        public NetworkPackage SetInt(int value)
        {
            _data.SetInt(_indexer++, value);
            return this;
        }

        public NetworkPackage SetFloat(float value)
        {
            _data.SetFloat(_indexer++, value);
            return this;
        }
        
        public NetworkPackage SetString(string value)
        {
            _data.SetString(_indexer++, value);
            return this;
        }

        public NetworkPackage SetVector2(Vector2 value)
        {
            _data.SetVector2(_indexer++, value);
            return this;
        }

        public NetworkPackage SetVector3(Vector3 value)
        {
            _data.SetVector3(_indexer++, value);
            return this;
        }

        public NetworkPackage SetVector4(Vector4 value)
        {
            _data.SetVector4(_indexer++, value);
            return this;
        }
        #endregion

        #region getters
        public int GetInt()
        {
            var value = _data.GetInt(_indexer++);

            if (value != null)
                return value.Value;
            throw new Exception("NetworkPacket - GetInt is null!");
        }

        public float GetFloat()
        {
            var value = _data.GetFloat(_indexer++);
            if (value != null)
                return value.Value;
            throw new Exception("NetworkPacket - GetFloat is null!");
        }
        
        public string GetString()
        {
            var value = _data.GetString(_indexer++);
            if (value != null)
                return value;
            throw new Exception("NetworkPacket - GetFloat is null!");
        }

        public Vector2 GetVector2()
        {
            var value = _data.GetVector2(_indexer++);
            if (value != null)
                return value.Value;
            throw new Exception("NetworkPacket - GetVector2 is null!");
        }

        public Vector3 GetVector3()
        {
            var value = _data.GetVector3(_indexer++);
            if (value != null)
                return value.Value;
            throw new Exception("NetworkPacket - GetVector3 is null!");
        }
        
        public Vector4 GetVector4()
        {
            var value = _data.GetVector4(_indexer++);
            if (value != null)
                return value.Value;
            throw new Exception("NetworkPacket - GetVector4 is null!");
        }
        #endregion
    }
}