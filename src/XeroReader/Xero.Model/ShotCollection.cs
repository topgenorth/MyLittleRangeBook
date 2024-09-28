using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace net.opgenorth.xero.device
{
    /// <summary>
    /// This class contains a sorted list of Shots.
    /// </summary>
    public class ShotCollection : ICollection<Shot>
    {
        readonly SortedDictionary<int, Shot> _shots = new SortedDictionary<int, Shot>();

        public IEnumerator<Shot> GetEnumerator()
        {
            return _shots.Values.GetEnumerator();
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
            return _shots.ContainsKey(item.ShotNumber);
        }

        public void CopyTo(Shot[] array, int arrayIndex)
        {
            if (_shots.Count == 0)
            {
                array = Array.Empty<Shot>();
                return;
            }

            var maxIndex = _shots.Count - 1;
            if (arrayIndex >maxIndex)
            {
                throw new IndexOutOfRangeException($"There are only {_shots.Count} Shots in the collection.");
            }
            
            array = new Shot[_shots.Count];
            for (var i = arrayIndex; i < _shots.Count; i++)
            {
                var shot = _shots[i];
                array[i] = shot;
            }
        }

        public bool Remove(Shot item)
        {
            return _shots.Remove(item.ShotNumber);
        }

        public int Count => _shots.Count;

        public bool IsReadOnly => false;

        public double StandardDeviation
        {
            get
            {
                var shotValues = _shots.Values.ToArray();
                var mean = shotValues.Average(s => s.Speed);
                var squaredDistances = shotValues.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();
                var shotCount = shotValues.Count();
                var meanSquaredDistances = squaredDistances.Sum() / shotCount;

                return Math.Sqrt(meanSquaredDistances);
            }
        }

        public ShotSpeed ExtremeSpread
        {
            get
            {
                return MaxSpeed - MinimumSpeed;
            }
        }
        public ShotSpeed MinimumSpeed
        {
            get
            {
                
                return _shots.Count == 0 ? ShotSpeed.Zero : _shots.Values.OrderBy(s => s.Speed).Last().Speed;
            }
        }
        public ShotSpeed MaxSpeed
        {
            get
            {
                return _shots.Count == 0 ? ShotSpeed.Zero : _shots.Values.OrderBy(s => s.Speed).First().Speed;
            }
        }
    }
}
