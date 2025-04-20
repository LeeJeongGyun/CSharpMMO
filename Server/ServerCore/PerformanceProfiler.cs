using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public class PerformanceProfiler
    {
        #region 싱글톤

        private static PerformanceProfiler _inst;

        public static PerformanceProfiler Instance
        {
            get
            {
                if (_inst == null)
                    _inst = new PerformanceProfiler();

                return _inst;
            }
        }

        #endregion 싱글톤

        public (int prev, int cur) _gcGen0Count;
        public (int prev, int cur) _gcGen1Count;
        public (int prev, int cur) _gcGen2Count;
        public int curRecvCount;
        public int curSendCount;
        public int prevRecvCount;
        public int prevSendCount;
        private PerformanceCounter _cpuCounter;
        private bool _performancePrintFlag;

        public PerformanceProfiler()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            _cpuCounter.NextValue();

            _performancePrintFlag = true;
            Task.Run(() =>
            {
                while (_performancePrintFlag)
                {
                    PrintPerformanceDataPerSecond();
                    Thread.Sleep(1000);
                }
            });
        }

        public void PrintPerformanceDataPerSecond()
        {
            curRecvCount = prevRecvCount;
            curSendCount = prevSendCount;
            prevRecvCount = prevSendCount = 0;

            float cpuUsage = _cpuCounter.NextValue();
            //GlobalLogger.WriteLog?.Invoke($"CPU(%): {cpuUsage:F2}, SendTPS: {curSendCount}, RecvTPS:{curRecvCount}");
            //GlobalLogger.WriteLog?.Invoke($"GC: {{Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}}}");
        }

        public void StopPrintPerformanceData() => _performancePrintFlag = false;
    }
}
