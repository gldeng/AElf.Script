using AElf.Contracts.MultiToken;
using AElf.Types;

// ReSharper disable TooManyArguments

namespace AElf.Scripts;

public static partial class Extension
{
    public static async Task TransferTokenAsync(
        this ContextWithSystemContracts ctx,
        Address to,
        long amount,
        string symbol = "ELF",
        string memo = ""
    )
    {
        await ctx.TokenContractStub.Transfer.SendAsync(new TransferInput()
        {
            To = to,
            Amount = amount,
            Symbol = symbol,
            Memo = memo
        });
    }

    public static async Task TransferTokenFromAsync(
        this ContextWithSystemContracts ctx,
        Address from,
        Address to,
        long amount,
        string symbol = "ELF",
        string memo = ""
    )
    {
        await ctx.TokenContractStub.TransferFrom.SendAsync(new TransferFromInput()
        {
            From = from,
            To = to,
            Amount = amount,
            Symbol = symbol,
            Memo = memo
        });
    }

    public static async Task ApproveTokenAllowanceAsync(
        this ContextWithSystemContracts ctx,
        Address spender,
        long amount,
        string symbol = "ELF"
    )
    {
        await ctx.TokenContractStub.Approve.SendAsync(new ApproveInput()
        {
            Spender = spender,
            Amount = amount,
            Symbol = symbol,
        });
    }


    public static async Task UnApproveTokenAllowanceAsync(
        this ContextWithSystemContracts ctx,
        Address spender,
        long amount,
        string symbol = "ELF"
    )
    {
        await ctx.TokenContractStub.UnApprove.SendAsync(new UnApproveInput()
        {
            Spender = spender,
            Amount = amount,
            Symbol = symbol
        });
    }

    public static async Task<long> GetBalanceAsync(
        this ContextWithSystemContracts ctx,
        Address owner,
        string symbol = "ELF"
    )
    {
        var output = await ctx.TokenContractStub.GetBalance.CallAsync(new GetBalanceInput()
        {
            Owner = owner,
            Symbol = symbol
        });
        return output.Balance;
    }
}