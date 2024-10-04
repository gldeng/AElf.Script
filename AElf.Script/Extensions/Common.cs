namespace AElf.Scripts;

public static partial class Extension
{
    public static void Assert(bool predicate, string message = "")
    {
        if (predicate) return;
        if (string.IsNullOrEmpty(message))
        {
            throw new Exception("Assertion failed.");
        }

        throw new Exception($"Assertion failed: {message}");
    }

    private static async Task<TResult?> RetryWithExponentialBackoff<TResult>(int maxRetries, int initialDelayMs,
        Func<Task<(bool completed, TResult? result)>> operation)
    {
        bool completed = false;
        TResult? result = default(TResult);
        for (int retry = 0; retry < maxRetries; retry++)
        {
            (completed, result) = await operation();
            if (completed)
            {
                return result; // Operation succeeded, return the result
            }

            if (retry < maxRetries - 1)
            {
                int delay = initialDelayMs * (int)Math.Pow(2, retry);
                await Task.Delay(delay);
            }
        }

        return result; // Return the final result if it's not NotExisted
    }
}