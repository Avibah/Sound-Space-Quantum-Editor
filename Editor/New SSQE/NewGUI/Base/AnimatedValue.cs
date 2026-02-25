using New_SSQE.Misc.Static;

namespace New_SSQE.NewGUI.Base
{
    internal class AnimatedValue
    {
        public bool Playing { get; set; } = false;
        public bool Reversed { get; set; } = false;
        public float Value { get; private set; } = 0;
        public float Scale { get; set; } = 1;
        public EasingStyle Style { get; set; } = EasingStyle.Exponential;
        public EasingDirection Direction { get; set; } = EasingDirection.Out;

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

        private void Update()
        {
            _currentTime = Math.Clamp(_currentTime, 0, _duration);
            Value = (float)Easing.Process(0, 1, _currentTime / _duration, Style, Direction) * Scale;
        }

        public void Process(float frametime)
        {
            if (!Playing)
                return;

            if (Reversed)
                frametime *= -1;
            else
                frametime *= 1;

            CurrentTime += frametime;
        }
    }
}
