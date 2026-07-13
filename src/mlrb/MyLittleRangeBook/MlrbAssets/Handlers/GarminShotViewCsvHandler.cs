using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.FIT.Model;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.MlrbAssets.Handlers
{
    public class GarminShotViewCsvHandler : IPipelineHandler<MlrbAssetFile>
    {
        readonly IFirearmsService          _firearmService;
        readonly ILogger                   _logger;
        readonly IXeroCsvShotSessionParser _parser;
        readonly ISqliteHelper             _sqliteHelper;

        public GarminShotViewCsvHandler(ILogger                   logger,
                                        IXeroCsvShotSessionParser parser,
                                        IFirearmsService          firearmService,
                                        ISqliteHelper             sqliteHelper)
        {
            _logger         = logger;
            _parser         = parser;
            _firearmService = firearmService;
            _sqliteHelper   = sqliteHelper;
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
                Result<ShotSession> x = await _parser.ParseCsvFileAsync(filePath, context.CancellationToken)
                                                     .ConfigureAwait(false);
                reasons.AddRange(x.Reasons);
                context.Metadata["ShotviewCSV"] = x.Value;

                if (!string.IsNullOrWhiteSpace(context.Record.AssociatedFirearmName))
                {
                    MlrbId assetId   = context.Record.Aggregate.Id;
                    MlrbId firearmId = MlrbId.FromString(context.Record.AssociatedFirearmName!);
                    context.Metadata["AssociatedFirearm"] = context.Record.AssociatedFirearmName!;

                    await using ScopedSqliteConnection db = await _sqliteHelper
                                                     .GetScopedDatabaseConnectionAsync(context.CancellationToken)
                                                     .ConfigureAwait(false);
                    DapperCommandContext dapperCtx = new(db, context.CancellationToken);

                    Result r = await _firearmService.AssociateWithAsset(dapperCtx, firearmId, assetId)
                                                    .ConfigureAwait(false);
                    reasons.AddRange(r.Reasons);
                }
            }
            else
            {
                reasons.Add(new Success($"File is not a valid Garmin Shot View CSV: '{filePath}'"));
            }

            return new Result().WithReasons(reasons);
        }
    }
}