using AElf.Client;
using AElf.Client.Dto;
using AElf.Cryptography.ECDSA;
using AElf.CSharp.Core;
using AElf.Standards.ACS0;
using AElf.Types;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts;

public static class Extensions
{
    public static T GetInstance<T>(this AElfClient client, Address contractAddress, ECKeyPair senderKey)
        where T : ContractStubBase, new()
    {
        return new T
        {
            __factory = new MethodStubFactory(client, senderKey, contractAddress)
        };
    }

    public static T GetInstance<T>(this Context ctx, string contractAddress)
        where T : ContractStubBase, new()
    {
        return ctx.GetInstance<T>(Address.FromBase58(contractAddress));
    }

    public static T GetInstance<T>(this Context ctx, Address contractAddress)
        where T : ContractStubBase, new()
    {
        Assert(Context.Client != null, "Context is not initialized.");
        var senderKey = ctx.SenderKey ?? Context.GlobalSenderKey ?? Context.DefaultKeyPair;
        return new T
        {
            __factory = new MethodStubFactory(Context.Client!, senderKey, contractAddress)
        };
    }

    public static async Task<Address?> DeployContractAsync(this ContextWithSystemContracts ctx, string filepath)
    {
        Assert(File.Exists(filepath), $"The file doesn't exist: {filepath}");
        var code = File.ReadAllBytes(filepath);
        var tx = (await ctx.Genesis.DeployUserSmartContract.SendAsync(new UserContractDeploymentInput()
        {
            Category = 0,
            Code = ByteString.CopyFrom(code),
            Salt = Hash.LoadFromHex("0000000000000000000000000000000000000000000000000000000000000005")
            // TODO: running sequence of salt
        })).MustSucceed();
        Context.Logger.LogTrace($"Code hash is {tx.Output.CodeHash}");
        return await ctx.WaitUntilContractIsReleased(tx.Output.CodeHash);
    }

    public static async Task<Address> WaitUntilContractIsReleased(this ContextWithSystemContracts ctx, Hash codeHash)
    {
        // TODO: Will this fail
        var res = await ctx.Genesis.GetSmartContractRegistrationByCodeHash.CallAsync(codeHash);
        return res.ContractAddress;
    }

    public static (TransactionResult, ByteString) Into(this TransactionResultDto dto)
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
                ? ByteString.FromBase64(dto.ReturnValue)
                : null,
            Error = dto.Error,
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
            Status = status
        }, ByteString.FromBase64(dto.ReturnValue));
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

    public static async Task<(TransactionResult, ByteString)> WaitForTransactionCompletionAsync(
        this AElfClient client,
        Hash transactionId)
    {
        const int maxRetries = 5;
        const int initialDelayMs = 1000;

        var result = await RetryWithExponentialBackoff(maxRetries, initialDelayMs, async () =>
        {
            var result =
                (await client.GetTransactionResultAsync(transactionId.ToHex())).Into();
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

    public static async Task<RefBlockInfo> GetRefBlockInfoAsync(this AElfClient client)
    {
        var chain = await client.GetChainStatusAsync();
        var height = chain.LastIrreversibleBlockHeight;
        var prefix = BlockHelper.GetRefBlockPrefix(Hash.LoadFromHex(chain.LastIrreversibleBlockHash));
        return new RefBlockInfo(height, prefix);
    }

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