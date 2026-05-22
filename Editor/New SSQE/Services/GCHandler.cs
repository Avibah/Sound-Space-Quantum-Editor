using New_SSQE.Audio;

namespace New_SSQE.Services
{
    internal class GCHandler
    {
        private const double GC_TIME = 10;
        private static double GC_TIMEOUT => SoundEngine.PERIOD_MILLISECONDS / 2;

        private static double _lastCollected = 0;
        private static double _lastStopped = 0;
        private static double _time = 0;
        private static double _timeouts = 0;

        public static void Process(double frametime)
        {
            if (MusicPlayer.IsPlaying)
                return;

            _time += frametime;

            if (_time <= _lastStopped + GC_TIME * Math.Pow(2, _timeouts + 1))
                return;
            if (_time <= _lastCollected + GC_TIME)
                return;

            _lastCollected = _time;

            try
            {
                double start = GC.GetTotalPauseDuration().TotalSeconds;
                GC.Collect();
                double end = GC.GetTotalPauseDuration().TotalSeconds;

                if (end - start > GC_TIMEOUT)
                {
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
