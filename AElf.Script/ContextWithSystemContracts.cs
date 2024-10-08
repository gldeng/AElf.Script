using AElf.Contracts.MultiToken;
using AElf.Standards.ACS0;
using AElf.Standards.ACS3;

namespace AElf.Scripts;

public class ContextWithSystemContracts : Context
{
    public AuthorizationContractContainer.AuthorizationContractStub Parliament =>
        this.GetInstance<AuthorizationContractContainer.AuthorizationContractStub>(
            "2JT8xzjR5zJ8xnBvdgBZdSjfbokFSbF5hDdpUCbXeWaJfPDmsK"
        );

    public ACS0Container.ACS0Stub Genesis => this.GetInstance<ACS0Container.ACS0Stub>(
        "pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i"
    );

    internal TokenContractContainer.TokenContractStub TokenContractStub =>
        this.GetInstance<TokenContractContainer.TokenContractStub>(
            "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE"
        );
}