﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TaskExecutor
{
    public class TaskExecutorQueue
    {
        BlockingCollection<Action> taskCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        Task executionTask;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        StopBehaviour stopBehaviour;

        public event EventHandler<ExceptionEventArguments> OnException;        

        public TaskExecutorQueue()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executionTask = Task.Run(() =>
            {
                while (true)
                {
                    Action action;
                    if (!taskCollection.TryTake(out action, Timeout.Infinite))
                        break;

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(this, new ExceptionEventArguments(ex));
                    }

                    if (stopBehaviour == StopBehaviour.WaitOne && taskCollection.IsAddingCompleted ||
                        stopBehaviour == StopBehaviour.WaitAll && taskCollection.IsCompleted)
                        break;
                }
            }, cancellationToken);
        }

        public void Add(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            taskCollection.Add(action);
        }

        public void Stop(StopBehaviour stopBehaviour)
        {
            this.stopBehaviour = stopBehaviour;
            taskCollection.CompleteAdding();

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
