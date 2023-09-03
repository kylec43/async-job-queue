class JobQueue
{
    public delegate void Job();
    private Queue<Job> queue;
    private bool isRunning;
    private int concurrentJobThreshold;
    private List<Task> concurrentJobs;

    public JobQueue(int concurrentJobThreshold = 1)
    {
        this.queue = new Queue<Job>();
        this.isRunning = false;
        this.concurrentJobThreshold = concurrentJobThreshold;
        this.concurrentJobs = new List<Task>();
    }

    public bool IsEmpty()
    {
        lock (this.queue)
        {
            return this.queue.Count == 0;
        }
    }

    public int Count()
    {
        lock (this.queue)
        {
            return this.queue.Count;
        }
    }

    public void WaitForAllJobs()
    {
        while (!this.IsEmpty() || !this.IsConcurrentJobsEmpty())
        {
            Thread.Yield();
        }
    }

    public void Enqueue(Job job)
    {
        lock (this.queue)
        {
            this.queue.Enqueue(job);
        }

        if (!this.isRunning)
        {
            this.isRunning = true;
            Task.Run(this.Run);
        }
    }

    private bool IsConcurrentJobsEmpty()
    {
        lock (this.concurrentJobs)
        {
            return this.concurrentJobs.Count == 0;
        }
    }

    private void Run()
    {
        while (!this.IsEmpty())
        {
            while (this.IsAtConcurrentJobThreshold())
            {
                Thread.Yield();
            }

            this.RunNextJob();
        }

        this.isRunning = false;
    }

    private bool IsAtConcurrentJobThreshold()
    {
        lock (this.concurrentJobs)
        {
            return this.concurrentJobs.Count >= this.concurrentJobThreshold;
        }
    }

    private void RunNextJob()
    {
        var job = this.Dequeue();
        var runningJob = Task.Run(() => job());
        lock (this.concurrentJobs)
        {
            this.concurrentJobs.Add(runningJob);
            runningJob.ContinueWith(_ => this.PostJobCleanup(runningJob));
        }
    }

    private Job Dequeue()
    {
        lock (this.queue)
        {
            return this.queue.Dequeue();
        }
    }

    private void PostJobCleanup(Task task)
    {
        lock (this.concurrentJobs)
        {
            this.concurrentJobs.Remove(task);
        }
    }
}