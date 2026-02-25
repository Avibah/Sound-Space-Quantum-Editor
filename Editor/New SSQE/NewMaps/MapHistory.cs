using New_SSQE.Misc;
using New_SSQE.Preferences;

namespace New_SSQE.NewMaps
{
    internal class MapHistory
    {
        private readonly string prefix;

        private string NameTXT(int index) => Assets.HistoryAt(index == 0 ? $"{prefix}_latest.txt" : $"{prefix}_cache_{index}.txt");
        private string NameINI(int index) => Assets.HistoryAt(index == 0 ? $"{prefix}_latest.ini" : $"{prefix}_cache_{index}.ini");

        private int curExists = 0;

        public MapHistory(string prefix)
        {
            this.prefix = prefix;
            bool[] exists = new bool[(int)Settings.maxMapHistory.Value];

            for (int i = 0; i < Settings.maxMapHistory.Value; i++)
            {
                if (File.Exists(NameTXT(i)))
                {
                    exists[i] = true;
                    curExists++;

                    for (int j = 0; j < i; j++)
                    {
                        if (!exists[j])
                        {
                            File.Move(NameTXT(i), NameTXT(j), true);
                            if (File.Exists(NameINI(i)))
                                File.Move(NameINI(i), NameINI(j), true);
                            else if (File.Exists(NameINI(j)))
                                File.Delete(NameINI(j));

                            exists[j] = true;
                            exists[i] = false;
                            break;
                        }
                    }
                }
            }
        }

        public void Store(string path)
        {
            if (!File.Exists(path))
                return;

            if (curExists >= Settings.maxMapHistory.Value)
            {
                File.Delete(NameTXT(curExists - 1));
                File.Delete(NameINI(curExists - 1));
            }

            for (int i = curExists - 1; i >= 0; i--)
            {
                if (File.Exists(NameTXT(i)))
                    File.Move(NameTXT(i), NameTXT(i + 1), true);
                if (File.Exists(NameINI(i)))
                    File.Move(NameINI(i), NameINI(i + 1), true);
            }

            curExists = Math.Min(curExists + 1, (int)Settings.maxMapHistory.Value);
            File.Copy(path, NameTXT(0), true);
            string ini = Path.ChangeExtension(path, ".ini");
            if (File.Exists(ini))
                File.Copy(ini, NameINI(0), true);
        }
    }
}
