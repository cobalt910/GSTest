using System;
using System.Collections.Generic;
using GameSparks.Api.Messages;
using NetworkFramework.Managers;

namespace NetworkFramework.Parser.ServerMessageParser
{
    public class ServerMessageParserUtility
    {
        private Dictionary<string, OnReceiveServerMessage> _parsers = new Dictionary<string, OnReceiveServerMessage>();
        public  bool                                       Contains(string extCode) => _parsers.ContainsKey(extCode);

        public void Add(OnReceiveServerMessage parser, string extCode) => _parsers.Add(extCode, parser);
        public void Parse(ScriptMessage message)
        {
            var contains = Contains(message.ExtCode);
            if(!contains) throw new Exception("Unhandled Server Message, cannot parse this >_< ExtCode: " + message.ExtCode);
            _parsers[message.ExtCode].Parse(message);
        }
    }

    public class OnReceiveServerMessage
    {
        private Action<ScriptMessage> _method;
    
        public OnReceiveServerMessage(string extCode, Action<ScriptMessage> method)
        {
            _method = method;
            if(GameSparksManager.Instance()) GameSparksManager.Instance().ServerMessageParserUtility.Add(this, extCode);
        }

        public void Parse(ScriptMessage message) => _method?.Invoke(message);
    }
}