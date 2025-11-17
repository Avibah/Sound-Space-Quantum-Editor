using New_SSQE.Misc.Static;

namespace New_SSQE.NewGUI.Base
{
    internal class AnimatedValue
    {
        public float Value { get; private set; } = 0;

        private float _currentTime = 0;
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                Update();
            }
        }

        private float _duration = 0;
        public float Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                Update();
            }
        }

        public EasingStyle Style = EasingStyle.Linear;
        public EasingDirection Direction = EasingDirection.InOut;

        private void Update()
        {
            _currentTime = Math.Clamp(_currentTime, 0, _duration);
            Value = (float)Easing.Process(0, 1, _currentTime / _duration, Style, Direction);
        }
    }
}
