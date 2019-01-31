using GameSparks.Api.Messages;

namespace NetworkFramework.Parser.RTParserCore
{
    public abstract class AScriptMessageExecute
    {
        protected readonly string ExecuteCode;
        public abstract    void   Execute(ScriptMessage message);

        protected AScriptMessageExecute(string executeCode) { ExecuteCode = executeCode; }
        public string GetExecuteCode() => ExecuteCode;
    }
}