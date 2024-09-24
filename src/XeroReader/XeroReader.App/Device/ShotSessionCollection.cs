using System.Collections;

namespace net.opgenorth.xero.Device
{
    public class ShotSessionCollection : ICollection<ShotSession>
    {
        public IEnumerator<ShotSession> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ShotSession item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ShotSession item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ShotSession[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ShotSession item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
    }
}
