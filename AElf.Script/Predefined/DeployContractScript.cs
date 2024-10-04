using AElf.Types;
using Microsoft.Extensions.Logging;

namespace AElf.Scripts.Predefined;

public class DeployContractScript : Script
{
    public DeployContractScript(string contractPathName)
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