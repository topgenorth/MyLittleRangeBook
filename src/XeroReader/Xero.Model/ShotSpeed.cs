using System;

namespace net.opgenorth.xero.device
{
    public struct ShotSpeed : IEquatable<ShotSpeed>, IComparable<ShotSpeed>
    {
        public static readonly ShotSpeed Zero = new ShotSpeed(0f);

        public ShotSpeed(float value, string units = "m/s")
        {
            Value = value;
            Units = units;
        }

        public float Value { get; internal set; }
        public string Units { get; internal set; }

        public override string ToString()
        {
            return $"{Value:F1}{Units}";
        }

        public static ShotSpeed operator -(ShotSpeed s)
        {
            return new ShotSpeed(-1 * s.Value, s.Units);
        }

        public static ShotSpeed operator -(ShotSpeed s1, ShotSpeed s2)
        {
            if (s1.Units != s2.Units)
            {
                throw new ArgumentException("Cannot add two ShotSpeeds of different units");
            }

            var v = s1.Value - s2.Value;

            return new ShotSpeed(v, s1.Units);
        }

        public static ShotSpeed operator +(ShotSpeed s1, ShotSpeed s2)
        {
            if (s1.Units != s2.Units)
            {
                throw new ArgumentException("Cannot add two ShotSpeeds of different units");
            }

            var v = s1.Value + s2.Value;

            return new ShotSpeed(v, s1.Units);
        }

        public static implicit operator float(ShotSpeed s)
        {
            return s.Value;
        }

        public bool Equals(ShotSpeed other)
        {
            return Value.Equals(other.Value) && Units == other.Units;
        }

        public int CompareTo(ShotSpeed other)
        {
            if (other.Units != Units)
            {
                throw new ArgumentException("Cannot compare shots with two different units of measure");
            }

            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is ShotSpeed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Units);
        }

        public static bool operator ==(ShotSpeed left, ShotSpeed right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShotSpeed left, ShotSpeed right)
        {
            return !left.Equals(right);
        }
    }
}
