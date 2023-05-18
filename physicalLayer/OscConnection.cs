using System.Collections;
using BuildSoft.VRChat.Osc.Avatar;
using Ardalis.Result;
using Serilog;

namespace wyrelib.physicalLayer;

public class OscConnection : IConnection {
    private OscAvatarConfig _config;
    private Clock _clock;
    private ILogger _logger;

    public OscConnection(ILoggingProvider provider) {
        _logger = provider.GetLogger();
    }

    public async Task<Result> ConnectToVrchat() {
        Result res = await GetValidAvatar();
        if (res.Status == ResultStatus.Error) {
            // throw new OscConnectionException("Couldn't connect to VRChat");
            _logger.Warning("Failed to get the current avatar, Do \"Reset Avatar\" or start VRChat.");
            return Result.Error();
        } else if (res.Status == ResultStatus.NotFound) {
            _logger.Debug("Current avatar is missing params!");
            return Result.NotFound();
        }

        _clock = new Clock(_config);

        return Result.Success();
    }
    public async Task<Result> WriteAsync(byte data) {
        BitArray buffer = new BitArray(data);

        try {
            foreach (bool bit in buffer) {
                _config.Parameters["wyre/write"] = bit;
                Result res = await _clock.PulseAsync(100);

                if (!res.IsSuccess) {
                    return Result.Error(string.Join(" | ", res.Errors));
                }
            }
        } catch (Exception e) {
            return Result.Error(e.Message);
        }

        return Result.Success();
    } 
    public async Task<Result<byte>> ReadAsync() {
        BitArray buffer = new BitArray(8);

        for (int i = 0; i < 8; i++) {
            await _clock.WaitForPulseAsync();
            buffer[i] = (bool) _config.Parameters["wyre/read"];
        }

        return ConvertToByte(buffer);
    }


    private async Task<Result> GetValidAvatar() {
        _logger.Debug("Waiting for avatar...");
        _config = await OscAvatarConfig.WaitAndCreateAtCurrentAsync();

        if (_config is null) {
            return Result.Error("Failed to get avatar");
        }

        bool valid = true;
        valid = _config.Parameters.ContainsKey("wyre/read");
        valid = _config.Parameters.ContainsKey("wyre/write");
        valid = _config.Parameters.ContainsKey("wyre/listening");
        valid = _config.Parameters.ContainsKey("wyre/clock");

        if (!valid) {
            return Result.NotFound();
        }

        return Result.Success();
    }
    private byte ConvertToByte(BitArray bits) {
        if (bits.Count != 8)
        {
            throw new ArgumentException("Incorrect amount of bits"); // ????
        }
        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }
}

[System.Serializable]
public class OscConnectionException : System.Exception
{
    public OscConnectionException() { }
    public OscConnectionException(string message) : base(message) { }
    public OscConnectionException(string message, System.Exception inner) : base(message, inner) { }
    protected OscConnectionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
