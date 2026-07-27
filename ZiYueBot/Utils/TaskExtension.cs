namespace ZiYueBot.Utils;

public static class TaskExtension
{
    public static void Forget(this Task task, Action<Exception>? exceptionHandler = null)
    {
        HandleFireAndForget(task, exceptionHandler);
    }

    public static void Forget(this Task task, Func<Exception, Task>? exceptionHandler = null)
    {
        HandleFireAndForget(task, exceptionHandler);
    }

    private static async void HandleFireAndForget(Task task, Action<Exception>? exceptionHandler = null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
        }
    }

    private static async void HandleFireAndForget(Task task, Func<Exception, Task>? exceptionHandler = null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            if (exceptionHandler is not null)
            {
                await exceptionHandler.Invoke(e);
            }
        }
    }
}