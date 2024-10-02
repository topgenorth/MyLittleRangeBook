using Dynastream.Fit;
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.FitFile
{
    public class SimpleFitReader
    {
        readonly ILogger _logger;

        public SimpleFitReader(ILogger logger)
        {
            _logger = logger.ForContext<SimpleFitReader>();
        }

        void ParseDeviceInfoMesg(DeviceInfoMesg? msg)
        {
            if (msg is null)
            {
                return;
            }

            if (!msg.Name.Equals("DeviceInfo", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var version = msg?.GetSoftwareVersion() ?? 0.0f;
            var serialNumber = msg?.GetSerialNumber() ?? 0;
            var manufactureranufacturer = msg?.GetManufacturer() ?? 0;
        }

        void ParseChronoShotSessionMessage(ShotSession shotSession, ChronoShotSessionMesg msg)
        {
            var dt = msg.GetTimestamp().GetDateTime();

            var f = msg.Fields.First(f => f.Name == "MinSpeed");
            var s = msg.GetMaxSpeed() ?? 0f;
            shotSession.SessionTimestamp = dt;
            shotSession.ProjectileWeight = msg.GetGrainWeight() ?? 0;
            shotSession.ProjectileType = msg.GetProjectileType().ToString() ?? "Unknown";
        }

        public async Task<int> Read(string filename, CancellationToken ct)
        {
            _logger.Information("Processing {FitFile}...", filename);
            var fit = await LoadFile(filename, ct);

            var fitListener = new FitListener();
            var decodeDemo = new Decode();
            decodeDemo.MesgEvent += fitListener.OnMesg;

            await using var fitSource = new MemoryStream(fit.ToArray());
            decodeDemo.Read(fitSource);
            var fitMessages = fitListener.FitMessages;

            var shotSession = new ShotSession();
            foreach (var msg in fitMessages.DeviceInfoMesgs)
            {
                ParseDeviceInfoMesg(msg);
            }

            foreach (var msg in fitMessages.ChronoShotSessionMesgs)
            {
                ParseChronoShotSessionMessage(shotSession, msg);
            }

            foreach (var msg in fitMessages.ChronoShotDataMesgs)
            {
                ParseChronoShotDataMesgs(shotSession, msg);
            }

            _logger.Information(shotSession.ToString());
            _logger.Information("Finished with {FitFile}.", filename);

            return 0;
        }

        void ParseChronoShotDataMesgs(ShotSession shotSession, ChronoShotDataMesg msg)
        {
            var shot = new Shot
            {
                ShotNumber = (int)msg.GetShotNum()!,
                Timestamp = msg.GetTimestamp().GetDateTime(),
                Speed = new ShotSpeed(msg.GetShotSpeed() ?? 0f)
            };

            shotSession.AddShot(shot);
        }

        async Task<ReadOnlyMemory<byte>> LoadFile(string filename, CancellationToken token)
        {
            await using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var result = new byte[fs.Length];
            var bytesRead = await fs.ReadAsync(result, 0, (int)fs.Length, token).ConfigureAwait(false);
            _logger.Verbose("Loaded {BytesRead} bytes from {Filename}", bytesRead, filename);

            return result;
        }
    }
}
