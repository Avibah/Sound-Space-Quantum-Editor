using New_SSQE.GUI;
using New_SSQE.Objects.Managers;
using New_SSQE.Objects;
using New_SSQE.Preferences;
using OpenTK.Mathematics;
using System.Numerics;
using System.Drawing;

namespace New_SSQE.Maps
{
    internal static class Patterns
    {
        public static void StorePattern(int index)
        {
            string pattern = "";
            long minDist = 0;

            for (int i = 0; i + 1 < CurrentMap.Notes.Selected.Count; i++)
            {
                long dist = Math.Abs(CurrentMap.Notes.Selected[i].Ms - CurrentMap.Notes.Selected[i + 1].Ms);

                if (dist > 0)
                    minDist = minDist > 0 ? Math.Min(minDist, dist) : dist;
            }

            foreach (Note note in CurrentMap.Notes.Selected)
            {
                long offset = CurrentMap.Notes.Selected[0].Ms;

                string x = note.X.ToString(Program.Culture);
                string y = note.Y.ToString(Program.Culture);
                string time = (minDist > 0 ? Math.Round((double)(note.Ms - offset) / minDist) : 0).ToString();

                pattern += $",{x}|{y}|{time}";
            }

            if (pattern.Length > 0)
                pattern = pattern[1..];

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
                editor.ShowToast($"BOUND PATTERN {index}", Settings.color1.Value);

            Settings.patterns.Value[index] = pattern;
        }

        public static void ClearPattern(int index)
        {
            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
            {
                Settings.patterns.Value[index] = "";
                editor.ShowToast($"UNBOUND PATTERN {index}", Settings.color1.Value);
            }
        }

        public static void RecallPattern(int index)
        {
            string pattern = Settings.patterns.Value[index];

            if (pattern == "")
                return;

            string[] patternSplit = pattern.Split(',');
            List<Note> toAdd = new();

            foreach (string note in patternSplit)
            {
                string[] noteSplit = note.Split('|');
                float x = float.Parse(noteSplit[0], Program.Culture);
                float y = float.Parse(noteSplit[1], Program.Culture);
                int time = int.Parse(noteSplit[2]);
                long ms = Timing.GetClosestBeatScroll((long)Settings.currentTime.Value.Value, false, time);

                toAdd.Add(new(x, y, ms));
            }

            NoteManager.Add("ADD PATTERN", toAdd);
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
            List<Note> final = new();

            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
            {
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
                            if (t > 0)
                            {
                                decimal x = (decimal)note.X + deltaX * t;
                                decimal y = (decimal)note.Y + deltaY * t;
                                decimal ms = note.Ms + deltaMs * t;

                                final.Add(new((float)x, (float)y, (long)ms));
                            }
                        }
                    }
                }
            }

            return final;
        }

        public static void RunBezier()
        {
            if (MainWindow.Instance.CurrentWindow is GuiWindowEditor editor)
            {
                int divisor = (int)(Settings.bezierDivisor.Value + 0.5f);

                if (divisor > 0 && ((CurrentMap.BezierNodes != null && CurrentMap.BezierNodes.Count > 1) || CurrentMap.Notes.Selected.Count > 1))
                {
                    bool success = true;
                    List<Note> nodes = CurrentMap.BezierNodes != null && CurrentMap.BezierNodes.Count > 1 ? CurrentMap.BezierNodes.ToList() : CurrentMap.Notes.Selected.ToList();
                    List<Note> result = new();

                    List<int> anchored = new() { 0 };

                    for (int i = 0; i < nodes.Count; i++)
                        if (nodes[i].Anchored && !anchored.Contains(i))
                            anchored.Add(i);

                    if (!anchored.Contains(nodes.Count - 1))
                        anchored.Add(nodes.Count - 1);

                    for (int i = 1; i < anchored.Count; i++)
                    {
                        List<Note> newNodes = new();

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
                            editor.ShowToast("TOO MANY NODES", Color.FromArgb(255, 255, 200, 0));
                            return;
                        }
                        catch
                        {
                            editor.ShowToast("FAILED TO DRAW BEZIER", Color.FromArgb(255, 255, 200, 0));
                            return;
                        }
                    }

                    CurrentMap.Notes.ClearSelection();
                    CurrentMap.SelectedPoint = null;

                    result.Add(nodes[0]);

                    if (success)
                        NoteManager.Replace("DRAW BEZIER", nodes, result);
                }

                foreach (Note note in CurrentMap.Notes)
                    note.Anchored = false;

                CurrentMap.BezierNodes?.Clear();
            }
        }
    }
}
