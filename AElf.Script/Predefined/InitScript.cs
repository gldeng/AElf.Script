using AElf.CSharp.Core.Extension;
using AElf.Standards.ACS3;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts.Predefined;

public class InitScript : Script
{
    public override async Task RunAsync()
    {
        // TODO: Check only executed in dev environment
        var tx = Parliament.ChangeOrganizationProposerWhiteList.GetTransaction(new ProposerWhiteList()
        {
            Proposers = { Address.FromBase58("pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i") }
        });

        var tx1 = await Parliament.CreateProposal.SendAsync(new CreateProposalInput()
        {
            ContractMethodName = tx.MethodName,
            Params = tx.Params,
            ToAddress = Address.FromBase58("2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK"),
            ExpiredTime = DateTime.UtcNow.ToTimestamp().AddDays(5),
            OrganizationAddress = Address.FromBase58("aeXhTqNwLWxCG6AzxwnYKrPMWRrzZBskW3HWVD9YREMx1rJxG"),
        });
        var proposalId1 = tx1.Output;
        await Parliament.Approve.SendAsync(proposalId1);
        await Parliament.Release.SendAsync(proposalId1);
        var proposal = await Parliament.GetProposal.CallAsync(proposalId1);
        Logger.LogDebug(proposal.ToString());
    }
}