using AElf.Contracts.Configuration;
using AElf.CSharp.Core.Extension;
using AElf.Standards.ACS3;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace AElf.Script.Predefined;

public class InitScript : Script
{
    public override async Task RunAsync()
    {
        await AllowGenesisContractToProposeAsync();
        await ChangeCallThresholdAsync();
        var configValue = await this.GetConfigurationAsync<Int32Value>(CallCountThresholdKey);
        Logger.LogInformation($"{CallCountThresholdKey} value is {configValue}");
    }

    private async Task AllowGenesisContractToProposeAsync()
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
    }

    private async Task ChangeCallThresholdAsync()
    {
        var callThreshold = Environment.GetEnvironmentVariable(EnvVarNames.AELF_CONFIG_CALL_THRESHOLD.ToString()) ??
                            "80000";
        var tx = ConfigurationContractStub.SetConfiguration.GetTransaction(new SetConfigurationInput()
        {
            Key = CallCountThresholdKey,
            Value = new Int32Value()
            {
                Value = int.Parse(callThreshold)
            }.ToByteString()
        });

        var tx1 = await Parliament.CreateProposal.SendAsync(new CreateProposalInput()
        {
            ContractMethodName = tx.MethodName,
            Params = tx.Params,
            ToAddress = Address.FromBase58("2iQBrmFhk8HAxgDeL5fyupghzs7ZConf8KMyhkYZFSHnNsNQsn"),
            ExpiredTime = DateTime.UtcNow.ToTimestamp().AddDays(5),
            OrganizationAddress = Address.FromBase58("aeXhTqNwLWxCG6AzxwnYKrPMWRrzZBskW3HWVD9YREMx1rJxG"),
        });
        var proposalId1 = tx1.Output;
        await Parliament.Approve.SendAsync(proposalId1);
        await Parliament.Release.SendAsync(proposalId1);
    }

    private const string CallCountThresholdKey = "CallCountThreshold";
}