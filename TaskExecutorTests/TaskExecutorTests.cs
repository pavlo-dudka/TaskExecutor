using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskExecutor.Tests
{
    [TestClass()]
    public class TaskExecutorTests
    {
        [TestMethod()]
        public void TaskExecutorTest()
        {
            var taskExecutorQueue = new TaskExecutorQueue();

            int i = 0;
            int actualTaskCount = 0;
            int expectedTaskCount = 100;

            //taskExecutorQueue.OnException += (sender, e) => {};

            while (i++ < expectedTaskCount)
            {
                taskExecutorQueue.Add(() =>
                {
                    actualTaskCount++;

                    //throw new Exception("task failed");
                });
            }

            taskExecutorQueue.Stop(TaskExecutorQueue.StopBehaviour.WaitAll);

            Assert.AreEqual(expectedTaskCount, actualTaskCount);
        }
    }
}