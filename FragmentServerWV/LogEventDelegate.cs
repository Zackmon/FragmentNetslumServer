using System;

namespace FragmentServerWV
{
    public delegate void Notify(String text,int Logsize);

    public class LogEventDelegate
    {
        public event Notify Logging;

        public void LogRequestResponse(String text, int logSize)
        {
            OnLogging(text,logSize);
        }

        protected virtual void OnLogging(String text,int LogSize)
        {
            Logging?.Invoke(text,LogSize);
        }
    }
}