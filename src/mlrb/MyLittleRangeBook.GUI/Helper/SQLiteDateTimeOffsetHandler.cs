using System.Data;
using Dapper;

namespace MyLittleRangeBook.GUI.Helper
{
    public class SQLiteDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is string s)
            {
                return DateTimeOffset.Parse(s);
            }

            if (value is DateTime dt)
            {
                return new DateTimeOffset(dt);
            }

            return (DateTimeOffset)value;
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString("O");
            // Round-trip ISO format
        }
    }
}
