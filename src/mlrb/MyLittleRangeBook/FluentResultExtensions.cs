using MyLittleRangeBook.Models;

namespace MyLittleRangeBook
{
    public static class FluentResultExtensions
    {
        public const string ID_KEY    = "Id";
        public const string ROWID_KEY = "RowId";

        public static Error ToError(this Exception ex, string? message = null)
        {
            Error err = string.IsNullOrWhiteSpace(message)
                            ? new Error($"Unexpected exception: {ex.Message}")
                            : new Error($"{message}. {ex.Message}");

            return err.CausedBy(ex);
        }

        public static Result FailWithException(this Exception ex)
        {
            Error? err = new Error($"Unexpected exception : {ex.Message}")
               .CausedBy(ex);

            return Result.Fail(err);
        }

        public static Error Enrich(this Error error, MlrbId id) => error.Enrich(id.ToString());

        public static Error Enrich(this Error error, EntityId eid) => error.Enrich(eid.Id, eid.RowId);

        public static Error Enrich(this Error error, string id) => error.WithMetadata(ID_KEY, id);

        public static Error Enrich(this Error error, long? rowId)
        {
            error.WithMetadata(ROWID_KEY, rowId);

            return error;
        }

        public static Error Enrich(this Error error, string id, long? rowId) => error.Enrich(id).Enrich(rowId);

        public static Success Enrich(this Success success, MlrbId id)
        {
            success.Metadata.Add(ID_KEY, id.ToString());

            return success;
        }

        public static Success Enrich(this Success success, EntityId eid)
        {
            success.Metadata.Add(ID_KEY,    eid.Id);
            success.Metadata.Add(ROWID_KEY, eid.RowId);

            return success;
        }

        public static Success Enrich(this Success success, string id, long? rowId)
        {
            success.Metadata.Add(ID_KEY,    id);
            success.Metadata.Add(ROWID_KEY, rowId);

            return success;
        }
    }
}