using CommunityToolkit.HighPerformance;
using Dynastream.Fit;
using FluentResults;
using MySimpleRangeLog.CLI.Model;
using File = System.IO.File;

namespace MySimpleRangeLog.CLI
{
    /// <summary>
    ///     Will decode a byte stream from a Xero C1 FIT file into a ShotSession.
    /// </summary>
    public class XeroShotSessionParser : IXeroShotSessionParser
    {
        internal const int ExpectedFileType = 54;
        readonly FitListener _fitListener = new();

        readonly ILogger _logger;
        readonly HashSet<string> _recordDeveloperFieldNames = [];
        readonly HashSet<string> _recordFieldNames = [];
#pragma warning disable CS0414 // Field is assigned but its value is never used
        FitMessages? _fitMessages = null;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        ShotSession? _shotSession;

        public XeroShotSessionParser(ILogger logger)
        {
            _logger = logger.ForContext<XeroShotSessionParser>();
        }

        /// <summary>
        ///     Decodes a FIT file into a ShotSession.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Result<ShotSession>> DecodeShotSessionAsync(string filePath, CancellationToken ct)
        {
            if (!File.Exists(filePath))
            {
                return Result.Fail(new FitFileNotFoundError(filePath));
            }

            Result<ShotSession> result;
            try
            {
                result = (await filePath.LoadBytesAsync(ct))
                    .Bind(bytesFromFitFile =>
                    {
                        _logger.Verbose("Loaded {bytes} bytes.", bytesFromFitFile.Length);
                        using var stream = bytesFromFitFile.AsStream();

                        return Decode(stream);
                    });
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to parse FIT file at {filePath}", filePath);
                result = Result.Fail(new FailedToParseFitFileError().CausedBy(e));
            }

            return result;
        }

        public Result<ShotSession> Decode(Stream input)
        {
            _shotSession = new ShotSession();

            Decode decoder = new();
            // Check that this is a FIT file
            if (!decoder.IsFIT(input))
            {
                return Result.Fail(new UnexpectedFitFileTypeError(ExpectedFileType));
            }

            MesgBroadcaster msgBroadcaster = new();
            decoder.MesgEvent += msgBroadcaster.OnMesg;
            msgBroadcaster.DeviceInfoMesgEvent += OnDeviceInfoMsg;
            msgBroadcaster.FileIdMesgEvent += OnFileIdMsg;
            msgBroadcaster.RecordMesgEvent += OnRecordMsg;
            msgBroadcaster.ChronoShotDataMesgEvent += OnChronoShotMsg;
            msgBroadcaster.ChronoShotSessionMesgEvent += OnChronoShotSessionMsg;
            decoder.MesgEvent += _fitListener.OnMesg;

            try
            {
                if (decoder.Read(input))
                {
                    return _shotSession;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to parse FIT bytes");

                return Result.Fail(new FailedToParseFitFileError().CausedBy(e));
            }

            return Result.Fail("Could not parse the file");
        }

        void OnDeviceInfoMsg(object sender, MesgEventArgs e)
        {
            var msg = (DeviceInfoMesg)e.mesg;
            var serialNumber = msg?.GetSerialNumber() ?? 0;

            _shotSession!.SerialNumber = serialNumber;
        }

        void OnChronoShotSessionMsg(object sender, MesgEventArgs e)
        {
            var msg = (ChronoShotSessionMesg)e.mesg;

            var dt = msg.GetTimestamp().GetDateTime();
            _shotSession!.DateTimeUtc = dt;

            _shotSession!.ProjectileWeight = Convert.ToInt32(msg.GetGrainWeight() ?? 0f);
            _shotSession!.ProjectileType = msg.GetProjectileType().ToString() ?? "Unknown";
            _shotSession.AverageSpeed = Convert.ToInt32(msg.GetAvgSpeed() ?? 0f);
            _shotSession.MaxSpeed = Convert.ToInt32(msg.GetMaxSpeed() ?? 0f);
            _shotSession.MinSpeed = Convert.ToInt32(msg.GetMinSpeed() ?? 0f);
            _shotSession.StandardDeviation = msg.GetStandardDeviation() ?? 0f;
            _shotSession.ExtremeSpread = _shotSession.MaxSpeed - _shotSession.MinSpeed;
        }

        void OnChronoShotMsg(object sender, MesgEventArgs e)
        {
            var msg = (ChronoShotDataMesg)e.mesg;

            Shot shot = new()
            {
                ShotNumber = (int)msg.GetShotNum()!,
                DateTimeUtc = msg.GetTimestamp().GetDateTime(),
                Speed = new ShotSpeed(msg.GetShotSpeed() ?? 0f)
            };
            _shotSession!.AddShot(shot);
        }

        void OnRecordMsg(object sender, MesgEventArgs e)
        {
            foreach (var field in e.mesg.Fields)
            {
                if (field.Name.ToLower() != "unknown")
                {
                    _recordFieldNames.Add(field.Name);
                }
            }

            foreach (var devField in e.mesg.DeveloperFields)
            {
                _recordDeveloperFieldNames.Add(devField.Name);
            }
        }

        void OnFileIdMsg(object sender, MesgEventArgs e)
        {
            var f = (FileIdMesg)e.mesg;
            var t = f.GetType();

            if (ExpectedFileType != (int)t!)
            {
                throw new Exception($"Expected FIT File type {ExpectedFileType}, received {t}.");
            }
        }
    }
}
