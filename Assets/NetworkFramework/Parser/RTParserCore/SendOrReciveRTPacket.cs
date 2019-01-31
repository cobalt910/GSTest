using System;
using System.Diagnostics.CodeAnalysis;
using GameSparks.RT;
using NetworkFramework.Managers;
using UnityEngine;

namespace NetworkFramework.Parser.RTParserCore
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SendOrReciveRTPacket
    {
        public int              OpCode;
        public Action<RTData>           Send;
        public Action<RTPacket> OnRecive;

        public SendOrReciveRTPacket(int opCode, Action<RTData> send, Action<RTPacket> onRecive)
        {
            OpCode = opCode;
            Send = send;
            OnRecive = onRecive;
            if(NetworkManager.Instance() && onRecive != null)
                NetworkManager.Instance().RtParser.AddParser(this);
        }

        public void AddSendAction(Action<RTData> action) => Send += action;
        public void Inject(RTData data = null)
        {
            if (Send == null)
            {
                Debug.LogError("Not Implemented Send Method!");
                return;
            }
            Send.Invoke(data);
        }
    }
}