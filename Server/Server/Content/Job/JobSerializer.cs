namespace Server.Content.Job;

public class JobSerializer
{
    private Queue<IJob> _jobs = new Queue<IJob>();
    private object _lock = new object();
    private bool _flush = false;
    private JobTimer _jobTimer = new JobTimer();

    public IJob PushAfter(Action action, int afterTick = 0) => PushAfter(new Job(action), afterTick);

    public IJob PushAfter<T>(Action<T> action, T param, int afterTick = 0) => PushAfter(new Job<T>(action, param), afterTick);

    public IJob PushAfter<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, int afterTick = 0) => PushAfter(new Job<T1, T2>(action, param1, param2), afterTick);

    public IJob PushAfter<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, int afterTick = 0) => PushAfter(new Job<T1, T2, T3>(action, param1, param2, param3), afterTick);

    public IJob PushAfter(IJob job, int afterTick = 0)
    {
        _jobTimer.Push(job, afterTick);
        return job;
    }

    public void Push(Action action) => Push(new Job(action));

    public void Push<T>(Action<T> action, T param) => Push(new Job<T>(action, param));

    public void Push<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) => Push(new Job<T1, T2>(action, param1, param2));

    public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => Push(new Job<T1, T2, T3>(action, param1, param2, param3));

    public void Push(IJob job)
    {
        lock (_lock)
            _jobs.Enqueue(job);
    }

    public void Flush()
    {
        _jobTimer.Flush();

        List<IJob> jobs = new List<IJob>();
        lock (_lock)
        {
            while (_jobs.Count > 0)
                jobs.Add(_jobs.Dequeue());
        }

        foreach (IJob job in jobs)
            job.Execute();
    }
}
