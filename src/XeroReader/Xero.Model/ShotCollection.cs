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

        public uint XeroSerialNumber { get; internal set; }

        public ShotCollection()
        {
            XeroSerialNumber = 0;
        }
        public ShotCollection(uint xeroSerialNumber)
        {
            XeroSerialNumber=xeroSerialNumber;
        }
        public IEnumerator<Shot> GetEnumerator()
        {
            return _shots.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Shot this[int i] => _shots[i];
     
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
                return;
            }
            _shots.Values.CopyTo(array, arrayIndex);
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

        public ShotSpeed ExtremeSpread => MaxSpeed - MinSpeed;

        public ShotSpeed MinSpeed
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

        public ShotSpeed AverageSpeed
        {
            get
            {
                var units = _shots[0].Speed.Units;
                var avg = _shots.Values.Average(s => s.Speed.Value);

                return new ShotSpeed(avg, units);
            }
        }

        public override string ToString()
        {
            var units = _shots[0].Speed.Units;
            return
                $"{_shots.Count} shots: Avg {AverageSpeed}{units}, SD {StandardDeviation}{units}, ES {ExtremeSpread}{units}";
        }
    }
}
