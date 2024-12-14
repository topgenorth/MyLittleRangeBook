
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.shotview
{
    public class WorkbookSession: ShotSession
    {
        public WorkbookSession() : base()
        {

        }

        public WorkbookSession(ShotSession session) : base()
        {
            this.Notes = session.Notes;
            this.FileName = session.FileName;
            this.ProjectileType = session.ProjectileType;
            this.ProjectileWeight = session.ProjectileWeight;
            this.SessionTimestamp = session.SessionTimestamp;

            foreach (var shot in session.Shots)
            {
                var newShot = new Shot(shot);
                this.AddShot(newShot);
            }
        }
        public int SheetNumber { get; set; } = 0;
        public string SheetName { get; set; }

        public void Mutate(Action<WorkbookSession> mutator)
        {
            mutator(this);
        }
        public void Mutate(List<Action<WorkbookSession>> mutators)
        {
            foreach (var mutator in mutators)
            {
                mutator(this);
            }
        }
    }

}
