using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GameSparks.RT;
using NetworkFramework.Managers;

namespace NetworkFramework.Parser.RTParserCore
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RTPacketParserUtility
    {
        private Dictionary<int, SendOrReciveRTPacket> _parsers = new Dictionary<int, SendOrReciveRTPacket>();
        private List<int>                         IgnoreInts => _parsers.Keys.ToList();

        public void AddParser(SendOrReciveRTPacket parserData)
        {
            var opCode = parserData.OpCode;
            if(NetworkManager.Instance().IntsReserverdBySystem.Contains(opCode))
                throw new Exception("Ooops, this opCode: " + opCode + " reserverd by system, please yse another :)");
            if (_parsers.ContainsKey(opCode))
                throw new Exception("Parser for this opCode: " + opCode + " already exists. Try use Subscribe Method instead.");
            _parsers.Add(opCode, parserData);
//            NetworkManager.Instance().IgnoreInts.AddRange(IgnoreInts);
            NetworkManager.Instance().IgnoreInts.Add(opCode);
        }

        public void RemoveParser(int opCode)
        {
            if(!_parsers.ContainsKey(opCode))
                throw new Exception("Parser with opCode: " + opCode + " not exist");
            _parsers.Remove(opCode);
        }

        public void SubscribeOnParser(int opCode, Action<RTPacket> action)
        {
            if (_parsers.ContainsKey(opCode))
                throw new Exception("Parser for this op code already exists. Try use AddParser Method instead.");
            _parsers[opCode].OnRecive += action;
        }

        public bool Contains(int           opCode) => _parsers.ContainsKey(opCode);
        public void ExecuteParser(RTPacket packet) => _parsers[packet.OpCode].OnRecive?.Invoke(packet);
    }
}