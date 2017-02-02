using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskExecutor
{
    public class TaskExecutorQueueMonitor
    {
        Queue<Action> taskCollection = new Queue<Action>();
        Task executionTask;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        object monitor = new object();
        StopBehaviour stopBehaviour;

        public event EventHandler<ExceptionEventArguments> OnException;

        public TaskExecutorQueueMonitor()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executionTask = Task.Run(() =>
            {
                while (true)
                {
                    Action action;
                    lock (monitor)
                    {
                        while (taskCollection.Count == 0 && stopBehaviour == StopBehaviour.Undefined)
                            Monitor.Wait(monitor, 100);

                        if (stopBehaviour == StopBehaviour.WaitOne ||
                            stopBehaviour == StopBehaviour.WaitAll && taskCollection.Count == 0)
                            break;

                        action = taskCollection.Dequeue();
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(this, new ExceptionEventArguments(ex));
                    }
                }
            }, cancellationToken);
        }

        public void Add(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (monitor)
            {
                taskCollection.Enqueue(action);
                if (taskCollection.Count == 1)
                    Monitor.Pulse(monitor);
            }
        }

        public void Stop(StopBehaviour stopBehaviour)
        {
            this.stopBehaviour = stopBehaviour;

            switch (stopBehaviour)
            {
                case StopBehaviour.Immidiately:
                    cancellationTokenSource.Cancel();
                    break;
                default:
                    executionTask.Wait();
                    break;
            }
        }
    }
}
