using AElf.Scripts;

namespace TestScript;

public class DeployMerkleTreeContractTask : Script
{
    public static string ContractPathName { get; set; }

    public override async Task RunAsync()
    {
        await this.DeployContractAsync(ContractPathName);
    }
}