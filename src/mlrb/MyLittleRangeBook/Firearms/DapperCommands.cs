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
            const string SELECT_SQL = """
                                      SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes, is_active AS IsActive,
                                             rounds_fired AS RoundsFired, created AS Created, modified AS Modified
                                      FROM firearms
                                      ORDER BY name;
                                      """;

            const string SELECT_BY_ID_SQL = """
                                            SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes,
                                                   is_active AS IsActive, rounds_fired AS RoundsFired, created AS Created, modified AS Modified
                                            FROM firearms WHERE id=@Id;
                                            """;

            const string SELECT_ACTIVE_SQL = """
                                             SELECT row_id AS RowId, id AS Id, name AS Name, notes AS Notes,
                                             is_active AS IsActive, rounds_fired AS RoundsFired, created AS Created, modified AS Modified
                                             FROM firearms WHERE is_active=1 ORDER BY name;
                                             """;

            const string DELETE_SQL = "DELETE FROM firearms WHERE id = @Id";

            const string UPSERT_SQL = """
                                      INSERT INTO firearms (id, name, notes, modified, created, rounds_fired)
                                      VALUES (@Id, @Name, @Notes, @Modified, @Created, @RoundsFired)
                                      ON CONFLICT(name) DO UPDATE
                                          SET notes = excluded.notes,
                                              modified = excluded.modified,
                                              rounds_fired=excluded.rounds_fired
                                      RETURNING row_id
                                      """;

            internal static readonly DapperCommand s_selectAll    = new(SELECT_SQL);
            internal static readonly DapperCommand s_selectActive = new(SELECT_ACTIVE_SQL);
            internal static readonly DapperCommand s_selectById   = new(SELECT_BY_ID_SQL);
            internal static readonly DapperCommand s_deleteById   = new(DELETE_SQL);
            internal static readonly DapperCommand s_upsert       = new(UPSERT_SQL);

            const string ASSOCIATE_FIREARM_WITH_ASSET_SQL = """
                                                            INSERT INTO asset_files_firearms (firearm_id, asset_id)
                                                            VALUES (@FirearmId, @AssetId)
                                                            ON CONFLICT DO NOTHING
                                                            RETURNING row_id;
                                                            """;

            const string ASSOCIATE_FIREARM_WITH_SIMPLE_RANGE_EVENTS_SQL = """
                                                                          INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                                          VALUES (@FirearmId, @SimpleRangeEventId)
                                                                          ON CONFLICT DO NOTHING
                                                                          RETURNING row_id;
                                                                          """;

            const string DISASSOCIATE_FIREARM_FROM_ASSET_SQL = """
                                                               DELETE FROM asset_files_firearms
                                                               WHERE firearm_id = @FirearmId AND asset_id = @AssetId
                                                               """;

            const string DISASSOCIATE_FIREARM_FROM_RANGE_EVENT_SQL = """
                                                                     DELETE FROM firearms_simple_range_events
                                                                     WHERE firearm_id = @FirearmId AND simple_range_event_id = @SimpleRangeEventId
                                                                     """;

            const string ASSOCIATE_FIREARM_WITH_NOTE_SQL = """
                                                           INSERT INTO firearms_notes (firearm_id, note_id)
                                                           VALUES (@FirearmId, @NoteId)
                                                           ON CONFLICT DO NOTHING
                                                           RETURNING row_id;
                                                           """;

            internal static readonly DapperCommand s_addAssociationToAsset =
                new(ASSOCIATE_FIREARM_WITH_ASSET_SQL);

            internal static readonly DapperCommand s_addAssociationToRangeEvent =
                new(ASSOCIATE_FIREARM_WITH_SIMPLE_RANGE_EVENTS_SQL);

            internal static readonly DapperCommand s_removeAssociationFromAsset =
                new(DISASSOCIATE_FIREARM_FROM_ASSET_SQL);

            internal static readonly DapperCommand s_removeAssociationFromRangeEvent =
                new(DISASSOCIATE_FIREARM_FROM_RANGE_EVENT_SQL);

            internal static readonly DapperCommand s_associateNoteWithFirearm =
                new(ASSOCIATE_FIREARM_WITH_NOTE_SQL);
        }
    }
}