class JobQueue
{
    private Queue<Task> queue;
    private object isRunningLock;
    private bool isRunning;
    private int concurrentJobThreshold;
    private List<Task> concurrentJobs;

    public JobQueue(int concurrentJobThreshold = 1)
    {
        this.queue = new Queue<Task>();
        this.isRunningLock = new object();
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
        var jobs = new List<Task>();

        // Wait for jobs in queue
        lock (this.queue)
        {
            jobs.AddRange(this.queue);
        }

        // Wait for currently running jobs
        lock (this.concurrentJobs) 
        {
            jobs.AddRange(this.concurrentJobs);
        }

        Task.WaitAll(jobs.ToArray());
    }

    public void Enqueue(Action job)
    {
        lock (this.queue)
        {
            this.queue.Enqueue(new Task(job));
        }

        lock (this.isRunningLock)
        {
            if (!this.isRunning)
            {
                this.isRunning = true;
                Task.Run(this.Run);
            }
        }
    }

    private void Run()
    {
        while (!this.IsEmpty())
        {
            if (this.IsAtConcurrentJobThreshold())
            {
                this.WaitForAnyRunningJobToComplete();
                this.RemoveCompletedJobs();
            }

            this.RunNextJob();
        }

        lock (this.isRunningLock)
        {
            this.isRunning = false;
        }
    }

    private bool IsAtConcurrentJobThreshold()
    {
        lock (this.concurrentJobs)
        {
            return this.concurrentJobs.Count >= this.concurrentJobThreshold;
        }
    }

    private void WaitForAnyRunningJobToComplete()
    {
        Task[] concurrentJobsArr;
        lock (this.concurrentJobs) 
        {
            concurrentJobsArr = this.concurrentJobs.ToArray();
        }

        Task.WaitAny(concurrentJobsArr);
    }

    private void RemoveCompletedJobs()
    {
        lock (this.concurrentJobs)
        {
            this.concurrentJobs.RemoveAll(job => job.IsCompleted);
        }
    }

    private void RunNextJob()
    {
        var job = this.Dequeue();
        job.Start();
        lock (this.concurrentJobs)
        {
            this.concurrentJobs.Add(job);
        }
    }

    private Task Dequeue()
    {
        lock (this.queue)
        {
            return this.queue.Dequeue();
        }
    }
}