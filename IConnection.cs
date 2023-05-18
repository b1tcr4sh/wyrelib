using Ardalis.Result;

namespace wyrelib;

public interface IConnection {
    Task<Result> WriteAsync(byte data);
    Task<Result<byte>> ReadAsync();
}