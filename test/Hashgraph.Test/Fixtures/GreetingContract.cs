﻿namespace Hashgraph.Test.Fixtures;

public class GreetingContract : IHasCryptoBalance, IHasTokenBalance, IAsyncDisposable
{
    public string TestInstanceId;
    public string Memo;
    public Client Client;
    public CreateFileParams FileParams;
    public FileRecord FileRecord;
    public CreateContractParams ContractParams;
    public CreateContractRecord ContractRecord;
    public NetworkCredentials Network;
    public ReadOnlyMemory<byte> PublicKey;
    public ReadOnlyMemory<byte> PrivateKey;

    /// <summary>
    /// The contract 'bytecode' encoded in Hex, Same as hello_world from java sdk, compiled in Remix for with Solidity 0.5.4
    /// </summary>
    private const string GREETING_CONTRACT_BYTECODE = "0x608060405234801561001057600080fd5b50336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055506101be806100606000396000f3fe608060405234801561001057600080fd5b5060043610610053576000357c01000000000000000000000000000000000000000000000000000000009004806341c0e1b514610058578063cfae321714610062575b600080fd5b6100606100e5565b005b61006a610155565b6040518080602001828103825283818151815260200191508051906020019080838360005b838110156100aa57808201518184015260208101905061008f565b50505050905090810190601f1680156100d75780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610153573373ffffffffffffffffffffffffffffffffffffffff16ff5b565b60606040805190810160405280600d81526020017f48656c6c6f2c20776f726c64210000000000000000000000000000000000000081525090509056fea165627a7a7230582077fbec49f64eda23cb526275088f65c1fc7e8d002b4681e098f18292791cd94b0029";

    public static async Task<GreetingContract> CreateAsync(NetworkCredentials networkCredentials, Action<GreetingContract> customize = null)
    {
        var fx = new GreetingContract();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Creating Greeting Contract Instance");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.TestInstanceId = Generator.Code(10);
        fx.Memo = ".NET SDK Test: Instantiating Contract Instance " + fx.TestInstanceId;
        fx.FileParams = new CreateFileParams
        {
            Expiration = DateTime.UtcNow.AddSeconds(7890000),
            Endorsements = new Endorsement[] { networkCredentials.PublicKey },
            Contents = Encoding.UTF8.GetBytes(GREETING_CONTRACT_BYTECODE)
        };
        fx.Client = networkCredentials.NewClient();
        fx.FileRecord = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.CreateFileWithRecordAsync(fx.FileParams, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Uploading Contract File " + fx.TestInstanceId;
            });
        });
        Assert.Equal(ResponseCode.Success, fx.FileRecord.Status);
        fx.ContractParams = new CreateContractParams
        {
            File = fx.FileRecord.File,
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
        fx.Network.Output?.WriteLine("SETUP COMPLETED: Greeting Contract Instance Created");
        return fx;
    }
    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Greeting Contract Instance");
        try
        {
            await Client.DeleteFileAsync(FileRecord.File, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Delete Contract File (may already be deleted) " + TestInstanceId;
            });
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
        Network.Output?.WriteLine("TEARDOWN COMPLETED Greeting Contract Instance");
    }

    public async Task<ulong> GetCryptoBalanceAsync()
    {
        return await Client.GetContractBalanceAsync(ContractRecord.Contract);
    }

    public async Task<long?> GetTokenBalanceAsync(Address token)
    {
        return await (await Network.GetMirrorRestClientAsync()).GetAccountTokenBalanceAsync(ContractRecord.Contract, token);
    }

    public async Task<TokenHoldingData[]> GetTokenBalancesAsync()
    {
        var list = new List<TokenHoldingData>();
        await foreach (var info in (await Network.GetMirrorRestClientAsync()).GetAccountTokenHoldingsAsync(ContractRecord.Contract))
        {
            list.Add(info);
        }
        return list.ToArray();
    }

    public static implicit operator Address(GreetingContract fxContract)
    {
        return fxContract.ContractRecord.Contract;
    }
}