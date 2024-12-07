using System;
using System.Collections.Generic;
using System.Linq;
using NanoidDotNet;

namespace net.opgenorth.xero.device
{
    /// <summary>
    /// A session holds data about a single Xero shooting session.
    /// </summary>
    public class ShotSession
    {
        readonly ShotCollection _shots = [];

        readonly uint _xeroSerialNumber;

        public ShotSession()
        {
            Id = Nanoid.Generate();
            _xeroSerialNumber = 0;
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
        }

        public string Id { get; }

        public string FileName { get; set; }

        public DateTime SessionTimestamp { get; set; }
        public int ShotCount => _shots.Count;
        public float ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }
        public string Units => _shots.MaxSpeed.Units;

        public float AverageSpeed => _shots.AverageSpeed;

        public float ExtremeSpread => _shots.Max(s => s.Speed) - _shots.Min(s => s.Speed);

        public double StandardDeviation => _shots.StandardDeviation;

        public IEnumerable<Shot> Shots => _shots;
        public uint SerialNumber { get; set; }

        public string Notes { get; set; }

        public void AddShot(Shot shot)
        {
            if (_shots.Contains(shot))
                // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
            {
                throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
            }

            _shots.Add(shot);
        }

        public override string ToString() => $"{Id}: {_shots}";
    }
}
