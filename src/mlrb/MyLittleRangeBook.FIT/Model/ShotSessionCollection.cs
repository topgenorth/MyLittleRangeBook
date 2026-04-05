using System.Collections;

namespace MyLittleRangeBook.FIT.Model
{
    public class ShotSessionCollection : ICollection<ShotSession>
    {
        readonly List<ShotSession> _sessions;

        internal ShotSessionCollection()
        {
            _sessions = [];
        }

        public IEnumerator<ShotSession> GetEnumerator()
        {
            return _sessions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ShotSession item)
        {
            // TODO [TO20240929] should check that we're not adding a duplicate.
            if (_sessions.Contains(item))
            {
                throw new ArgumentException("Duplicate shot session cannot be added", nameof(item));
            }
            else
            {
                _sessions.Add(item);
            }
        }

        public void Clear()
        {
            _sessions.Clear();
        }

        public bool Contains(ShotSession item)
        {
            return _sessions.Contains(item);
        }

        public void CopyTo(ShotSession[] array, int arrayIndex)
        {
            _sessions.CopyTo(array, arrayIndex);
        }

        public bool Remove(ShotSession item)
        {
            return _sessions.Remove(item);
        }

        public int Count => _sessions.Count;
        public bool IsReadOnly => false;


    }
}
