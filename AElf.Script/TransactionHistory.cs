using AElf.Types;
using Google.Protobuf;

namespace AElf.Script;

public class SentTransactionRecord
{
    public SentTransactionRecord(Hash transactionId, Transaction transaction, TransactionResult result,
        ByteString returnValue)
    {
        TransactionId = transactionId;
        Transaction = transaction;
        Result = result;
        ReturnValue = returnValue;
    }

    public Hash TransactionId { get; }
    public Transaction Transaction { get; }
    public TransactionResult Result { get; }
    public ByteString ReturnValue { get; }
}

public static class TransactionHistory
{
    private static readonly List<SentTransactionRecord> History = new List<SentTransactionRecord>();

    public static void AddTransaction(Hash transactionId, Transaction transaction, TransactionResult result,
        ByteString returnValue)
    {
        History.Add(new SentTransactionRecord(transactionId, transaction, result, returnValue));
    }

    public static IReadOnlyList<SentTransactionRecord> GetHistory()
    {
        return History.AsReadOnly();
    }

    public static SentTransactionRecord GetLastTransaction()
    {
        return History.Last();
    }
}