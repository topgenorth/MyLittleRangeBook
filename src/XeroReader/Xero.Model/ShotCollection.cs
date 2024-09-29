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

        public string Units => _shots.Any() ? _shots.Values.First().Speed.Units : "m/s";

        public ShotSpeed StandardDeviation
        {
            get
            {
                if (!_shots.Any()) return ShotSpeed.Zero;
                
                var shotValues = _shots.Values.ToArray();
                var mean = shotValues.Average(s => s.Speed);
                var squaredDistances = shotValues.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();
                var shotCount = shotValues.Count();
                var meanSquaredDistances = squaredDistances.Sum() / shotCount;

                var speed =  Math.Sqrt(meanSquaredDistances);
                return new ShotSpeed((float) speed , Units);
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
                return _shots.Count == 0 ? ShotSpeed.Zero : _shots.Values.Max(s => s.Speed);
            }
        }

        public ShotSpeed AverageSpeed
        {
            get
            {
                if (!_shots.Any()) return ShotSpeed.Zero;
                var units = _shots[1].Speed.Units;
                var avg = _shots.Values.Average(s => s.Speed.Value);
                return new ShotSpeed(avg, units);

            }
        }

        public override string ToString()
        {
            return _shots.Any() ? 
                $"{_shots.Count} shots, Average {AverageSpeed}, SD {StandardDeviation}, ES {ExtremeSpread}" 
                : "No shots";
        }
    }
}
