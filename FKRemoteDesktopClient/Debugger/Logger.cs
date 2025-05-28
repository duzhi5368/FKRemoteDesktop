using FKRemoteDesktop.Enums;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Debugger
{
    public static class Logger
    {
        public static event Action<ELogType, string> OnLog;

        public static void Log(ELogType logType, string message)
        {
            OnLog?.Invoke(logType, message);
        }
    }
}