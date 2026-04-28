using System.Data;
using Dapper;

namespace MyLittleRangeBook.Database.Sqlite
{
/// <summary>
    ///     All dates are stored as a DateTimeOffset in SQLite, formatted to ISO 8601.  We need a special handler to convert
    ///     these back and forth, since SQLite doesn't have a native DateTimeOffset type.
    /// </summary>
    public class SqliteDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            return value switch
            {
                string s => DateTimeOffset.Parse(s),
                DateTime dt => new DateTimeOffset(dt),
                _ => (DateTimeOffset)value
            };
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString("O");
            // Round-trip ISO format
        }
    }
}
