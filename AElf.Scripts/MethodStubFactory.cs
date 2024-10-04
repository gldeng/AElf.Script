using AElf.Client;
using AElf.Client.Dto;
using AElf.Cryptography;
using AElf.Cryptography.ECDSA;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Nito.AsyncEx;
using Enum = System.Enum;

namespace AElf.Scripts;

public class MethodStubFactory : IMethodStubFactory
{
    private readonly AElfClient _client;
    private readonly ECKeyPair _senderKey;
    private readonly Address _contractAddress;

    public MethodStubFactory(AElfClient client, ECKeyPair senderKey, Address contractAddress)
    {
        _client = client;
        _senderKey = senderKey;
        _contractAddress = contractAddress;
    }


    // public Address ContractAddress { get; set; }

    public Address Sender => Address.FromPublicKey(_senderKey.PublicKey);

    public IMethodStub<TInput, TOutput> Create<TInput, TOutput>(Method<TInput, TOutput> method)
        where TInput : IMessage<TInput>, new() where TOutput : IMessage<TOutput>, new()
    {
        Transaction GetUnsignedTransaction(TInput input, Method<TInput, TOutput> method)
        {
            return new Transaction
            {
                From = Sender,
                To = _contractAddress,
                MethodName = method.Name,
                Params = ByteString.CopyFrom(method.RequestMarshaller.Serializer(input))
            };
        }

        async Task<Transaction> GetTransactionAsync(TInput input)
        {
            var refBlockInfo = await _client.GetRefBlockInfoAsync();
            var transaction = GetUnsignedTransaction(input, method);
            transaction.RefBlockNumber = refBlockInfo.Height;
            transaction.RefBlockPrefix = refBlockInfo.Prefix;

            var signature = CryptoHelper.SignWithPrivateKey(
                _senderKey.PrivateKey, transaction.GetHash().Value.ToByteArray());
            transaction.Signature = ByteString.CopyFrom(signature);
            return transaction;
        }

        Transaction GetTransaction(TInput input)
        {
            return AsyncContext.Run(() => GetTransactionAsync(input));
        }

        async Task<IExecutionResult<TOutput>> SendAsync(TInput input)
        {
            var transaction = await GetTransactionAsync(input);
            var sendResult = await _client.SendTransactionAsync(new SendTransactionInput()
            {
                RawTransaction = transaction.ToByteArray().ToHex()
            });

            if (sendResult == null) return new ExecutionResult<TOutput> { Transaction = transaction };

            // TODO: Must wait if transaction status is pending
            var (transactionResult, returnValue) =
                await _client.WaitForTransactionCompletionAsync(Hash.LoadFromHex(sendResult.TransactionId));

            var status = transactionResult.Status;
            switch (status)
            {
                case TransactionResultStatus.Mined:
                    break;
                case TransactionResultStatus.Pending:
                case TransactionResultStatus.PendingValidation:
                    throw new Exception($"Transaction is {status}");
                case TransactionResultStatus.NotExisted:
                    throw new Exception("Transaction does not exist");
                case TransactionResultStatus.Failed:
                case TransactionResultStatus.Conflict:
                case TransactionResultStatus.NodeValidationFailed:
                    throw new Exception($"Transaction failed with status: {status}. Error: {transactionResult.Error}");
                default:
                    throw new Exception($"Unexpected transaction status: {status}. {transactionResult}");
            }


            return new ExecutionResult<TOutput>
            {
                Transaction = transaction,
                TransactionResult = transactionResult,
                Output = method.ResponseMarshaller.Deserializer(returnValue.ToByteArray())
            };
        }

        async Task<IExecutionResult<TOutput>> SendWithExceptionAsync(TInput input)
        {
            // Not available in the context
            throw new NotImplementedException();
        }

        async Task<TOutput> CallAsync(TInput input)
        {
            var transaction = GetUnsignedTransaction(input, method);

            var sendResult = await _client.ExecuteTransactionAsync(new ExecuteTransactionDto()
            {
                RawTransaction = transaction.ToByteArray().ToHex()
            });

            return method.ResponseMarshaller.Deserializer(ByteString.FromBase64(sendResult).ToByteArray());
        }

        async Task<StringValue> CallWithExceptionAsync(TInput input)
        {
            // Not available in the context
            throw new NotImplementedException();
        }


        return new MethodStub<TInput, TOutput>(method, SendAsync, CallAsync, GetTransaction, SendWithExceptionAsync,
            CallWithExceptionAsync);
    }
}