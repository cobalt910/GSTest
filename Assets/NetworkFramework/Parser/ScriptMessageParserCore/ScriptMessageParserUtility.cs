using System;
using System.Collections.Generic;
using GameSparks.Api.Messages;
using NetworkFramework.Parser.RTParserCore;
using UnityEngine;

namespace NetworkFramework.Parser.ScriptMessageParserCore
{
	public class ScriptMessageParserUtility
	{
		private Dictionary<string, AScriptMessage> _parsers = new Dictionary<string, AScriptMessage>();
		public bool Contains(string key) => _parsers.ContainsKey(key);

		public void Parse(ScriptMessage message)
		{
			var contains = Contains(message.ExtCode);
			if(!contains) throw new Exception("Unhandled Script Message, cannot parse this >_< ExtCode: " + message.ExtCode);
			_parsers[message.ExtCode].Parse(message);
		}

		public void AddParser(AScriptMessage scriptMessageParser)
		{
			if(!Contains(scriptMessageParser.GetExecuteCode()))
				_parsers.Add(scriptMessageParser.GetExecuteCode(), scriptMessageParser);
			else Debug.LogError("Parser.ExecuteCode : [" + scriptMessageParser.GetExecuteCode() + "] already exists.");
		}
		
		public void DeleteParser(string executeCode)
		{
			if (Contains(executeCode))
				_parsers.Remove(executeCode);
			else Debug.LogError("ExecudeCode: [" + executeCode + "] not exists.");
		}
		
		public void DeleteParser(AScriptMessageExecute parser)
		{
			if (Contains(parser.GetExecuteCode()))
				_parsers.Remove(parser.GetExecuteCode());
			else Debug.LogError("ExecudeCode: [" + parser.GetExecuteCode() + "] not exists.");                
		}
	}
}
