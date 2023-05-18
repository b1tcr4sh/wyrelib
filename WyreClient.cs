using Serilog;
using Ardalis.Result;
using BuildSoft.VRChat.Osc.Avatar;
using BuildSoft.VRChat.Osc;

using wyrelib.PhysicalLayer;

namespace wyrelib;

public class WyreClient {
    // TODO
    // - Listening read/write lock
    // - Establishing connections with other players
    // - Handling timeouts/disconnects
    // - Implement data channels as streams
    // - Encryption maybe?

    private OscConnection _connection;
    private ILogger _logger;

    public WyreClient(ILoggingProvider provider) {
        _logger = provider.GetLogger();
        OscAvatarUtility.AvatarChanged += OnAviChanged;
    }

    public async Task<Result> ConnectAsync() {
        Result<OscAvatarConfig> res = await GetValidAvatarAsync();
        if (res.Status == ResultStatus.Error) {
            // throw new OscConnectionException("Couldn't connect to VRChat");
            _logger.Warning("Failed to get the current avatar, Do \"Reset Avatar\" or start VRChat.");
            return Result.Error();
        } else if (res.Status == ResultStatus.NotFound) {
            _logger.Debug("Current avatar is missing params!");
            return Result.NotFound();
        }

        _connection = new OscConnection(res.Value, _logger);
        return Result.Success();
    }

    private async Task<Result<OscAvatarConfig>> GetValidAvatarAsync() {   
        _logger.Debug("Waiting for avatar...");
        OscAvatarConfig config = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();

        if (config is null) {
            return Result.Error("Failed to get avatar");
        }

        bool valid = true;
        valid = config.Parameters.ContainsKey("wyre/read");
        valid = config.Parameters.ContainsKey("wyre/write");
        valid = config.Parameters.ContainsKey("wyre/listening");
        valid = config.Parameters.ContainsKey("wyre/clock");

        if (!valid) {
            return Result.NotFound();
        }

        return Result.Success(config);
    }
    private void OnAviChanged(OscAvatar avi, ValueChangedEventArgs<OscAvatar> args) {
        _logger.Debug("Loading new avi..");
        ConnectAsync().GetAwaiter().GetResult();
    }
}