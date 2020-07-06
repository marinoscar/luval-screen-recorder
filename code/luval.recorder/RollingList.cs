using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luval.recorder
{
    public class RollingList<T> : IEnumerable<T>
    {
        private List<T> _list;
        public RollingList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public int Capacity { get { return _list.Capacity;  } }
        public int Count => _list.Count;

        public void Add(T value)
        {
            if (_list.Count == Capacity)
            {
                _list.RemoveAt(0);
            }
            _list.Add(value);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();

                return _list[index];
            }
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
