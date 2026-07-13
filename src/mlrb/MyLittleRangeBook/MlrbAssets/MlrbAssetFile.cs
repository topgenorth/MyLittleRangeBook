using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     This delegate is used to create the name of a file-based asset that will be copied over to the range event
    ///     directory.
    /// </summary>
    /// <param name="rangeAssetsDirectory">This is the name of the directory that will hold range assets files.</param>
    public delegate string AssetFileNameResolver(string rangeAssetsDirectory,
                                                 MlrbAssetFile mlrbAssetFile);

    /// <summary>
    ///     Represents an asset file specifically associated with a range event in the context of the application.
    ///     Provides functionality to define, identify, and process assets for range events, including copying the
    ///     asset file to the asset directory for a RangeEvent.
    /// </summary>
    public record MlrbAssetFile
    {
        // TODO [TO20260602] Maybe this should all move into the MlrbAssetAggregate?
        public MlrbAssetFile(MlrbAssetAggregate agg)
        {
            FileToImport = agg.SourceFile;
            Aggregate = agg;
            Id = Aggregate.Id;
        }

        public MlrbAssetAggregate Aggregate { get; }

        /// <summary>
        ///     A unique ID that will identify the range asset file; it's also the aggregate Id for the MlrbAsset.
        /// </summary>
        public MlrbId Id { get; private set; }

        /// <summary>
        ///     Path to the asset that is to be copied over to the range event asset directory.
        /// </summary>
        public string FileToImport { get; }

        public string DestinationFile => Aggregate.DestinationPath;

        public string MimeType => Aggregate.MimeType;

        public byte[] FileContents => Aggregate.FileContents;
        public DateTimeOffset Created => Aggregate.Created;

        public DateTimeOffset Modified => Aggregate.Modified;

        /// <summary>
        /// The name of the firearm to associate with the asset. Optional.
        /// </summary>
        public string? AssociatedFirearmName { get; set; } = null;

        public string? SHA256 => Aggregate.SHA256;

        public override string ToString()
        {
            return FileToImport;
        }
    }
}
