using StudentPortal.API.Util.Logging;

namespace StudentPortal.API.Domain.Contracts;

public interface ILoggerFactory
{
    IExtendedLogger CreateLogger(string? userId, string? ip, string context);
}
