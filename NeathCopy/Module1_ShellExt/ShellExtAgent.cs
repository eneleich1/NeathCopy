using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module1_ShellExt
{
    public enum ArgumentsState
    {
        Valids,Invalids
    }
    public abstract class ShellExtAgent
    {
        public ArgumentsState ArgumentsState { get; set; }
        public abstract RequestInfo ProcessArguments();
    }

    public class CmdShellExtAgent : ShellExtAgent
    {
        public string[] Arguments { get; set; }
        public CmdShellExtAgent(string[] args)
        {
            Arguments = args;

            if (Arguments == null || Arguments.Length != 3)
                ArgumentsState = ArgumentsState.Invalids;
            else ArgumentsState = ArgumentsState.Valids;

        }
        public override RequestInfo ProcessArguments()
        {
            //Process and get RequestInfo From argumentes
            return new RequestInfo(Arguments);
        }
    }
    public class DropShellExtAgent : ShellExtAgent
    {
        public RequestInfo requestInfo { get; set; }
        public DropShellExtAgent(RequestInfo requestInfo)
        {
            this.requestInfo = requestInfo;
        }
        public override RequestInfo ProcessArguments()
        {
            //Process and get RequestInfo From argumentes
            return requestInfo;
        }
    }
}
