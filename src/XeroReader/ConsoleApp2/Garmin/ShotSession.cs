namespace ConsoleApp2.Garmin;

public class ShotSession
{
    List<Shot> _shots;
    public DateTime SessionTimestamp { get; set; }
    public int ShotCount => _shots.Count;
    public float ProjectileWeight { get; set; }
    public string ProjectileType { get; set; }
    public string UnitsOfMeasure { get; set; }
}

public class Shot
{
    public int ShotNumber { get; set; }
    
}
