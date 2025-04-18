using New_SSQE.Audio;
using New_SSQE.Objects;
using New_SSQE.Objects.Managers;
using New_SSQE.Preferences;
using OpenTK.Mathematics;

namespace New_SSQE.NewMaps
{
    internal static class Timing
    {
        public static List<Note> GetNotesFromPoint(TimingPoint point)
        {
            int index = Mapping.Current.TimingPoints.IndexOf(point);
            long next = index + 1 < Mapping.Current.TimingPoints.Count ? Mapping.Current.TimingPoints[index + 1].Ms : (long)Settings.currentTime.Value.Max;
            List<Note> notes = [];

            for (int i = 0; i < Mapping.Current.Notes.Count; i++)
            {
                Note note = Mapping.Current.Notes[i];

                if (note.Ms >= point.Ms && note.Ms < next)
                    notes.Add(note);
            }

            return notes;
        }

        public static void UpdatePoint(TimingPoint point, float newBpm, long newMs)
        {
            float mult = point.BPM / newBpm;
            List<Note> notes = GetNotesFromPoint(point);

            NoteManager.Edit("ADJUST NOTE[S]", notes, n => n.Ms = (long)((n.Ms - point.Ms) * mult) + point.Ms);
        }

        public static void MovePoints(List<TimingPoint> points, long offset)
        {
            List<Note> toAdjust = [];

            for (int i = 0; i < points.Count; i++)
            {
                TimingPoint point = points[i];
                List<Note> notes = GetNotesFromPoint(point);

                for (int j = 0; j < notes.Count; j++)
                    toAdjust.Add(notes[j]);
            }

            NoteManager.Edit("ADJUST NOTE[S]", toAdjust, n => n.Ms += offset);
        }



        public static long GetClosestBeat(float currentMs, TimingPoint? draggingPoint = null)
        {
            long closestMs = -1;
            TimingPoint point = GetCurrentBpm(currentMs, draggingPoint);

            if (point.BPM > 0)
            {
                float interval = 60000f / point.BPM / (Settings.beatDivisor.Value.Value + 1f);
                float offset = point.Ms % interval;

                closestMs = (long)Math.Round((long)Math.Round((currentMs - offset) / interval) * interval + offset);
            }

            return closestMs;
        }

        public static long GetClosestBeatScroll(float currentMs, bool negative = false, int iterations = 1)
        {
            long closestMs = GetClosestBeat(currentMs);

            if (GetCurrentBpm(closestMs).BPM <= 0)
                return -1;

            for (int i = 0; i < iterations; i++)
            {
                TimingPoint currentPoint = GetCurrentBpm(currentMs, negative ? new(0, 0) : null);
                float interval = 60000 / currentPoint.BPM / (Settings.beatDivisor.Value.Value + 1f);

                if (negative)
                {
                    closestMs = GetClosestBeat(currentMs, new(0, 0));

                    if (closestMs >= currentMs)
                        closestMs = GetClosestBeat(closestMs - (long)interval);
                }
                else
                {
                    if (closestMs <= currentMs)
                        closestMs = GetClosestBeat(closestMs + (long)interval);

                    if (GetCurrentBpm(currentMs).Ms != GetCurrentBpm(closestMs).Ms)
                        closestMs = GetCurrentBpm(closestMs).Ms;
                }

                currentMs = closestMs;
            }

            if (closestMs < 0)
                return -1;

            return (long)Math.Clamp(closestMs, 0, Settings.currentTime.Value.Max);
        }

        public static TimingPoint GetCurrentBpm(float currentMs, TimingPoint? draggingPoint = null)
        {
            TimingPoint currentPoint = new(0, 0);

            for (int i = 0; i < Mapping.Current.TimingPoints.Count; i++)
            {
                TimingPoint point = Mapping.Current.TimingPoints[i];

                if (point == draggingPoint)
                    continue;
                if (point.Ms < currentMs || (draggingPoint == null && point.Ms == currentMs))
                    currentPoint = point;
            }

            return currentPoint;
        }

        public static void Advance(bool reverse = false)
        {
            SliderSetting setting = Settings.currentTime.Value;
            float bpm = GetCurrentBpm(setting.Value).BPM;

            if (bpm > 0)
                setting.Value = GetClosestBeatScroll(setting.Value, reverse);
        }

        public static void Scroll(bool reverse, float mult = 1)
        {
            float delta = mult * (reverse ? -1 : 1);

            SliderSetting setting = Settings.currentTime.Value;
            float currentTime = setting.Value;
            float totalTime = setting.Max;

            if (MusicPlayer.IsPlaying && !Settings.pauseScroll.Value)
            {
                currentTime += delta * 500f * Mapping.Current.Tempo;
                currentTime = Math.Clamp(currentTime, 0f, totalTime);

                setting.Value = currentTime;

                MusicPlayer.CurrentTime = TimeSpan.FromMilliseconds(currentTime);
            }
            else
            {
                if (MusicPlayer.IsPlaying)
                    MusicPlayer.Pause();

                long closest = GetClosestBeatScroll(currentTime, delta < 0);
                TimingPoint bpm = GetCurrentBpm(0);

                currentTime = closest >= 0 || bpm.BPM > 0 ? closest : currentTime + delta / 10f * 1000f / Mapping.Current.Zoom * 0.5f;

                if (GetCurrentBpm(setting.Value).BPM == 0 && GetCurrentBpm(currentTime).BPM != 0)
                    currentTime = GetCurrentBpm(currentTime).Ms;

                currentTime = Math.Clamp(currentTime, 0f, totalTime);

                setting.Value = currentTime;
            }
        }
    }
}
