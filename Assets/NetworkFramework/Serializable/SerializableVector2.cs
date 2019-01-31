using UnityEngine;

namespace NetworkFramework.Serializable
{
    public class SerializableVector2 
    {
        public float x;
        public float y;
        
        public SerializableVector2(float rX, float rY)
        {
            x = rX;
            y = rY;
        }
        
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", x, y);
        }
        
        public static implicit operator Vector2(SerializableVector2 rValue)
        {
            return new Vector3(rValue.x, rValue.y);
        }
        
        public static implicit operator SerializableVector2(Vector2 rValue)
        {
            return new SerializableVector2(rValue.x, rValue.y);
        }
    }
}
