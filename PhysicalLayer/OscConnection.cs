using System.Collections;
using BuildSoft.VRChat.Osc.Avatar;
using Ardalis.Result;
using Serilog;

namespace wyrelib.PhysicalLayer;

public class AviConnection {
    private OscAvatarConfig _config;
    private ILogger _logger;

    internal AviConnection(OscAvatarConfig config, ILogger logger) {
        _config = config;
        _logger = logger;
    }

    internal Result WriteAsync(Byte data) {
        BitArray buffer = new BitArray(data);

        try {
            foreach (bool bit in buffer) {
                WriteBit(bit);
                Result<bool> result = ReadBit();

                if (!result.IsSuccess) {
                    return Result.NotFound();
                }

                if (bit != result.Value) {
                    return Result.Error();
                }
            }
        } catch (Exception e) {
            return Result.Error(e.Message);
        }

        return Result.Success();
    }
    internal Result<byte> ReadAsync() {
        BitArray buffer = new BitArray(8);

        for (int i = 0; i < 8; i++) {
            Result<bool> result = ReadBit();

            if (!result.IsSuccess) {
                return Result<byte>.Error();
            }

            buffer[i] = result.Value;
            WriteBit(buffer[i]);
        }

        return ConvertToByte(buffer);
    }
    private void WriteBit(bool bit) {
        _config.Parameters["wyre/write"] = bit;
    }
    private Result<bool> ReadBit(int counter = 0) {
        bool one = (bool) _config.Parameters["wyre/read1"];
        bool zero = (bool) _config.Parameters["wyre/read0"];

        if (one == false && zero == false) {
            if (counter > 2) {
                return Result<bool>.NotFound();
            }

            ReadBit(1);
        }
        if (one == true && zero == false) {
            return Result<bool>.Error();
        }

        if (zero == true) {
            return false;
        } else if (one == true) {
            return true;
        } else {
            return Result<bool>.Error();
        }
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
