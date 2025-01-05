using System;
using System.Collections.Generic;
using System.Linq;
using NanoidDotNet;

namespace net.opgenorth.xero.device;

/// <summary>
///     A session holds data about a single Xero shooting session.
/// </summary>
public class ShotSession
{
    private readonly ShotCollection _shots = [];

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private readonly uint _xeroSerialNumber;
#pragma warning restore CS0414 // Field is assigned but its value is never used

    public ShotSession()
    {
        Id = Nanoid.Generate();
        _xeroSerialNumber = 0;
        FileName = string.Empty;
        ProjectileType = "Rifle";
        Notes = string.Empty;
        ProjectileUnits = "grains";
        VelocityUnits = "fps";
    }

    public string Id { get; set; }

    public string FileName { get; set; }

    public DateTime DateTimeUtc { get; set; }
    public int ShotCount => _shots.Count;
    public int ProjectileWeight { get; set; }
    public string ProjectileType { get; set; }
    public string Units => _shots.MaxSpeed.Units;

    public float AverageSpeed => _shots.AverageSpeed;

    public float ExtremeSpread => _shots.Max(s => s.Speed) - _shots.Min(s => s.Speed);

    public double StandardDeviation => _shots.StandardDeviation;

    public IEnumerable<Shot> Shots => _shots;
    public uint SerialNumber { get; set; }

    public string Notes { get; set; }
    public string ProjectileUnits { get; set; }
    public string VelocityUnits { get; set; }

    public void AddShot(Shot shot)
    {
        if (_shots.Contains(shot))
            // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
        {
            throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
        }

        _shots.Add(shot);
    }

    public void Mutate(Action<ShotSession> mutator) => mutator(this);

    public void Mutate(List<Action<ShotSession>> mutators)
    {
        foreach (Action<ShotSession> mutator in mutators)
        {
            mutator(this);
        }
    }

    public override string ToString() => $"{Id}: {_shots}";
}
