using System.Collections;
using BuildSoft.VRChat.Osc.Avatar;
using Ardalis.Result;
using Serilog;

namespace wyrelib.PhysicalLayer;

public class OscConnection {
    private OscAvatarConfig _config;
    private Clock _clock;
    private ILogger _logger;

    internal OscConnection(OscAvatarConfig config, ILogger logger) {
        _config = config;
        _logger = logger;
        _clock = new Clock(config);
    }

    internal async Task<Result> WriteAsync(Byte data) {
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
    internal async Task<Result<byte>> ReadAsync() {
        BitArray buffer = new BitArray(8);

        for (int i = 0; i < 8; i++) {
            await _clock.WaitForPulseAsync();
            buffer[i] = (bool) _config.Parameters["wyre/read"];
        }

        return ConvertToByte(buffer);
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
