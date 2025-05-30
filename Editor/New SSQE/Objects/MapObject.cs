﻿namespace New_SSQE.Objects
{
    internal class MapObject
    {
        public int ID;
        public long Ms { get; set; }
        public long Duration;

        public string? Name;

        // used to cache data in case an id doesnt exist on an older version
        public string[] ExtraData;

        public bool Selected;
        public long DragStartMs;

        public bool HasDuration = true;
        public bool PlayHitsound = true;

        public MapObject(int id, long ms, string? name = null)
        {
            ID = id;
            Ms = ms;

            Name = name;

            ExtraData = [];
        }

        public virtual string ToString(params object[] data)
        {
            data = data.Concat(ExtraData).ToArray();
            return data.Length > 0 ? $"{Ms}|{string.Join('|', data)}" : Ms.ToString();
        }

        public virtual MapObject Clone()
        {
            return new(ID, Ms, Name)
            {
                ExtraData = ExtraData
            };
        }
    }
}
