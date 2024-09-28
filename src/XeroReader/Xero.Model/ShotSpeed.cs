using System;

namespace net.opgenorth.xero.device
{
    public struct ShotSpeed
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
            return $"{Value} {Units}";
        }

        
        public static ShotSpeed operator -(ShotSpeed s)
        {
            return new ShotSpeed(-1*s.Value, s.Units);
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
    }
}
