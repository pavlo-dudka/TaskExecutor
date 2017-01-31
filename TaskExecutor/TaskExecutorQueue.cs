using System;
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

        public enum StopBehaviour { Undefined, Immidiately, WaitOne, WaitAll };

        public TaskExecutorQueue()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executionTask = Task.Run(() =>
            {
                while (true)
                {
                    Action action;
                    if (!taskCollection.TryTake(out action, -1))
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
            taskCollection.Add(action);
        }

        public void Stop(StopBehaviour stopBehaviour)
        {
            taskCollection.CompleteAdding();

            switch (stopBehaviour)
            {
                case StopBehaviour.Immidiately:
                    cancellationTokenSource.Cancel();
                    break;
                default:
                    this.stopBehaviour = stopBehaviour;
                    executionTask.Wait();
                    break;
            }
        }
    }
}
