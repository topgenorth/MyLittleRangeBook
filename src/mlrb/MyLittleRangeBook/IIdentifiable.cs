using MyLittleRangeBook.Models;

namespace MyLittleRangeBook
{
    public interface IIdentifiable
    {
        MlrbId Id { get; }
    }
}