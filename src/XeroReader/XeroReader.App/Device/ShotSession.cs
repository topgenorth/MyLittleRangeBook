namespace net.opgenorth.xero.Device
{
    public class ShotSession
    {
        List<Shot> _shots = new();

        public DateTime SessionTimestamp { get; set; }
        public int ShotCount => _shots.Count;
        public float ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }
        public string Units { get; set;  }

        public float GetAverage
        {
            get
            {
                return _shots.Average(s => s.Speed);
            }
        }

        public float GetExtremeSpread()
        {
            return _shots.Max(s => s.Speed) - _shots.Min(s => s.Speed);
        }

        public double GetStandardDeviation()
        {
            double meanSquaredDistances = 0;

            var enumerable = _shots.ToArray();
            var datapointCount =  enumerable.Count();
            var mean = enumerable.Average(s => s.Speed);

            var squaredDistances = enumerable.Select(s => Math.Pow(Math.Abs(s.Speed - mean), 2)).ToList();

            meanSquaredDistances = squaredDistances.Sum() / datapointCount;

            return Math.Sqrt(meanSquaredDistances);
        }

        public IEnumerable<Shot> Shots => _shots;

        internal void AddShot(Shot shot)
        {
            _shots.Add(shot);
        }

        public override string ToString()
        {
            return $"{_shots.Count} shots, Average {GetAverage} {Units}, SD {GetStandardDeviation()})";
        }
    }
}
