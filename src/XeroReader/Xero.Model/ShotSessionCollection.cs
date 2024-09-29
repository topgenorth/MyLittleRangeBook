using System;
using System.Collections;
using System.Collections.Generic;

namespace net.opgenorth.xero.device
{
    public class ShotSessionCollection : ICollection<ShotSession>
    {
        readonly List<ShotSession> _sessions;

        internal ShotSessionCollection()
        {
            _sessions = new List<ShotSession>();
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
            _sessions.Add(item);
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

        public void MergeAll()
        {
            throw new NotImplementedException();
        }

        public int Count => _sessions.Count;
        public bool IsReadOnly => false;
    }
}