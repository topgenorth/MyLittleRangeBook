using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace net.opgenorth.xero.device
{
    /// <summary>
    ///     This class contains a sorted list of Shots.
    /// </summary>
    public class ShotCollection : ICollection<Shot>
    {
        readonly SortedDictionary<int, Shot> _shots = new SortedDictionary<int, Shot>();

        public ShotCollection()
        {
            XeroSerialNumber = 0;
        }

        public ShotCollection(uint xeroSerialNumber)
        {
            XeroSerialNumber = xeroSerialNumber;
        }

        public uint XeroSerialNumber { get; internal set; }

        public Shot this[int i] => _shots[i];

        public IEnumerable<Shot> ActiveShots => from s in _shots where !s.Value.IgnoreShot select s.Value;
        public string Units => _shots.Any() ? _shots.Values.First().Speed.Units : "m/s";

        public ShotSpeed StandardDeviation
        {
            get
            {
                var shotValues = ActiveShots.ToArray();
                if (!shotValues.Any())
                {
                    return ShotSpeed.Zero;
                }
        
                var mean = shotValues.Average(s => s.Speed);
                var squaredDistances = shotValues.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();
                var shotCount = shotValues.Count();
                var meanSquaredDistances = squaredDistances.Sum() / shotCount;

                var speed = Math.Sqrt(meanSquaredDistances);
                return new ShotSpeed((float)speed, Units);
            }
        }

        public ShotSpeed ExtremeSpread => MaxSpeed - MinSpeed;

        public ShotSpeed MinSpeed
        {
            get { return ActiveShots.Any()?  ActiveShots.Min(s => s.Speed) : ShotSpeed.Zero ; }
        }

        public ShotSpeed MaxSpeed
        {
            get { return ActiveShots.Any() ? ShotSpeed.Zero : ActiveShots.Max(s => s.Speed); }
        }

        public ShotSpeed AverageSpeed
        {
            get
            {
                if (!ActiveShots.Any())
                {
                    return ShotSpeed.Zero;
                }

                var units = _shots.First().Value.Speed.Units;
                var avg = ActiveShots.Select(s=> s).Average(s => s.Speed.Value);

                return new ShotSpeed(avg, units);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>All of the shots, ignored or not.</returns>
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
                return;
            }

            _shots.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(Shot item)
        {
            return _shots.Remove(item.ShotNumber);
        }

        public int Count => ActiveShots.Count();

        public bool IsReadOnly => false;

        public override string ToString()
        {
            return _shots.Any()
                ? $"{Count} shots, Average {AverageSpeed}, SD {StandardDeviation}, ES {ExtremeSpread}, Max {MaxSpeed}, Min {MinSpeed}"
                : "No shots";
        }
    }
}
