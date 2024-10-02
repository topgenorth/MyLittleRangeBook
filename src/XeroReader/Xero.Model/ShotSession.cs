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

        public ShotSession()
        {
            Id = Nanoid.Generate();
            _xeroSerialNumber = 0;
            FileName = string.Empty;
            ProjectileType = "Rifle";
        }

        public ShotSession(XeroC1 xeroDevice) : this()
        {
            _xeroSerialNumber = xeroDevice.SerialNumber;
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

        public void AddShot(Shot shot)
        {
            if (_shots.Contains(shot))
                // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
            {
                throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
            }

            _shots.Add(shot);
        }

        public override string ToString()
        {
            return
                $"{Id}: {_shots}";
        }
    }
}
