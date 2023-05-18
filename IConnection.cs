using Ardalis.Result;

namespace wyrelib;

public interface IConnection {
    public Task<Result> WriteAsync(byte data);
    public Task<Result<byte>> ReadAsync();
}