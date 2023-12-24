namespace Hashgraph.Test.Fixtures;

public class InitCodeContract : IAsyncDisposable
{
    public string TestInstanceId;
    public string Memo;
    public Client Client;
    public CreateContractParams ContractParams;
    public CreateContractRecord ContractRecord;
    public NetworkCredentials Network;
    public ReadOnlyMemory<byte> PublicKey;
    public ReadOnlyMemory<byte> PrivateKey;

    /// <summary>
    /// The contract 'bytecode' encoded in Hex, Same as hello_world from java sdk, compiled in Remix for with Solidity 0.5.4
    /// </summary>
    public const string CONTRACT_BYTECODE = "608060405234801561001057600080fd5b50336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506101be806100606000396000f3fe608060405234801561001057600080fd5b5060043610610053576000357c01000000000000000000000000000000000000000000000000000000009004806341c0e1b514610058578063cfae321714610062575b600080fd5b6100606100e5565b005b61006a610155565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156100aa57808201518184015260208101905061008f565b50505050905090810190601f1680156100d75780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610153573373ffffffffffffffffffffffffffffffffffffffff16ff5b565b60606040805190810160405280600d81526020017f48656c6c6f2c20776f726c64210000000000000000000000000000000000000081525090509056fea165627a7a7230582077fbec49f64eda23cb526275088f65c1fc7e8d002b4681e098f18292791cd94b0029";

    public static async Task<InitCodeContract> CreateAsync(NetworkCredentials networkCredentials, Action<InitCodeContract> customize = null)
    {
        var fx = new InitCodeContract();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Creating Greeting Contract Instance");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.TestInstanceId = Generator.Code(10);
        fx.Memo = ".NET SDK Test: Instantiating Contract Instance " + fx.TestInstanceId;
        fx.Client = networkCredentials.NewClient();
        fx.ContractParams = new CreateContractParams
        {
            ByteCode = Hex.ToBytes(CONTRACT_BYTECODE),
            Administrator = fx.PublicKey,
            Signatory = fx.PrivateKey,
            Gas = 200000,
            RenewPeriod = TimeSpan.FromSeconds(7890000),
            Memo = ".NET SDK Test: " + Generator.Code(10)
        };
        customize?.Invoke(fx);
        fx.ContractRecord = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.CreateContractWithRecordAsync(fx.ContractParams, ctx =>
            {
                ctx.Memo = fx.Memo;
            });
        });
        Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
        fx.Network.Output?.WriteLine("SETUP COMPLETED: InitCode Contract Instance Created");
        return fx;
    }
    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: InitCode Contract Instance");
        try
        {
            await Client.DeleteContractAsync(ContractRecord.Contract, Network.Payer, PrivateKey, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Delete Contract (may already be deleted) " + TestInstanceId;
            });
        }
        catch
        {
            //noop
        }
        await Client.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED InitCode Contract Instance");
    }
    public static implicit operator Address(InitCodeContract fxContract)
    {
        return fxContract.ContractRecord.Contract;
    }
}