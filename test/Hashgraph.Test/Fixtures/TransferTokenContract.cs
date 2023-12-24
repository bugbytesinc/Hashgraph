namespace Hashgraph.Test.Fixtures;

public class TransferTokenContract : IAsyncDisposable
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
    /// The contract 'bytecode' encoded in Hex, Compiled using HardHat
    /// </summary>
    private const string TRANSFER_CONTRACT_BYTECODE = "0x608060405234801561001057600080fd5b50610520806100206000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c8063eca3691714610030575b600080fd5b61004a60048036038101906100459190610252565b610060565b60405161005791906103eb565b60405180910390f35b60007f0728e02ffc672da3da2cf5a1c530e83e86bcd8efa5f88d0ce7cc88b94ededcb3858585856040516100979493929190610353565b60405180910390a160008061016773ffffffffffffffffffffffffffffffffffffffff167feca369170841cc24e1e61a5b964f1fa4c22cc61112ae96fc3223db7b7f0ad7ac888888886040516024016100f39493929190610353565b604051602081830303815290604052907bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff838183161783525050505060405161015d919061033c565b6000604051808303816000865af19150503d806000811461019a576040519150601f19603f3d011682016040523d82523d6000602084013e61019f565b606091505b5091509150816101b05760006101c5565b808060200190518101906101c491906102b5565b5b60030b92507fa4cf7c7af613868bb4f26ad5352eedcc5984c76da511c035831b76fb78ea0df78787878787604051610201959493929190610398565b60405180910390a15050949350505050565b600081359050610222816104a5565b92915050565b600081519050610237816104bc565b92915050565b60008135905061024c816104d3565b92915050565b6000806000806080858703121561026857600080fd5b600061027687828801610213565b945050602061028787828801610213565b935050604061029887828801610213565b92505060606102a98782880161023d565b91505092959194509250565b6000602082840312156102c757600080fd5b60006102d584828501610228565b91505092915050565b6102e78161041c565b82525050565b60006102f882610406565b6103028185610411565b9350610312818560208601610472565b80840191505092915050565b6103278161042e565b82525050565b61033681610445565b82525050565b600061034882846102ed565b915081905092915050565b600060808201905061036860008301876102de565b61037560208301866102de565b61038260408301856102de565b61038f606083018461032d565b95945050505050565b600060a0820190506103ad60008301886102de565b6103ba60208301876102de565b6103c760408301866102de565b6103d4606083018561032d565b6103e1608083018461031e565b9695505050505050565b6000602082019050610400600083018461031e565b92915050565b600081519050919050565b600081905092915050565b600061042782610452565b9050919050565b6000819050919050565b60008160030b9050919050565b60008160070b9050919050565b600073ffffffffffffffffffffffffffffffffffffffff82169050919050565b60005b83811015610490578082015181840152602081019050610475565b8381111561049f576000848401525b50505050565b6104ae8161041c565b81146104b957600080fd5b50565b6104c581610438565b81146104d057600080fd5b50565b6104dc81610445565b81146104e757600080fd5b5056fea2646970667358221220a233d884ac3d74624f90e68ab5b5e920a7fd09364aa34ab5045d2ffb858d497c64736f6c63430008040033";

    public static async Task<TransferTokenContract> CreateAsync(NetworkCredentials networkCredentials, Action<TransferTokenContract> customize = null)
    {
        var fx = new TransferTokenContract();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Creating Transfer Token Contract Instance");
        (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
        fx.Network = networkCredentials;
        fx.TestInstanceId = Generator.Code(10);
        fx.Memo = ".NET SDK Test: Instantiating Contract Instance " + fx.TestInstanceId;
        fx.FileParams = new CreateFileParams
        {
            Expiration = DateTime.UtcNow.AddSeconds(7890000),
            Endorsements = new Endorsement[] { networkCredentials.PublicKey },
            Contents = Encoding.UTF8.GetBytes(TRANSFER_CONTRACT_BYTECODE)
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
        fx.Network.Output?.WriteLine("SETUP COMPLETED: FTXFR Contract Instance Created");
        return fx;
    }
    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: FTXFR Contract Instance");
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
        Network.Output?.WriteLine("TEARDOWN COMPLETED FTXFR Contract Instance");
    }
    public static implicit operator Address(TransferTokenContract fxContract)
    {
        return fxContract.ContractRecord.Contract;
    }
}