using System.Collections.Generic;
using System;

namespace ServerCore
{
    public class PriorityQ<T> where T : IComparable<T>
    {
        private List<T> _heap = new List<T>();
        public int Count => _heap.Count;

        public void Push(T data)
        {
            _heap.Add(data);
            int cIdx = _heap.Count - 1;

            while (cIdx > 0)
            {
                int pIdx = (cIdx - 1) / 2;
                if (_heap[cIdx].CompareTo(_heap[pIdx]) < 0)
                    break;

                (_heap[pIdx], _heap[cIdx]) = (_heap[cIdx], _heap[pIdx]);
                cIdx = pIdx;
            }
        }

        public T Pop()
        {
            T retData = _heap[0];
            int lastIdx = _heap.Count - 1;
            _heap[0] = _heap[lastIdx];
            _heap.RemoveAt(lastIdx);
            lastIdx--;

            int cIdx = 0;
            while (true)
            {
                int lIdx = cIdx * 2 + 1;
                int rIdx = cIdx * 2 + 2;

                int compIdx = cIdx;
                if (lIdx <= lastIdx && _heap[compIdx].CompareTo(_heap[lIdx]) < 0)
                    compIdx = lIdx;

                if (rIdx <= lastIdx && _heap[compIdx].CompareTo(_heap[rIdx]) < 0)
                    compIdx = rIdx;

                if (cIdx == compIdx)
                    break;

                (_heap[cIdx], _heap[compIdx]) = (_heap[compIdx], _heap[cIdx]);
                cIdx = compIdx;
            }

            return retData;
        }

        public T Peek() => _heap[0];
    }
}
