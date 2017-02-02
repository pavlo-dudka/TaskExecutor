using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskExecutor
{
    public class TaskExecutorQueueDataflow
    {
        ActionBlock<Action> actionBlock;
        Task executionTask;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        StopBehaviour stopBehaviour;

        public event EventHandler<ExceptionEventArguments> OnException;

        public TaskExecutorQueueDataflow()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            actionBlock = new ActionBlock<Action>(
                action: (action) =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(this, new ExceptionEventArguments(ex));
                    }
                    if (stopBehaviour == StopBehaviour.WaitOne)
                        cancellationTokenSource.Cancel();
                },
                dataflowBlockOptions: new ExecutionDataflowBlockOptions() { CancellationToken = cancellationToken }               
            );
            executionTask = actionBlock.Completion;
        }

        public void Add(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            actionBlock.Post(action);
        }

        public void Stop(StopBehaviour stopBehaviour)
        {
            this.stopBehaviour = stopBehaviour;
            actionBlock.Complete();

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
