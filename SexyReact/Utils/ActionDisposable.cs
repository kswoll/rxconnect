using System;

namespace SexyReact.Utils
{
    public class ActionDisposable : IDisposable
    {
        private readonly Action action;
        private bool disposed;
        
        public ActionDisposable(Action action)
        {
            this.action = action;
        }

        public void Dispose() 
        {
            if (!disposed) 
            {
                disposed = true;
                action();
            }
        }
    }
}

