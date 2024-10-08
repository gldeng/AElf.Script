using AElf.CSharp.Core;
using Google.Protobuf;

namespace AElf.Scripts;

public static partial class Extension
{
    public static List<T> GetLogEvents<T>(this IExecutionTask executionTask) where T : IEvent<T>, new()
    {
        return executionTask.TransactionResult.Logs.Where(l => l.Name == typeof(T).Name)
            .Select(e =>
            {
                var logEvent = new T();
                logEvent.MergeFrom(e.NonIndexed);
                return logEvent;
            }).ToList();
    }
}