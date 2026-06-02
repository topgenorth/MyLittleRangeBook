namespace MyLittleRangeBook.MlrbAssets
{
    public record struct MlrbAssetRow(
        string Id,
        string OriginalFileName,
        string PathToAssetFile,
        string MimeType,
        byte[] FileContentBytes,
        DateTimeOffset Created,
        DateTimeOffset Modified,
        long? RowId = null);
}
