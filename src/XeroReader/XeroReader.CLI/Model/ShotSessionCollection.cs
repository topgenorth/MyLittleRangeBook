using System.Collections;

namespace net.opgenorth.xero.Model;

public class ShotSessionCollection : ICollection<ShotSession>
{
    private readonly List<ShotSession> _sessions;

    internal ShotSessionCollection() => _sessions = new List<ShotSession>();

    public IEnumerator<ShotSession> GetEnumerator() => _sessions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(ShotSession item) =>
        // TODO [TO20240929] should check that we're not adding a duplicate.
        _sessions.Add(item);

    public void Clear() => _sessions.Clear();

    public bool Contains(ShotSession item) => _sessions.Contains(item);

    public void CopyTo(ShotSession[] array, int arrayIndex) => _sessions.CopyTo(array, arrayIndex);

    public bool Remove(ShotSession item) => _sessions.Remove(item);

    public int Count => _sessions.Count;
    public bool IsReadOnly => false;

    /// <summary>
    ///     This will combine all the shot sessions returning a single <c cref="ShotSession" />.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ShotSession MergeAll() => throw new NotImplementedException();
}
