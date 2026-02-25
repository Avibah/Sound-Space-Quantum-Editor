using New_SSQE.Misc.Static;

namespace New_SSQE.NewGUI.Base
{
    internal class Animator
    {
        private readonly Dictionary<string, AnimatedValue> _values = [];

        public void Play()
        {
            foreach (AnimatedValue value in _values.Values)
                value.Playing = true;
        }
        public void Play(string key) => _values[key].Playing = true;

        public void Pause()
        {
            foreach (AnimatedValue value in _values.Values)
                value.Playing = false;
        }
        public void Pause(string key) => _values[key].Playing = false;

        public void Reset()
        {
            foreach (AnimatedValue value in _values.Values)
                value.CurrentTime = 0;
        }
        public void Reset(string key) => _values[key].CurrentTime = 0;

        public void Stop()
        {
            Pause();
            Reset();
        }
        public void Stop(string key)
        {
            Pause(key);
            Reset(key);
        }

        public void SetReversed(bool reversed)
        {
            foreach (AnimatedValue value in _values.Values)
                value.Reversed = reversed;
        }
        public void SetReversed(string key, bool reversed) => _values[key].Reversed = reversed;

        public bool Process(float frametime)
        {
            bool updated = false;

            foreach (AnimatedValue value in _values.Values)
            {
                float prevValue = value.Value;
                value.Process(frametime);

                updated |= prevValue != value.Value;
            }

            return updated;
        }

        public AnimatedValue AddKey(string key, float duration, float scale = 1, EasingStyle style = EasingStyle.Exponential, EasingDirection direction = EasingDirection.Out)
        {
            AnimatedValue value = new()
            {
                Duration = duration,
                Style = style,
                Direction = direction,
                Scale = scale
            };

            if (!_values.TryAdd(key, value))
                _values[key] = value;

            return value;
        }

        public AnimatedValue Get(string key)
        {
            return _values[key];
        }

        public float this[string key] => _values[key].Value;
    }
}
