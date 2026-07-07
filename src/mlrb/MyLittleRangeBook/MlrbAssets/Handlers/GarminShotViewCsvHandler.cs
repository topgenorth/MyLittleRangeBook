using MyLittleRangeBook.FIT;

namespace MyLittleRangeBook.MlrbAssets.Handlers
{
    public class GarminShotViewCsvHandler : IPipelineHandler<MlrbAssetFile>
    {
        readonly ILogger                   _logger;
        readonly IXeroCsvShotSessionParser _parser;

        public GarminShotViewCsvHandler(ILogger logger, IXeroCsvShotSessionParser parser)
        {
            _logger = logger;
            _parser = parser;
        }

        public string Name => "Parse Garmin Shot View CSV";

        public async Task<Result> ExecuteAsync(PipelineContext<MlrbAssetFile>                     context,
                                               Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            List<IReason> reasons  = [];
            string        filePath = context.Record.FileToImport;

            if (await _parser.IsShotViewCsvAsync(filePath, context.CancellationToken))
            {
                reasons.Add(new Success($"File is a valid Garmin Shot View CSV: '{filePath}'"));
                var x = await _parser.ParseCsvFileAsync(filePath, context.CancellationToken).ConfigureAwait(false);
                reasons.AddRange(x.Reasons);
                context.Metadata["ShotviewCSV"] = x.Value;
            }
            else
            {
                reasons.Add(new Success($"File is not a valid Garmin Shot View CSV: '{filePath}'"));
            }

            return new Result().WithReasons(reasons);
        }
    }
}