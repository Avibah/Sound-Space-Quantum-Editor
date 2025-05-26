using New_SSQE.Objects;

namespace New_SSQE.NewMaps.Parsing
{
    internal class FormatUtils
    {
        public static string FixID(string id)
        {
            id = id.ToLower();
            char[] fix = new char[id.Length];

            for (int i = 0; i < id.Length; i++)
            {
                fix[i] = id[i] switch
                {
                    >= '0' and <= '9' => id[i],
                    >= 'a' and <= 'z' => id[i],
                    >= 'A' and <= 'Z' => char.ToLower(id[i]),
                    ' ' or '-' => id[i],
                    _ => '_'
                };
            }

            return new(fix);
        }

        public static Dictionary<string, byte> Difficulties = new()
        {
            {"N/A", 0x00 },
            {"Easy", 0x01 },
            {"Medium", 0x02 },
            {"Hard", 0x03 },
            {"Logic", 0x04 },
            {"Tasukete", 0x05 }
        };

        // true: vfx, false: special
        public static readonly bool[] VfxLookup = 
        [
            false, // 0
            false, // 1
            true,  // 2
            true,  // 3
            true,  // 4
            true,  // 5
            true,  // 6
            true,  // 7
            true,  // 8
            true,  // 9
            true,  // 10
            true,  // 11
            false, // 12
            false, // 13
            false, // 14
            false, // 15
        ];

        public static (List<MapObject>, List<MapObject>) SplitVFXSpecial(List<MapObject> objects)
        {
            List<MapObject> vfx = [];
            List<MapObject> special = [];

            foreach (MapObject obj in objects)
            {
                if (VfxLookup[obj.ID])
                    vfx.Add(obj);
                else
                    special.Add(obj);
            }

            return (vfx, special);
        }

        private static long encodeDiff = 0;
        private static long encodePrev = 0;

        public static long EncodeTimestamp(long ms)
        {
            long diff = ms - encodePrev;
            long offset = diff - encodeDiff;

            encodeDiff = diff;
            encodePrev = ms;

            return offset;
        }

        public static void ResetEncode()
        {
            encodeDiff = 0;
            encodePrev = 0;
        }

        private static long decodeTotal = 0;
        private static long decodePrev = 0;

        public static long DecodeTimestamp(long ms)
        {
            decodePrev += ms;
            decodeTotal += decodePrev;

            return decodeTotal;
        }

        public static void ResetDecode()
        {
            decodeTotal = 0;
            decodePrev = 0;
        }

        public static string ConvertString(string str)
        {
            return $"\"{str.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
