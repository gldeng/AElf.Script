using AElf.Client;
using AElf.Client.Dto;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf;

namespace AElf.Scripts;

public static partial class Extension
{
    public static async Task<(TransactionResult, ByteString)> WaitForTransactionCompletionAsync(
        this AElfClient client,
        Hash transactionId)
    {
        const int maxRetries = 6;
        const int initialDelayMs = 1000;

        var result = await RetryWithExponentialBackoff(maxRetries, initialDelayMs, async () =>
        {
            var result =
                (await client.GetTransactionResultAsync(transactionId.ToHex())).IntoProtobuf();
            var status = result.Item1.Status;

            switch (status)
            {
                case TransactionResultStatus.Pending:
                case TransactionResultStatus.PendingValidation:
                case TransactionResultStatus.NotExisted:
                    return (false, result); // Continue retrying
                case TransactionResultStatus.Failed:
                case TransactionResultStatus.Conflict:
                case TransactionResultStatus.NodeValidationFailed:
                case TransactionResultStatus.Mined:
                    return (true, result); // Transaction is mined, exit the retry loop
                default:
                    throw new Exception($"Unexpected transaction status: {status}");
            }
        });

        // ReSharper disable once ComplexConditionExpression
        if (result.Item1 == null || result.Item1.Status == TransactionResultStatus.NotExisted)
        {
            throw new Exception($"Transaction does not exist after {maxRetries} retries");
        }

        return result;
    }

    public static (TransactionResult, ByteString) IntoProtobuf(this TransactionResultDto dto)
    {
        var status = dto.Status.ParseStatus();

        return (new TransactionResult()
        {
            BlockHash = dto.BlockHash != null
                ? Hash.LoadFromHex(dto.BlockHash)
                : null,
            BlockNumber = dto.BlockNumber,
            Bloom = dto.Bloom != null ? ByteString.FromBase64(dto.Bloom) : ByteString.Empty,
            ReturnValue = dto.ReturnValue != null
                ? ByteString.CopyFrom(ByteArrayHelper.HexStringToByteArray(dto.ReturnValue))
                : ByteString.Empty,
            Error = string.IsNullOrEmpty(dto.Error) ? "" : dto.Error,
            Logs =
            {
                dto.Logs.Select(x => new LogEvent()
                {
                    Address = Address.FromBase58(x.Address),
                    Indexed = { x.Indexed.Select(ByteString.FromBase64) },
                    Name = x.Name,
                    NonIndexed = ByteString.FromBase64(x.NonIndexed)
                })
            },
            Status = status,
            TransactionId = Hash.LoadFromHex(dto.TransactionId),
        }, ByteString.CopyFrom(ByteArrayHelper.HexStringToByteArray(dto.ReturnValue)));
    }


    public static TransactionResultStatus ParseStatus(this string value)
    {
        var mapping = new Dictionary<string, string>()
        {
            { "NOTEXISTED", "NOT_EXISTED" },
            { "PENDINGVALIDATION", "PENDING_VALIDATION" },
            { "NODEVALIDATIONFAILED", "NODE_VALIDATION_FAILED" }
        };

        // ReSharper disable once ComplexConditionExpression
        if (!Enum.TryParse<TransactionResultStatus>(value, true, out var status) &&
            mapping.TryGetValue(value, out var newValue) &&
            !Enum.TryParse<TransactionResultStatus>(newValue, true, out status))
        {
            throw new Exception($"Invalid transaction status: {value}");
        }

        return status;
    }

    public static IExecutionResult<T> MustSucceed<T>(this IExecutionResult<T> t) where T : IMessage<T>
    {
        Assert(t.TransactionResult.Error == "", $"Transaction failed: {t}");
        return t;
    }
}