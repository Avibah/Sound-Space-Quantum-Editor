using New_SSQE.Objects;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Numerics;
using New_SSQE.NewGUI.Windows;
using SoundTouch;
using New_SSQE.Services;

namespace New_SSQE.NewMaps
{
    internal static class Patterns
    {
        public static void StorePattern(int index)
        {
            List<Note> notes = Mapping.Current.Notes.Selected;
            if (notes.Count == 0)
                return;

            TimingPoint point = Timing.GetCurrentBpm(notes[0].Ms);
            bool onPoint = point.BPM > 0;

            float interval = onPoint ? 60000 / point.BPM / (Settings.beatDivisor.Value.Value + 1f) : 0;
            string pattern = onPoint ? "1" : "0";
            long offset = notes[0].Ms;

            foreach (Note note in notes)
            {
                string x = note.X.ToString(Program.Culture);
                string y = note.Y.ToString(Program.Culture);
                string time = (onPoint ? (note.Ms - offset) / interval : note.Ms - offset).ToString(Program.Culture);

                pattern += $",{x}|{y}|{time}";
            }

            Settings.patterns.Value[index] = pattern;
            GuiWindowEditor.ShowInfo($"BOUND PATTERN {index}");
        }

        public static void ClearPattern(int index)
        {
            Settings.patterns.Value[index] = "";
            GuiWindowEditor.ShowInfo($"UNBOUND PATTERN {index}");
        }

        public static void RecallPattern(int index)
        {
            try
            {
                string pattern = Settings.patterns.Value[index];
                if (pattern == "")
                    return;

                float offset = Settings.currentTime.Value.Value;
                TimingPoint point = Timing.GetCurrentBpm(offset);
                bool onPoint = point.BPM > 0;

                string[] patternSplit = pattern.Split(',');
                bool quantized = patternSplit[0] == "1";
                List<Note> toAdd = [];

                for (int i = 1; i < patternSplit.Length; i++)
                {
                    string note = patternSplit[i];
                    string[] noteSplit = note.Split('|');

                    float x = float.Parse(noteSplit[0], Program.Culture);
                    float y = float.Parse(noteSplit[1], Program.Culture);
                    float time = float.Parse(noteSplit[2], Program.Culture);

                    if (!quantized)
                        toAdd.Add(new(x, y, (long)(time + offset)));
                    else
                    {
                        float bpm = onPoint ? point.BPM : 60;
                        float interval = 60000 / bpm / (Settings.beatDivisor.Value.Value + 1f);

                        toAdd.Add(new(x, y, (long)(time * interval + offset)));
                    }
                }

                if (toAdd.Count == 0)
                    return;

                if (!quantized && onPoint)
                {
                    float interval = 60000 / point.BPM / (Settings.beatDivisor.Value.Value + 1f);
                    float minDist = float.MaxValue;

                    for (int i = 1; i < toAdd.Count; i++)
                    {
                        float dist = Math.Min(minDist, toAdd[i].Ms - toAdd[i - 1].Ms);
                        if (dist > 0)
                            minDist = dist;
                    }

                    for (int i = 1; i < toAdd.Count; i++)
                        toAdd[i].Ms = (long)((toAdd[i].Ms - offset) / minDist * interval + offset);
                }

                Mapping.Current.Notes.Modify_Add("ADD PATTERN", toAdd);
            }
            catch (Exception ex)
            {
                Logging.Log("Failed to place pattern", LogSeverity.WARN, ex);
            }
        }



        public static void HorizontalFlip(Note note)
        {
            note.X = 2 - note.X;
        }

        public static void VerticalFlip(Note note)
        {
            note.Y = 2 - note.Y;
        }

        public static void Rotate(Note note, float deg)
        {
            double angle = MathHelper.RadiansToDegrees(Math.Atan2(note.Y - 1, note.X - 1));
            double distance = Math.Sqrt(Math.Pow(note.X - 1, 2) + Math.Pow(note.Y - 1, 2));
            double anglef = MathHelper.DegreesToRadians(angle + deg);

            note.X = (float)(Math.Cos(anglef) * distance + 1);
            note.Y = (float)(Math.Sin(anglef) * distance + 1);

            if (Settings.clampSR.Value)
            {
                note.X = Math.Clamp(note.X, -0.85f, 2.85f);
                note.Y = Math.Clamp(note.Y, -0.85f, 2.85f);
            }
        }

        public static void Scale(Note note, float scale)
        {
            float scalef = scale / 100f;

            note.X = (note.X - 1) * scalef + 1;
            note.Y = (note.Y - 1) * scalef + 1;

            if (Settings.clampSR.Value)
            {
                note.X = Math.Clamp(note.X, -0.85f, 2.85f);
                note.Y = Math.Clamp(note.Y, -0.85f, 2.85f);
            }
        }



        private static BigInteger FactorialApproximation(int num)
        {
            BigInteger result = BigInteger.One;

            if (num < 10)
            {
                for (int factor = 1; factor <= num; factor++)
                    result *= factor;
            }
            else
                result = (BigInteger)(Math.Sqrt(2 * Math.PI * num) * Math.Pow(num / Math.E, num));

            return result;
        }

        private static BigInteger BinomialCoefficient(int total, int choose)
        {
            return FactorialApproximation(total) / (FactorialApproximation(choose) * FactorialApproximation(total - choose));
        }

        public static List<Note> DrawBezier(List<Note> nodes, int divisor)
        {
            List<Note> final = [];

            int degree = nodes.Count - 1;

            if (Settings.curveBezier.Value)
            {
                decimal tIncrement = 1m / (divisor * degree);
                decimal deltaMs = nodes[degree].Ms - nodes[0].Ms;

                for (decimal t = 0; t <= 1 + tIncrement / 2m; t += tIncrement)
                {
                    float noteX = 0;
                    float noteY = 0;
                    decimal ms = nodes[0].Ms + deltaMs * t;

                    for (int point = 0; point <= degree; point++)
                    {
                        Note note = nodes[point];
                        double value = (double)BinomialCoefficient(degree, point) * (Math.Pow(1 - (double)t, degree - point) * Math.Pow((double)t, point));

                        noteX += (float)(value * note.X);
                        noteY += (float)(value * note.Y);
                    }

                    final.Add(new(noteX, noteY, (long)ms));
                }
            }
            else
            {
                decimal tIncrement = 1m / divisor;

                for (int point = 0; point < degree; point++)
                {
                    Note note = nodes[point];
                    Note nextnote = nodes[point + 1];
                    decimal deltaX = (decimal)(nextnote.X - note.X);
                    decimal deltaY = (decimal)(nextnote.Y - note.Y);
                    decimal deltaMs = nextnote.Ms - note.Ms;

                    for (decimal t = 0; t < 1 + tIncrement / 2m; t += tIncrement)
                    {
                        if (t > 0 || point == 0)
                        {
                            decimal x = (decimal)note.X + deltaX * t;
                            decimal y = (decimal)note.Y + deltaY * t;
                            decimal ms = note.Ms + deltaMs * t;

                            final.Add(new((float)x, (float)y, (long)ms));
                        }
                    }
                }
            }

            return final;
        }

        public static void RunBezier()
        {
            int divisor = (int)(Settings.bezierDivisor.Value + 0.5f);

            if (divisor > 0 && ((Mapping.Current.BezierNodes != null && Mapping.Current.BezierNodes.Count > 1)
                || Mapping.Current.Notes.Selected.Count > 1))
            {
                bool success = true;
                List<Note> nodes = [];

                if (Mapping.Current.BezierNodes != null && Mapping.Current.BezierNodes.Count > 1)
                {
                    for (int i = 0; i < Mapping.Current.BezierNodes.Count; i++)
                    {
                        Note note = Mapping.Current.Notes[Mapping.Current.BezierNodes[i]];
                        nodes.Add(note);
                    }
                }
                else
                    nodes = Mapping.Current.Notes.Selected;

                List<Note> result = [];

                List<int> anchored = [0];

                for (int i = 1; i < nodes.Count; i++)
                    if (nodes[i].Anchored)
                        anchored.Add(i);

                if (!anchored.Contains(nodes.Count - 1))
                    anchored.Add(nodes.Count - 1);

                for (int i = 1; i < anchored.Count; i++)
                {
                    List<Note> newNodes = [];

                    for (int j = anchored[i - 1]; j <= anchored[i]; j++)
                        newNodes.Add(nodes[j]);

                    try
                    {
                        List<Note> finalbez = DrawBezier(newNodes, divisor);
                        success = finalbez.Count > 0;

                        if (success)
                            for (int j = 1; j < finalbez.Count; j++)
                                result.Add(finalbez[j]);
                    }
                    catch (OverflowException)
                    {
                        GuiWindowEditor.ShowError("TOO MANY NODES");
                        return;
                    }
                    catch
                    {
                        GuiWindowEditor.ShowError("FAILED TO DRAW BEZIER");
                        return;
                    }
                }

                Mapping.Current.ClearSelected();

                result.Add(nodes[0]);

                if (success)
                    Mapping.Current.Notes.Modify_Replace("DRAW BEZIER", nodes, result);
            }

            foreach (Note note in Mapping.Current.Notes)
                note.Anchored = false;

            Mapping.Current.BezierNodes?.Clear();
        }
    }
}
