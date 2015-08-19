using System.Runtime.CompilerServices;

namespace SexyReact
{
    public interface IRxObjectMixin : IRxObject
    {
        TValue Get<TValue>([CallerMemberName]string propertyName = null);
        void Set<TValue>(TValue newValue, [CallerMemberName]string propertyName = null);
    }
}

