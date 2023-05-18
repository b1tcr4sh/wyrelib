using Serilog;

namespace wyrelib;

public class LoggingProvider : ILoggingProvider {
    private ILogger _logger;
    public LoggingProvider(ILogger logger) {
        _logger = logger;
    }
    public LoggingProvider() {
        _logger = CreateLogger();
    }

    public ILogger GetLogger() {
        if (_logger is null) {
            _logger = CreateLogger();
        }

        return _logger;
    }
    private ILogger CreateLogger() {
        ILogger log = new LoggerConfiguration() 
        .WriteTo.Console()
        .CreateLogger();

        return log;
    }
}

public interface ILoggingProvider {
    public ILogger GetLogger();
}