# Async JobQueue

This was just done for fun. Thrown together in just an hour or so while practicing async and threads in C#

### Methods
* Constructor(int concurrentJobThreshold = 1)
  * concurrentJobThreshold specifies the maximum number of jobs that can run concurrently
* Enqueue(Job job)
  * Adds a job to the queue
* WaitForAllJobs()
  * Yields thread and waits for all jobs in the queue to complete

### Example
* This example will add 5 jobs to the queue
* Only 3 jobs will be able to run concurrently from the queue
  
```csharp
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
```
### Output
```
Job 1 Executing
Job 3 Executing
Job 2 Executing
Job 2 Executed
Job 4 Executing
Job 4 Executed
Job 5 Executing
Job 5 Executed
Job 3 Executed
Job 1 Executed
-----Finished-----
```