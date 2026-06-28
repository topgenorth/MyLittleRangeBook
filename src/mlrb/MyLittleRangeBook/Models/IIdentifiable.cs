namespace MyLittleRangeBook.Models
{
    public interface IHaveMetaDataJson
    {
        string? MetaDataJson { get;  }
    }
    public interface IIdentifiable
    {
        MlrbId Id { get; }
    }
}