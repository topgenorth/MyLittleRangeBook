
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.shotview
{


    public class WorkbookSession : ShotSession
    {

        public WorkbookSession(string sessionId)
        {
            Id = sessionId;
        }

        public WorkbookSession(ShotSession session)
        {
            Id = session.Id;
            Notes = session.Notes;
            FileName = session.FileName;
            ProjectileType = session.ProjectileType;
            ProjectileWeight = session.ProjectileWeight;
            SessionTimestamp = session.SessionTimestamp;


            foreach (Shot shot in session.Shots)
            {
                Shot newShot = new(shot);
                AddShot(newShot);
            }
        }

        public int SheetNumber { get; init; } = 0;
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
