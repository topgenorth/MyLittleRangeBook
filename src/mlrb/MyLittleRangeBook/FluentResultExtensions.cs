using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook
{
    public static class FluentResultExtensions
    {
        public static Error Enrich(this Error error, EntityId eid)
        {
            return error.Enrich(eid.Id, eid.RowId);
        }

        public static Error Enrich(this Error error, string id)
        {
            return error.WithMetadata("Id", id);
        }

        public static Error Enrich(this Error error, long? rowId)
        {
            error.WithMetadata("RowId", rowId);

            return error;
        }

        public static Error Enrich(this Error error, string id, long? rowId)
        {
            return error.Enrich(id).Enrich(rowId);
        }

        public static Success Enrich(this Success success, EntityId eid)
        {
            success.Metadata.Add("Id", eid.Id);
            success.Metadata.Add("RowId", eid.RowId);

            return success;
        }

        public static Success Enrich(this Success success, string id, long? rowId)
        {
            success.Metadata.Add("Id", id);
            success.Metadata.Add("RowId", rowId);

            return success;
        }
    }
}
