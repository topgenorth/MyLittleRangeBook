using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace net.opgenorth.xero.device;

/// <summary>
///     This class contains a sorted list of Shots.
/// </summary>
public class ShotCollection : ICollection<Shot>
{
    private readonly SortedDictionary<int, Shot> _shots = new();

    public ShotCollection() => XeroSerialNumber = 0;

    public ShotCollection(uint xeroSerialNumber) => XeroSerialNumber = xeroSerialNumber;

    public uint XeroSerialNumber { get; internal set; }

    public Shot this[int i] => _shots[i];

    public IEnumerable<Shot> ActiveShots => from s in _shots where !s.Value.IgnoreShot select s.Value;
    public string Units => _shots.Any() ? _shots.Values.First().Speed.Units : "m/s";

    public ShotSpeed StandardDeviation
    {
        get
        {
            Shot[] shotValues = ActiveShots.ToArray();
            if (!shotValues.Any())
            {
                return ShotSpeed.Zero;
            }

            float mean = shotValues.Average(s => s.Speed);
            List<double> squaredDistances = shotValues.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();
            int shotCount = shotValues.Count();
            double meanSquaredDistances = squaredDistances.Sum() / shotCount;

            double speed = Math.Sqrt(meanSquaredDistances);

            return new ShotSpeed((float)speed, Units);
        }
    }

    public ShotSpeed ExtremeSpread => MaxSpeed - MinSpeed;

    public ShotSpeed MinSpeed => ActiveShots.Any() ? ActiveShots.Min(s => s.Speed) : ShotSpeed.Zero;

    public ShotSpeed MaxSpeed => ActiveShots.Any() ? ShotSpeed.Zero : ActiveShots.Max(s => s.Speed);

    public ShotSpeed AverageSpeed
    {
        get
        {
            if (!ActiveShots.Any())
            {
                return ShotSpeed.Zero;
            }

            string units = _shots.First().Value.Speed.Units;
            double avg = ActiveShots.Select(s => s).Average(s => s.Speed.Value);

            return new ShotSpeed(avg, units);
        }
    }

    /// <summary>
    /// </summary>
    /// <returns>All of the shots, ignored or not.</returns>
    public IEnumerator<Shot> GetEnumerator() => _shots.Values.GetEnumerator();


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Shot item) => _shots[item.ShotNumber] = item;

    public void Clear() => _shots.Clear();

    public bool Contains(Shot item) => _shots.ContainsKey(item.ShotNumber);

    public void CopyTo(Shot[] array, int arrayIndex)
    {
        if (_shots.Count == 0)
        {
            return;
        }

        _shots.Values.CopyTo(array, arrayIndex);
    }

    public bool Remove(Shot item) => _shots.Remove(item.ShotNumber);

    public int Count => ActiveShots.Count();

    public bool IsReadOnly => false;

    public override string ToString() => _shots.Any()
        ? $"{Count} shots, Average {AverageSpeed}, SD {StandardDeviation}, ES {ExtremeSpread}, Max {MaxSpeed}, Min {MinSpeed}"
        : "No shots";
}
