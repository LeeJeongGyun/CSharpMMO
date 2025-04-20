namespace Server.Content.Job;

using ServerCore;

public struct JobTimerElem : IComparable<JobTimerElem>
{
    public int execTick;
    public IJob job;

    public int CompareTo(JobTimerElem other) => other.execTick - execTick;
}

public class JobTimer
{
    private PriorityQ<JobTimerElem> _priorityQ = new PriorityQ<JobTimerElem>();
    private object _lock = new object();

    public void Push(IJob job, int tickAfter = 0)
    {
        JobTimerElem jobElem;
        jobElem.execTick = (int)Environment.TickCount64 + tickAfter;
        jobElem.job = job;

        lock (_lock)
            _priorityQ.Push(jobElem);
    }

    public void Flush()
    {
        while (true)
        {
            IJob execJob;
            long nowTick = Environment.TickCount64;
            if (_priorityQ.Count == 0)
                break;

            lock (_lock)
            {
                JobTimerElem jobElem = _priorityQ.Peek();
                if (jobElem.execTick > nowTick)
                    break;

                execJob = _priorityQ.Pop().job;
            }
            execJob.Execute();
        }
    }
}
