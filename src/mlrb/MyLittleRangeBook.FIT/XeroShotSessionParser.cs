using CommunityToolkit.HighPerformance;
using Dynastream.Fit;
using FluentResults;
using MyLittleRangeBook.FIT.Model;

using DynastreamFitFile = Dynastream.Fit.File;

namespace MyLittleRangeBook.FIT
{
    /// <summary>
    ///     Will decode a byte stream from a Xero C1 FIT file into a ShotSession.
    /// </summary>
    public class XeroShotSessionParser : IXeroShotSessionParser
    {
        internal const int EXPECTED_FILE_TYPE = 54; // TODO [TO20260414] Change the name to a FITMessageType
        readonly FitListener _fitListener = new();
        // ReSharper disable CollectionNeverQueried.Local
        readonly HashSet<string> _recordDeveloperFieldNames = [];
        readonly HashSet<string> _recordFieldNames = [];
        // ReSharper restore CollectionNeverQueried.Local
#pragma warning disable CS0414 // Field is assigned but its value is never used
        FitMessages? _fitMessages = null;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        ILogger _logger;
        ShotSession? _shotSession;

        public XeroShotSessionParser(ILogger logger)
        {
            _logger = logger.ForContext<XeroShotSessionParser>();
        }

        /// <summary>
        ///     Decodes a FIT file into a ShotSession.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<ShotSession>> DecodeFITFileAsync(string filePath, CancellationToken cancellationToken)
        {
            Result<ShotSession> result;
            try
            {
                result = (await filePath.LoadFitFileBytesAsync(cancellationToken))
                    .Bind(bytesFromFitFile =>
                    {
                        // TODO [TO20260405] Refactor this to get rid of the dependency on CommunityToolkit.HighPerformance
                        using Stream stream = bytesFromFitFile.AsStream();
                        _logger.Verbose("Loaded {bytes} bytes.", bytesFromFitFile.Length);

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
            Decode decoder = new();
            // Check that this is a FIT file
            if (!decoder.IsFIT(input))
            {
                return Result.Fail(new UnsupportedFitFileTypeError(EXPECTED_FILE_TYPE));
            }

            MesgBroadcaster msgBroadcaster = new();

            msgBroadcaster.FileIdMesgEvent += OnFileIdMsg;

            msgBroadcaster.DeviceInfoMesgEvent += OnDeviceInfoMsg;
            decoder.MesgEvent += msgBroadcaster.OnMesg;

            msgBroadcaster.RecordMesgEvent += OnRecordMsg;
            msgBroadcaster.ChronoShotDataMesgEvent += OnChronoShotMsg;
            msgBroadcaster.ChronoShotSessionMesgEvent += OnChronoShotSessionMsg;
            decoder.MesgEvent += _fitListener.OnMesg;

            msgBroadcaster.ActivityMesgEvent += OnActivityMsg;
            msgBroadcaster.DeveloperDataIdMesgEvent += (_, e) =>
            {
                var msg = (DeveloperDataIdMesg)e.mesg;
                _logger.Verbose("OnDeveloperDataIdMsg: {developerDataIdMsg}", msg);
                foreach (Field? field in msg.Fields)
                {
                    _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
                }
            };

            msgBroadcaster.ConnectivityMesgEvent += (_, e) =>
            {
                var msg = (ConnectivityMesg)e.mesg;
                _logger.Verbose("OnConnectivityMsg: {connectivityMsg}", msg);
                foreach (Field? field in msg.Fields)
                {
                    _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
                }
            };


            try
            {
                if (decoder.Read(input))
                {
                    return _shotSession is null
                        ? Result.Fail(new FailedToParseFitFileError())
                        : Result.Ok(_shotSession);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to parse FIT bytes");

                return Result.Fail(new FailedToParseFitFileError().CausedBy(e));
            }

            return Result.Fail("Could not parse the file");
        }

        void OnActivityMsg(object sender, MesgEventArgs e)
        {
            var msg = (ActivityMesg)e.mesg;
            _logger.Verbose("OnActivityMsg: {activityMsg}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }
        }

        ILogger CreateFileLogger(string fitFile)
        {
            string logFilePath = Path.ChangeExtension(fitFile, ".txt");

            if (System.IO.File.Exists(logFilePath))
            {
                System.IO.File.Delete(logFilePath);
            }

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logFilePath)
                .CreateLogger();
        }


        /// <summary>
        ///     Will log the contents of the FIT file to text.
        /// </summary>
        /// <param name="fitFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<ShotSession>> ExploreFITFileAsync(string fitFile, CancellationToken cancellationToken)
        {
            ILogger oldLogger = _logger;
            _logger = CreateFileLogger(fitFile);
            Result<ShotSession> result;
            try
            {
                result = await DecodeFITFileAsync(fitFile, cancellationToken);
            }
            catch (Exception e)
            {
                oldLogger.Error(e, "Failed to parse FIT file at {fitFile}", fitFile);

                return Result.Fail(new FailedToParseFitFileError().CausedBy(e));
            }

            _logger = oldLogger;

            return result;
        }



        void OnChronoShotSessionMsg(object sender, MesgEventArgs e)
        {
            var msg = (ChronoShotSessionMesg)e.mesg;
            _logger.Verbose("OnChronoShotSessionMsg: {shotSessionMsg}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }

            _shotSession!.DateTimeUtc = msg.GetTimestampUtc();

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
            _logger.Verbose("OnChronoShotMsg: {chronoShotMsg}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }
            Shot shot = new()
            {
                ShotNumber = (int)msg.GetShotNum()!,
                DateTimeUtc = msg.GetTimestampUtc(),
                Speed = new ShotSpeed(msg.GetShotSpeed() ?? 0f)
            };
            _shotSession!.AddShot(shot);
        }

        void OnRecordMsg(object sender, MesgEventArgs e)
        {
            var msg = (RecordMesg)e.mesg;
            _logger.Verbose("OnRecordMsg: {onRecordMsg}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }

            foreach (Field? field in e.mesg.Fields)
            {
                if (field.Name.ToLower() != "unknown")
                {
                    _recordFieldNames.Add(field.Name);
                }
            }

            foreach (DeveloperField? devField in e.mesg.DeveloperFields)
            {
                _recordDeveloperFieldNames.Add(devField.Name);
            }
        }

        void OnFileIdMsg(object sender, MesgEventArgs e)
        {
            var msg = (FileIdMesg)e.mesg;
            _logger.Verbose("OnFileIdMsg: {fileIdMsg}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }

            var fileIdMsg = (FileIdMesg)e.mesg;
            DynastreamFitFile? fitType = fileIdMsg.GetType();

            if (EXPECTED_FILE_TYPE != (int)fitType!)
            {
                throw new Exception($"Expected FIT File type {EXPECTED_FILE_TYPE}, received {fitType}.");
            }

            uint? sn = fileIdMsg.GetSerialNumber();

            _shotSession ??= new ShotSession(sn);
            _shotSession.SerialNumber = sn!.Value;
            _shotSession.TimeCreated = fileIdMsg.GetTimeCreatedUtc();
            _shotSession.Manufacturer = fileIdMsg.GetManufacturer();
            _shotSession.Product = fileIdMsg.GetProduct();
            _shotSession.Type = (byte)fitType;

        }

        void OnDeviceInfoMsg(object sender, MesgEventArgs e)
        {
            var msg = (DeviceInfoMesg)e.mesg;
            _logger.Verbose("OnDeviceInfoMsg: {deviceInfo}", msg);
            foreach (Field? field in msg.Fields)
            {
                _logger.Verbose("  Field: {field}, {fieldType}", field.Name, field.Type);
            }

            if (_shotSession == null)
            {
                uint? sn = msg.GetSerialNumber();
                _shotSession = new ShotSession(sn)
                {
                    TimeCreated = msg.GetTimestampUtc(),
                    Manufacturer = msg.GetManufacturer(),
                    Product = msg.GetProduct()
                };
            }

            _shotSession.SoftwareVersion = msg.GetSoftwareVersion().ToString() ?? "Unknown";

        }
    }
}
