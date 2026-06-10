using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmsService
    {
                /// <summary>
        ///     The SQL and commands we can perform on the database
        /// </summary>
        public static class Commands
        {
            const string SelectSql = "SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes, is_active AS IsActive, rounds_fired AS RoundsFired, created AS Created, modified AS Modified FROM firearms ORDER BY name;";
            const string SelectByIdSql = "SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes, is_active AS IsActive, rounds_fired AS RoundsFired, created AS Created, modified AS Modified FROM firearms WHERE id=@Id;";
            const string SelectActiveSql = "SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes, is_active AS IsActive, rounds_fired AS RoundsFired, created AS Created, modified AS Modified FROM firearms WHERE is_active=1 ORDER BY name;";
            const string DeleteSql = "DELETE FROM firearms WHERE id = @Id";

            const string UpsertSql = """
                                     INSERT INTO firearms (id, name, notes, modified, created, rounds_fired) 
                                     VALUES (@Id, @Name, @Notes, utcnow(), utcnow(), @RoundsFired) 
                                     ON CONFLICT(name) DO UPDATE SET notes = @Notes, modified = utcnow(), rounds_fired=@RoundsFired
                                     RETURNING row_id
                                     """;

            const string AssociateFirearmWithRangeEventSql = """
                                                             INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id) 
                                                             VALUES (@FirearmsId, @SimpleRangeEventId);
                                                             """;


            const string RangeEventsByFirearmNameSql = """
                                                        SELECT 
                                                            s.firearm_name AS FirearmName,
                                                            s.id AS SimpleRangeEventId
                                                        FROM simple_range_events AS s
                                                        LEFT JOIN firearms AS f ON f.name = s.firearm_name
                                                        WHERE f.name IS NULL
                                                        ORDER BY s.firearm_name;                                                       
                                                       """;



            internal static readonly DapperCommand AssociateFirearmWithRangeEvent =
                new(AssociateFirearmWithRangeEventSql);

            internal static readonly DapperCommand RangeEventsByFirearmName = new(RangeEventsByFirearmNameSql);
            internal static DapperCommand SelectAll => new(SelectSql);
            internal static DapperCommand SelectActive => new(SelectActiveSql);
            internal static DapperCommand SelectById => new(SelectByIdSql);
            internal static DapperCommand DeleteById => new(DeleteSql);
            internal static DapperCommand Upsert => new(UpsertSql);

            internal record struct RangeEventForFirearm(string FirearmName, string SimpleRangeId);

        }
    }
}
