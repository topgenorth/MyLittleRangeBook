using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     This delegate is used to create the name of a file based asset that will be copied over to the range event
    ///     directory.
    /// </summary>
    /// <param name="rangeAssetsDirectory">This is the name of directory that will hold range assets files.</param>
    /// <param name="rangeEventId">The <c cref="MlrbAssetFile" /> that will be processed. </param>
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
        public MlrbAssetFile(string fileToImport, MlrbAssetAggregate agg)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileToImport);
            if (!Path.Exists(fileToImport))
            {
                throw new FileNotFoundException("Cannot import asset file; does not exist.");
            }
            FileToImport = fileToImport;
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

        public override string ToString()
        {
            return FileToImport;
        }
    }
}
