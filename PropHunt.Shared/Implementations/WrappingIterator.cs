using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Implementations
{
    public sealed class WrappingIterator<T>
    {
        private IList<T> _list;
        private int _index;

        public WrappingIterator(IList<T> list, int index)
        {
            _list = list;
            _index = index;
        }

        public T GetNext()
        {
            _index++;
            if (_index >= _list.Count)
            {
                _index = 0;
            }
            return _list[_index];
        }

        public T GetPrevious()
        {
            _index--;
            if (_index < 0)
            {
                _index = _list.Count - 1;
            }
            return _list[_index];
        }

        public static WrappingIterator<T> CreateAt(IList<T> list, T value)
        {
            var index = list.IndexOf(value);
            return new WrappingIterator<T>(list, index);
        }
    }
}
