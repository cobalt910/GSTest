using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkFramework.Serializable;
using Newtonsoft.Json;
using UnityEngine;

namespace NetworkFramework.Data
{
    [Serializable]
    public class RpcMethodCallInfo
    {
        #region variables        
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public int InstanceId;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string MethodName;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public object[] Arguments;
        #endregion

        #region supported types        
        private readonly Dictionary<Type, int> _supportedTypes = new Dictionary<Type, int>
        {
            // buidin supported types
            {typeof(int), 0},
            {typeof(float), 1},
            {typeof(double), 2},
            {typeof(bool), 3},

            // unity supported types
            {typeof(Vector2), 50},
            {typeof(Vector3), 51},
            {typeof(Vector4), 52},
            {typeof(Quaternion), 53}
        };
        #endregion

        #region constructor
        public RpcMethodCallInfo()
        {
        }

        public RpcMethodCallInfo(string methodName, object[] arguments, int instanceId)
        {
            // check supported types
            foreach (var argument in arguments)
            {
                // ignore enums
                if (argument.GetType().IsEnum)
                    continue;

                if (!_supportedTypes.Keys.Contains(argument.GetType()))
                    throw new Exception("UnsupportedTypeException!" + " Type: " + argument.GetType().Name);
            }

            // save values
            Arguments  = arguments;
            MethodName = methodName;
            InstanceId = instanceId;
        }
        #endregion

        #region serialization
        public byte[] ToByteArray()
        {
            SerializeArguments();

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var jsonString = JsonConvert.SerializeObject(this, jsonSerializerSettings);

            return Encoding.ASCII.GetBytes(jsonString);
        }

        public static RpcMethodCallInfo FromByteArray(byte[] bytes)
        {
            var jsonString = Encoding.ASCII.GetString(bytes);

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var obj = JsonConvert.DeserializeObject<RpcMethodCallInfo>(jsonString, jsonSerializerSettings);
            obj.DeserializeArguments();

            return obj;
        }
        #endregion

        #region serialization cast
        private void SerializeArguments()
        {
            for (var i = 0; i < Arguments.Length; i++)
            {
                if (Arguments[i].GetType().IsEnum)
                    Arguments[i] = new EnumWrapper(Arguments[i]);

                if (Arguments[i] is Vector2)
                    Arguments[i] = (SerializableVector2) (Vector2) Arguments[i];
                
                if (Arguments[i] is Vector3)
                    Arguments[i] = (SerializableVector3) (Vector3) Arguments[i];

                if (Arguments[i] is Vector4)
                    Arguments[i] = (SerializableVector4) (Vector4) Arguments[i];
                
                if (Arguments[i] is Quaternion)
                    Arguments[i] = (SerializableQuaternion) (Quaternion) Arguments[i];

            }
        }

        private void DeserializeArguments()
        {
            for (var i = 0; i < Arguments.Length; i++)
            {
                if (Arguments[i] is EnumWrapper)
                {
                    var wrap = (EnumWrapper) Arguments[i];
                    Arguments[i] = Enum.ToObject(wrap.EnumType, wrap.EnumObject);
                }

                if (Arguments[i] is SerializableVector2)
                    Arguments[i] = (Vector2) (SerializableVector2) Arguments[i];
                
                if (Arguments[i] is SerializableVector3)
                    Arguments[i] = (Vector3) (SerializableVector3) Arguments[i];
                
                if (Arguments[i] is SerializableVector4)
                    Arguments[i] = (Vector4) (SerializableVector4) Arguments[i];

                if (Arguments[i] is SerializableQuaternion)
                    Arguments[i] = (Quaternion) (SerializableQuaternion) Arguments[i];
            }
        }

        [Serializable]
        private class EnumWrapper
        {
            public Type   EnumType;
            public object EnumObject;

            public EnumWrapper()
            {
            }

            public EnumWrapper(object o)
            {
                EnumType   = o.GetType();
                EnumObject = o;
            }
        }
        #endregion
    }
}