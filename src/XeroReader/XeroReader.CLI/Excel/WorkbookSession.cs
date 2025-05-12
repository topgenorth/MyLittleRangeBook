using NanoidDotNet;
using net.opgenorth.xero.Garmin.Model;

namespace net.opgenorth.xero.Excel
{
    public class WorkbookSession : ShotSession
    {
        public WorkbookSession() => SheetName = Nanoid.Generate();
        public WorkbookSession(string sessionId) : this() => Id = sessionId;

        public WorkbookSession(ShotSession session) : this()
        {
            Id = session.Id;
            Notes = session.Notes;
            FileName = session.FileName;
            ProjectileType = session.ProjectileType;
            ProjectileWeight = session.ProjectileWeight;
            DateTimeUtc = session.DateTimeUtc;

            foreach (Shot shot in session.Shots)
            {
                Shot newShot = new(shot);
                AddShot(newShot);
            }
        }

        public int SheetNumber { get; set; } = 0;
        public string SheetName { get; set; }

        public void Mutate(Action<WorkbookSession> mutator) => mutator(this);

        public void Mutate(List<Action<WorkbookSession>> mutators)
        {
            foreach (Action<WorkbookSession> mutator in mutators)
            {
                mutator(this);
            }
        }
    }
}
