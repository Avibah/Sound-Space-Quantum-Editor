using New_SSQE.ExternalUtils;

namespace New_SSQE.Misc.Static
{
    internal class GCHandler
    {
        private const double GC_TIME = 2;
        private const double GC_TIMEOUT = 0.05;

        private static double _lastCollected = 0;
        private static double _lastStopped = 0;
        private static double _time = 0;
        private static double _timeouts = 0;

        public static void Process(double frametime)
        {
            _time += frametime;

            if (_time <= _lastStopped + GC_TIME * Math.Pow(2, _timeouts + 1))
                return;
            if (_time <= _lastCollected + GC_TIME)
                return;

            _lastCollected = _time;

            try
            {
                Logging.Log($"Ran GC: {_time}");
                double start = GC.GetTotalPauseDuration().TotalSeconds;
                GC.Collect();
                double end = GC.GetTotalPauseDuration().TotalSeconds;

                if (end - start > GC_TIMEOUT)
                {
                    Logging.Log("GC Timeout");
                    _lastStopped = _time;
                    _timeouts++;
                }
                else if (_timeouts > 0)
                    _timeouts--;
            }
            catch { }
        }
    }
}
