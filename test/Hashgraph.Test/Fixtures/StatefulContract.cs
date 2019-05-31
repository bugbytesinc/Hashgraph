using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class StatefulContract : IAsyncDisposable
    {
        public Account Payer;
        public Client Client;
        public CreateFileParams CreateFileParams;
        public FileRecord FileCreateRecord;
        public CreateContractParams CreateContractParams;
        public ContractRecord ContractCreateRecord;
        public NetworkCredentials NetworkCredentials;

        /// <summary>
        /// The contract 'bytecode' encoded in Hex, Same as hello_world from java sdk, compiled in Remix for with Solidity 0.5.4
        /// </summary>
        private const string STATEFUL_CONTRACT_BYTECODE = "0x608060405234801561001057600080fd5b506040516105ab3803806105ab8339810180604052602081101561003357600080fd5b81019080805164010000000081111561004b57600080fd5b8281019050602081018481111561006157600080fd5b815185600182028301116401000000008211171561007e57600080fd5b50509291905050506000815111151561009657600080fd5b336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555080600190805190602001906100ec9291906100f3565b5050610198565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061013457805160ff1916838001178555610162565b82800160010185558215610162579182015b82811115610161578251825591602001919060010190610146565b5b50905061016f9190610173565b5090565b61019591905b80821115610191576000816000905550600101610179565b5090565b90565b610404806101a76000396000f3fe608060405234801561001057600080fd5b506004361061005e576000357c0100000000000000000000000000000000000000000000000000000000900480632e9826021461006357806332af2edb1461011e57806341c0e1b5146101a1575b600080fd5b61011c6004803603602081101561007957600080fd5b810190808035906020019064010000000081111561009657600080fd5b8201836020820111156100a857600080fd5b803590602001918460018302840111640100000000831117156100ca57600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600081840152601f19601f8201169050808301925050505050505091929192905050506101ab565b005b610126610221565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561016657808201518184015260208101905061014b565b50505050905090810190601f1680156101935780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6101a96102c3565b005b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415156102065761021e565b806001908051906020019061021c929190610333565b505b50565b606060018054600181600116156101000203166002900480601f0160208091040260200160405190810160405280929190818152602001828054600181600116156101000203166002900480156102b95780601f1061028e576101008083540402835291602001916102b9565b820191906000526020600020905b81548152906001019060200180831161029c57829003601f168201915b5050505050905090565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161415610331573373ffffffffffffffffffffffffffffffffffffffff16ff5b565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061037457805160ff19168380011785556103a2565b828001600101855582156103a2579182015b828111156103a1578251825591602001919060010190610386565b5b5090506103af91906103b3565b5090565b6103d591905b808211156103d15760008160009055506001016103b9565b5090565b9056fea165627a7a7230582073d9bfd2161f19cd3534934a222f5715c6ebcecb05ec3acecb351e095ad7d0650029";
        public static async Task<StatefulContract> SetupAsync(NetworkCredentials networkCredentials)
        {
            networkCredentials.Output?.WriteLine("STARTING SETUP: Stateful Contract Configuration");
            var fx = await InternalSetupAsync(networkCredentials);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Stateful Contract Configuration");
            return fx;
        }
        public static async Task<StatefulContract> CreateAsync(NetworkCredentials networkCredentials)
        {
            networkCredentials.Output?.WriteLine("STARTING SETUP: Stateful Contract Instance");
            var fx = await InternalSetupAsync(networkCredentials);
            await fx.CompleteCreateAsync();
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Stateful Contract Instance");
            return fx;
        }
        public async Task CompleteCreateAsync()
        {
            ContractCreateRecord = await Client.CreateContractWithRecordAsync(CreateContractParams, ctx =>
            {
                ctx.Memo = "Stateful Contract Create: Instantiating Stateful Instance";
            });
            Assert.Equal(ResponseCode.Success, FileCreateRecord.Status);
        }
        private static async Task<StatefulContract> InternalSetupAsync(NetworkCredentials networkCredentials)
        {
            var fx = new StatefulContract();
            fx.NetworkCredentials = networkCredentials;
            fx.Payer = networkCredentials.Payer;
            fx.CreateFileParams = new CreateFileParams
            {
                Expiration = Generator.TruncatedFutureDate(12, 24),
                Endorsements = new Endorsement[] { networkCredentials.PublicKey },
                Contents = Encoding.UTF8.GetBytes(STATEFUL_CONTRACT_BYTECODE)
            };
            fx.Client = networkCredentials.NewClient();
            fx.FileCreateRecord = await fx.Client.CreateFileWithRecordAsync(fx.CreateFileParams, ctx =>
            {
                ctx.Memo = "Stateful Contract Create: Uploading Contract File";
            });
            Assert.Equal(ResponseCode.Success, fx.FileCreateRecord.Status);
            fx.CreateContractParams = new CreateContractParams
            {
                File = fx.FileCreateRecord.File,
                Administrator = networkCredentials.PublicKey,
                Gas = 500_000,
                RenewPeriod = TimeSpan.FromDays(Generator.Integer(2, 4)),
                Arguments = new object[] { "Hello from .NET. " + DateTime.UtcNow.ToLongDateString() }
            };
            return fx;
        }
        public async ValueTask DisposeAsync()
        {
            NetworkCredentials.Output?.WriteLine("STARTING TEARDOWN: Stateful Contract Instance");
            try
            {
                await Client.DeleteFileAsync(FileCreateRecord.File, ctx =>
                {
                    ctx.Memo = "Stateful Contract Teardown: Attempting to delete Contract File from Network (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            NetworkCredentials.Output?.WriteLine("TEARDOWN COMPLETED Stateful Contract Instance");
        }
    }
}