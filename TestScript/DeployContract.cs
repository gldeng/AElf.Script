using AElf.Scripts;
using AElf.Types;
using Microsoft.Extensions.Logging;

namespace TestScript;

public class DeployContractTask : Script
{
    public DeployContractTask(string contractPathName)
    {
        ContractPathName = contractPathName;
    }

    public string ContractPathName { get; set; }
    public Address? DeployedAddress { get; private set; }

    public override async Task RunAsync()
    {
        var address = await this.DeployContractAsync(ContractPathName);
        DeployedAddress = address;
        Logger.LogInformation($"Deployed to {address}");
    }
}