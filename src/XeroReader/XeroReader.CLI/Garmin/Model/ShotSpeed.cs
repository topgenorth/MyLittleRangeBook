namespace net.opgenorth.xero.Garmin.Model
{
    public struct ShotSpeed : IEquatable<ShotSpeed>, IComparable<ShotSpeed>
    {
        public static readonly ShotSpeed Zero = new(0, "fps");

        public ShotSpeed(double value, string units = "m/s")
        {
            Value = Convert.ToInt32(value);
            Units = units;
        }

        public ShotSpeed(float value, string units = "m/s")
        {
            Value = Convert.ToInt32(value);
            Units = units;
        }

        public ShotSpeed(int value, string units = "m/s")
        {
            Value = value;
            Units = units;
        }

        public int Value { get; internal set; }
        public string Units { get; internal set; }

        public override string ToString() => $"{Value:F1}{Units}";

        public static ShotSpeed operator -(ShotSpeed s) => new(-1 * s.Value, s.Units);

        public static ShotSpeed operator -(ShotSpeed s1, ShotSpeed s2)
        {
            if (s1.Units != s2.Units)
            {
                throw new ArgumentException("Cannot add two ShotSpeeds of different units");
            }

            int v = s1.Value - s2.Value;

            return new ShotSpeed(v, s1.Units);
        }

        public static ShotSpeed operator +(ShotSpeed s1, ShotSpeed s2)
        {
            if (s1.Units != s2.Units)
            {
                throw new ArgumentException("Cannot add two ShotSpeeds of different units");
            }

            int v = s1.Value + s2.Value;

            return new ShotSpeed(v, s1.Units);
        }

        public static implicit operator float(ShotSpeed s) => s.Value;

        public bool Equals(ShotSpeed other) => Value.Equals(other.Value) && Units == other.Units;

        public int CompareTo(ShotSpeed other)
        {
            if (other.Units != Units)
            {
                throw new ArgumentException("Cannot compare shots with two different units of measure");
            }

            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object? obj) => obj is ShotSpeed other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Value, Units);

        public static bool operator ==(ShotSpeed left, ShotSpeed right) => left.Equals(right);

        public static bool operator !=(ShotSpeed left, ShotSpeed right) => !left.Equals(right);
    }
}
