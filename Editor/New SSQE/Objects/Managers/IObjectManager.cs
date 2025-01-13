namespace New_SSQE.Objects.Managers
{
    internal interface IObjectManager<T> where T : MapObject
    {
        static abstract void Replace(string label, List<T> oldObjects, List<T> newObjects);

        static abstract void Edit(string label, List<T> toModify, Action<T> action);
        static abstract void Edit(string label, Action<T> action);

        static abstract void Add(string label, List<T> toAdd);
        static abstract void Add(string label, T toAdd);

        static abstract void Remove(string label, List<T> toRemove);
        static abstract void Remove(string label, T toRemove);
        static abstract void Remove(string label);
    }
}
