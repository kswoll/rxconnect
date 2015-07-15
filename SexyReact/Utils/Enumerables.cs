using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SexyReact.Utils
{
    public class Enumerables
    {
        public static IEnumerable<T> Return<T>(T value)
        {
            return new ReturnEnumerable<T>(value);
        }

        private struct ReturnEnumerable<T> : IEnumerable<T>
        {
            private readonly T value;

            public ReturnEnumerable(T value) : this()
            {
                this.value = value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new ReturnEnumerator<T>(value);
            }
        }

        private struct ReturnEnumerator<T> : IEnumerator<T>
        {
            private T value;
            private bool hasReadValue;

            public ReturnEnumerator(T value) : this()
            {
                this.value = value;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (!hasReadValue)
                {
                    hasReadValue = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                hasReadValue = false;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public T Current
            {
                get { return value; }
            }
        }
    }
}
