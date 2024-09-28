using System;
using System.Collections.Generic;
using System.Linq;
using NanoidDotNet;

namespace net.opgenorth.xero.device
{
    public class ShotSession
    {
        readonly ShotCollection _shots = new ShotCollection();

        readonly uint _xeroSerialNumber;
        public ShotSession(XeroC1 xeroDevice)
        {
            _xeroSerialNumber = xeroDevice.SerialNumber;
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Units = "m/s";
        }

        public string Id { get; } = Nanoid.Generate();

        public string FileName { get; set; }

        public DateTime SessionTimestamp { get; set; }
        public int ShotCount => _shots.Count;
        public float ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }
        public string Units { get; set; }

        public float GetAverage => _shots.Average(s => s.Speed);

        public float ExtremeSpread => _shots.Max(s => s.Speed) - _shots.Min(s => s.Speed);

        public double StandardDeviation => _shots.StandardDeviation; 

        public IEnumerable<Shot> Shots => _shots;

        public void AddShot(Shot shot)
        {
            if (_shots.Contains(shot))
            {
                // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
                throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
            }
            _shots.Add(shot);
        }

        public override string ToString()
        {
            return $"{Id} {_shots.Count} shots, Average {GetAverage:F0} {Units}, SD {_shots.StandardDeviation:F1} {Units})";
        }
    }
}
