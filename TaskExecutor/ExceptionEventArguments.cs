using System;

namespace TaskExecutor
{
    public class ExceptionEventArguments : EventArgs
    {
        public ExceptionEventArguments(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
