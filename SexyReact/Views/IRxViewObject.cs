namespace SexyReact.Views
{
    public interface IRxViewObject : IRxObject
    {
        object Model { get; set; }
    }

    public interface IRxViewObject<T> : IRxViewObject
        where T : IRxObject
    {
        new T Model { get; set; }
    }
}

