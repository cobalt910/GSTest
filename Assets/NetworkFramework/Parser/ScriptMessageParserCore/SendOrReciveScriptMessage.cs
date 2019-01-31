using System;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using Newtonsoft.Json;
using UnityEngine;

namespace NetworkFramework.Parser.ScriptMessageParserCore
{
	public abstract class AScriptMessage
	{
		public abstract void Inject(string targetId);
		public abstract void Parse(ScriptMessage message);
		public abstract string GetExecuteCode();
	}

	public class SendOrReciveScriptMessage<TRequest, TResponse> : AScriptMessage where TRequest : class 
	{
		public string ExtCode;
		private Func<TRequest> _sendAction;
		private Action<TResponse> _getAction;

		public SendOrReciveScriptMessage(string extCode = "", Func<TRequest> sendAction = null, Action<TResponse> getAction = null)
		{
			ExtCode = extCode;
			_sendAction = sendAction;
			_getAction = getAction;
		}

		public override void Inject(string targetId)
		{
			var data = _sendAction?.Invoke();
			if(data == null) throw new Exception("Inejected data cannot be null!");
			var jsonData = JsonConvert.SerializeObject(data);

			var request = new LogEventRequest().SetEventKey(ExtCode);
			request.SetEventAttribute("targetId", targetId);
			request.SetEventAttribute("data", jsonData);
			request.Send(x =>
			{
				if(x.HasErrors) Debug.Log(x.Errors.JSON);
			});
		}

		public override void Parse(ScriptMessage message)
		{
			if (!message.ExtCode.Equals(ExtCode)) return;
			var parsedObj = JsonConvert.DeserializeObject<TResponse>(message.Data.JSON);
			_getAction?.Invoke(parsedObj);
		}

		public override string GetExecuteCode() => ExtCode;

	}
}
