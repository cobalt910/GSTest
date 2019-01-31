using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using UnityEngine;

public class TestProto : MonoBehaviour 
{
    [ContextMenu("Test")]
    public void Test()
    {
        float serialize;
        float deserialize;
        
        TestJson(out serialize, out deserialize);
        Debug.Log("Json serialize: " + serialize);
        Debug.Log("Json deserialize: " + deserialize);
        
        TestProtobuf(out serialize, out deserialize);
        Debug.Log("Proto serialize: " + serialize);
        Debug.Log("Proto deserialize: " + deserialize);
    }


    [Serializable]
    public class ForJson
    {
        public object[] Details;
    }

    private void TestJson(out float timeSerialize, out float timeDeserialize)
    {
        // serialize
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
        var s = new ForJson();
        s.Details = new[] {(object) 10, (object) "some text"};
        var json = JsonConvert.SerializeObject(s);
        
        watch.Stop();
        timeSerialize = watch.ElapsedTicks;
        
        // deserialize
        watch = System.Diagnostics.Stopwatch.StartNew();

        var parsed = JsonConvert.DeserializeObject<ForJson>(json);
        var intVal    = parsed.Details[0];
        var stringVal = parsed.Details[1];
        
        watch.Stop();
        timeDeserialize = watch.ElapsedTicks;
    }

    private void TestProtobuf(out float timeSerialize, out float timeDeserialize)
    {
        // serialize
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
        var s = new ForTest();
        s.Details.Add(Any.Pack(new Int32Value {Value  = 10}));
        s.Details.Add(Any.Pack(new StringValue {Value = "some text"}));
        var bytes = s.ToByteArray();
        
        watch.Stop();
        timeSerialize = watch.ElapsedTicks;
        
        // deserialize
        watch = System.Diagnostics.Stopwatch.StartNew();
        
        var parsed = ForTest.Parser.ParseFrom(bytes);
        var intVal = parsed.Details[0].Unpack<Int32Value>();
        var stringVal = parsed.Details[1].Unpack<StringValue>();
        
        watch.Stop();
        timeDeserialize = watch.ElapsedTicks;
    }
}
