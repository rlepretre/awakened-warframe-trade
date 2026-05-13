using System.Reflection;

namespace GameOcrOverlay.Services;

public static class ExceptionFormatter
{
    public static string GetInnermostMessage(Exception exception)
    {
        Exception current = exception;
        while (current is TargetInvocationException && current.InnerException is not null)
        {
            current = current.InnerException;
        }

        return current.Message;
    }
}
