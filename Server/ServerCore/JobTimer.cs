using System;

namespace ServerCore
{
    public struct JobElement : IComparable<JobElement>
    {
        public int reservedTick { get; set; }
        public Action action { get; set; }

        public int CompareTo(JobElement other)
        {
            return other.reservedTick - reservedTick;
        }
    }

    public class JobTimer
    {
        private PriorityQ<JobElement> _jobs = new PriorityQ<JobElement>();
        private object _lock = new object();
        public static JobTimer Instance { get; } = new JobTimer();

        public void Add(JobElement element)
        {
            element.reservedTick += Environment.TickCount;
            lock (_lock)
                _jobs.Push(element);
        }

        public void Flush()
        {
            lock (_lock)
            {
                int currentTick = Environment.TickCount;
                while (_jobs.Count > 0)
                {
                    JobElement job = _jobs.Peek();
                    if (currentTick > job.reservedTick)
                        break;

                    job.action.Invoke();
                    _jobs.Pop();
                }
            }
        }
    }
}
