using NanoidDotNet;

namespace net.opgenorth.xero.Device
{
    public class ShotSession
    {
        readonly List<Shot> _shots = new();

        public ShotSession()
        {
            FileName = string.Empty;
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

        public double StandardDeviation
        {
            get
            {
                var enumerable = _shots.ToArray();
                var mean = enumerable.Average(s => s.Speed);
                var squaredDistances = enumerable.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();
                var shotCount = enumerable.Count();
                var meanSquaredDistances = squaredDistances.Sum() / shotCount;

                return Math.Sqrt(meanSquaredDistances);
            }
        }

        public IEnumerable<Shot> Shots => _shots;

        internal void AddShot(Shot shot)
        {
            _shots.Add(shot);
        }

        public override string ToString()
        {
            return $"{Id} {_shots.Count} shots, Average {GetAverage:F0} {Units}, SD {StandardDeviation:F1} {Units})";
        }
    }
}
