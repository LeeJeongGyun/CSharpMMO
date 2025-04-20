using System.Collections.Generic;
using System;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action action);
    }

    public class JobQueue : IJobQueue
    {
        private bool _flush = false;
        private Queue<Action> _jobQ = new Queue<Action>();
        private object _lock = new object();
        public int JobCount => _jobQ.Count;

        public void DoJobs()
        {
            while (true)
            {
                Action? job = Pop();
                if (job == null)
                    return;

                job.Invoke();
            }

            // 단독 스레드가 있을 때 사용하는 게 구현하기 용이.
            //List<Action> jobs = AllPop();
            //foreach (Action job in jobs)
            //    job.Invoke();

            //_flush = false;
        }

        public void Push(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                _jobQ.Enqueue(job);
                if (_flush == false)
                    _flush = flush = true;
            }

            if (flush)
                DoJobs();
        }

        private List<Action> AllPop()
        {
            List<Action> jobs = new List<Action>();

            lock (_lock)
            {
                while (_jobQ.Count > 0)
                    jobs.Add(_jobQ.Dequeue());
            }

            return jobs;
        }

        private Action? Pop()
        {
            Action? job = null;
            lock (_lock)
            {
                if (_jobQ.Count == 0)
                {
                    _flush = false;
                    return null;
                }

                job = _jobQ.Dequeue();
                return job;
            }
        }
    }
}
