using System.Collections;

namespace net.opgenorth.xero.Device
{
    public class ShotCollection : ICollection<Shot>
    {
        readonly SortedDictionary<int, Shot> _shots = new();

        public IEnumerator<Shot> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Shot item)
        {
            _shots[item.ShotNumber] = item;
        }

        public void Clear()
        {
            _shots.Clear();
        }

        public bool Contains(Shot item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Shot[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Shot item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
    }
}
