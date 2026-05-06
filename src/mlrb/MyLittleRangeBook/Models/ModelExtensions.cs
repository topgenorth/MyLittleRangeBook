using FluentResults;

namespace MyLittleRangeBook.Models
{
    public static class ModelExtensions
    {
        public static Error Enrich(this Error error, EntityId eid)
        {
            error.Metadata.Add("Id", eid.Id);
            error.Metadata.Add("RowId", eid.RowId);

            return error;
        }

        public static Error Enrich(this Error error, string id, long? rowId)
        {
            error.Metadata.Add("Id", id);
            error.Metadata.Add("RowId", rowId);

            return error;
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
