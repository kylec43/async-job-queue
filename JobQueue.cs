class JobQueue
{
    public delegate void Job();
    private Queue<Job> queue;
    private object isRunningLock;
    private bool isRunning;
    private int concurrentJobThreshold;
    private List<Task> concurrentJobs;
    private Task? runTask;

    public JobQueue(int concurrentJobThreshold = 1)
    {
        this.queue = new Queue<Job>();
        this.isRunningLock = new object();
        this.isRunning = false;
        this.concurrentJobThreshold = concurrentJobThreshold;
        this.concurrentJobs = new List<Task>();
        this.runTask = null;
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
        this.runTask?.Wait();

        Task[] jobsArr;
        lock (this.concurrentJobs) 
        {
            jobsArr = this.concurrentJobs.ToArray();
        }

        Task.WaitAll(jobsArr);
    }

    public void Enqueue(Job job)
    {
        lock (this.queue)
        {
            this.queue.Enqueue(job);
        }

        lock (this.isRunningLock)
        {
            if (!this.isRunning)
            {
                this.isRunning = true;
                this.runTask = Task.Run(this.Run);
            }
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