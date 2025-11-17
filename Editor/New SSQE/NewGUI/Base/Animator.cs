using New_SSQE.Misc.Static;

namespace New_SSQE.NewGUI.Base
{
    internal class Animator
    {
        public bool Playing { get; private set; } = false;
        public bool Reversed { get; set; } = false;
        private readonly Dictionary<string, AnimatedValue> _values = [];

        public void Play()
        {
            Playing = true;
        }

        public void Pause()
        {
            Playing = false;
        }

        public void Reset()
        {
            foreach (AnimatedValue value in _values.Values)
                value.CurrentTime = 0;
        }

        public void Stop()
        {
            Pause();
            Reset();
        }

        public bool Process(float frametime)
        {
            if (!Playing)
                return false;

            bool updated = false;
            if (Reversed)
                frametime *= -1;

            foreach (AnimatedValue value in _values.Values)
            {
                float prevValue = value.Value;
                value.CurrentTime += frametime;

                updated |= prevValue != value.Value;
            }

            return updated;
        }

        public void AddKey(string key, float duration, EasingStyle style = EasingStyle.Linear, EasingDirection direction = EasingDirection.InOut)
        {
            AnimatedValue value = new()
            {
                Duration = duration,
                Style = style,
                Direction = direction
            };

            if (!_values.TryAdd(key, value))
                _values[key] = value;
        }

        public AnimatedValue Get(string key)
        {
            return _values[key];
        }

        public float this[string key] => _values[key].Value;
    }
}
