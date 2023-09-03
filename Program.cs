class AsyncJobQueue
{
    static void Main(string[] args)
    {
        var queue = new JobQueue(3);
        queue.Enqueue(() => PrintSleepyMessage(10000, "Job 1 Executing", "Job 1 Executed"));
        queue.Enqueue(() => PrintSleepyMessage(500, "Job 2 Executing", "Job 2 Executed"));
        queue.Enqueue(() => PrintSleepyMessage(5000, "Job 3 Executing", "Job 3 Executed"));
        queue.Enqueue(() => PrintSleepyMessage(500, "Job 4 Executing", "Job 4 Executed"));
        queue.Enqueue(() => PrintSleepyMessage(200, "Job 5 Executing", "Job 5 Executed"));
        queue.WaitForAllJobs();
        Console.WriteLine("-----Finished-----");
        Console.ReadLine();
    }

    static void PrintSleepyMessage(int waitTimeMs = 1000, string beginMessage = "", string endMessage = "")
    {
        Console.WriteLine(beginMessage);
        Thread.Sleep(waitTimeMs);
        Console.WriteLine(endMessage);
    }
}