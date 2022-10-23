using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures;

public class EventEmittingContract : IAsyncDisposable
{
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
    private const string EVENTEMIT_CONTRACT_BYTECODE = "0x6080604052336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550610395806100536000396000f3fe608060405260043610610051576000357c01000000000000000000000000000000000000000000000000000000009004806341c0e1b514610053578063c1cfb99a1461006a578063e264d18314610095575b005b34801561005f57600080fd5b506100686100e6565b005b34801561007657600080fd5b5061007f6101a6565b6040518082815260200191505060405180910390f35b3480156100a157600080fd5b506100e4600480360360208110156100b857600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff1690602001909291905050506101c5565b005b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614151561018d576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252602b81526020018061033f602b913960400191505060405180910390fd5b3373ffffffffffffffffffffffffffffffffffffffff16ff5b60003073ffffffffffffffffffffffffffffffffffffffff1631905090565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614151561026c576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252602b81526020018061033f602b913960400191505060405180910390fd5b60003073ffffffffffffffffffffffffffffffffffffffff163190508173ffffffffffffffffffffffffffffffffffffffff166108fc829081150290604051600060405180830381858888f193505050501580156102ce573d6000803e3d6000fd5b507f9277a4302be4a765ae8585e09a9306bd55da10e20e59ed4f611a04ba606fece88282604051808373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019250505060405180910390a1505056fe4f6e6c7920636f6e7472616374206f776e65722063616e2063616c6c20746869732066756e6374696f6e2ea165627a7a72305820e0cd44f59bcdc2845896eeb3600e442d57fc43c3a951c5440530dab87eaf44f10029";

    public static async Task<EventEmittingContract> CreateAsync(NetworkCredentials networkCredentials, Action<EventEmittingContract> customize = null)
    {
        var fx = new EventEmittingContract();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Event Emit Contract Create Configuration");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.FileParams = new CreateFileParams
        {
            Expiration = DateTime.UtcNow.AddSeconds(7890000),//Generator.TruncatedFutureDate(12, 24),
            Endorsements = new Endorsement[] { networkCredentials.PublicKey },
            Contents = Encoding.UTF8.GetBytes(EVENTEMIT_CONTRACT_BYTECODE)
        };
        fx.Client = networkCredentials.NewClient();
        fx.FileRecord = await fx.Client.RetryKnownNetworkIssues(async client =>
        {
            return await fx.Client.CreateFileWithRecordAsync(fx.FileParams, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Uploading Contract File " + Generator.Code(10);
            });
        });
        Assert.Equal(ResponseCode.Success, fx.FileRecord.Status);
        fx.ContractParams = new CreateContractParams
        {
            File = fx.FileRecord.File,
            Administrator = fx.PublicKey,
            Signatory = fx.PrivateKey,
            Gas = 300000,
            InitialBalance = 1_000_000,
            RenewPeriod = TimeSpan.FromSeconds(7890000),//TimeSpan.FromDays(Generator.Integer(2, 4))
        };
        customize?.Invoke(fx);
        fx.ContractRecord = await fx.Client.RetryKnownNetworkIssues(async client =>
        {
            return await fx.Client.CreateContractWithRecordAsync(fx.ContractParams, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Instantiating Event Emit Instance " + Generator.Code(10);
            });
        });
        Assert.Equal(ResponseCode.Success, fx.FileRecord.Status);
        fx.Network.Output?.WriteLine("SETUP COMPLETED: Event Emit Contract Instance Created");
        return fx;
    }
    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Event Emit Contract Instance");
        try
        {
            await Client.DeleteFileAsync(FileRecord.File, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Delete Contract File (may already be deleted)";
            });
            await Client.DeleteContractAsync(ContractRecord.Contract, Network.Payer, PrivateKey, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Delete Contract (may already be deleted)";
            });
        }
        catch
        {
            //noop
        }
        await Client.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED Event Emit Contract Instance");
    }
    public static implicit operator Address(EventEmittingContract fxContract)
    {
        return fxContract.ContractRecord.Contract;
    }
}