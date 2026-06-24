using System.Data;
using Dapper;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    /// <summary>
    /// A custom type handler for the MlrbId type, enabling seamless conversion between the MlrbId C# struct
    /// and its string representation in SQLite. This handler manages the serialization of MlrbId values into
    /// string format for storage in the database and deserialization back into MlrbId instances when retrieving
    /// values from the database.
    /// </summary>
    public class SqliteMlrbIdHandler : SqlMapper.TypeHandler<MlrbId>
    {
        public override void SetValue(IDbDataParameter parameter, MlrbId value)
        {
            parameter.Value = value.ToString();
        }

        public override MlrbId Parse(object value)
        {
            string? str = value.ToString();
            return string.IsNullOrWhiteSpace(str) ? throw new InvalidCastException($"Cannot convert value to a MlrbId: {value}") : new MlrbId(str!);
        }
    }
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
