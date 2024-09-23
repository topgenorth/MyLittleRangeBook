namespace net.opgenorth.xero.Device
{
    public class ShotSession
    {
        List<Shot> _shots;

        public DateTime SessionTimestamp { get; set; }
        public int ShotCount => _shots.Count;
        public float ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }
    }
}
