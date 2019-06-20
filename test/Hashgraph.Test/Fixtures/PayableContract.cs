using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class PayableContract : IAsyncDisposable
    {
        public Client Client;
        public CreateFileParams FileParams;
        public FileRecord FileRecord;
        public CreateContractParams ContractParams;
        public ContractRecord ContractRecord;
        public NetworkCredentials Network;

        /// <summary>
        /// The contract 'bytecode' encoded in Hex, Same as hello_world from java sdk, compiled in Remix for with Solidity 0.5.4
        /// </summary>
        private const string PAYABLE_CONTRACT_BYTECODE = "0x6080604052336000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555061032a806100536000396000f3fe608060405260043610610051576000357c01000000000000000000000000000000000000000000000000000000009004806341c0e1b514610053578063c1cfb99a1461006a578063e264d18314610095575b005b34801561005f57600080fd5b506100686100e6565b005b34801561007657600080fd5b5061007f6101a6565b6040518082815260200191505060405180910390f35b3480156100a157600080fd5b506100e4600480360360208110156100b857600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff1690602001909291905050506101c5565b005b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614151561018d576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252602b8152602001806102d4602b913960400191505060405180910390fd5b3373ffffffffffffffffffffffffffffffffffffffff16ff5b60003073ffffffffffffffffffffffffffffffffffffffff1631905090565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614151561026c576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252602b8152602001806102d4602b913960400191505060405180910390fd5b60003073ffffffffffffffffffffffffffffffffffffffff163190508173ffffffffffffffffffffffffffffffffffffffff166108fc829081150290604051600060405180830381858888f193505050501580156102ce573d6000803e3d6000fd5b50505056fe4f6e6c7920636f6e7472616374206f776e65722063616e2063616c6c20746869732066756e6374696f6e2ea165627a7a72305820958b3369a655d57506babce1f72e76d752de46eed693cc101284b76522f404170029";

        public static async Task<PayableContract> SetupAsync(NetworkCredentials networkCredentials)
        {
            networkCredentials.Output?.WriteLine("STARTING SETUP: Payable Contract Create Configuration");
            var fx = await InternalSetupAsync(networkCredentials);
            networkCredentials.Output?.WriteLine("SETUP PAUSED: Payable Contract Configuration Initialized");
            return fx;
        }
        public static async Task<PayableContract> CreateAsync(NetworkCredentials networkCredentials)
        {
            networkCredentials.Output?.WriteLine("STARTING SETUP: Creating Payable Contract Instance");
            var fx = await InternalSetupAsync(networkCredentials);
            await fx.InternalCompleteCreateAsync();
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Payable Contract Instance Created");
            return fx;
        }
        public async Task CompleteCreateAsync()
        {
            Network.Output?.WriteLine("SETUP CONTINUE: Payable Contract Create Resumed");
            await InternalCompleteCreateAsync();
            Network.Output?.WriteLine("SETUP COMPLETED: Payable Contract Instance Created");
        }
        private static async Task<PayableContract> InternalSetupAsync(NetworkCredentials networkCredentials)
        {
            var fx = new PayableContract();
            fx.Network = networkCredentials;
            fx.FileParams = new CreateFileParams
            {
                Expiration = Generator.TruncatedFutureDate(12, 24),
                Endorsements = new Endorsement[] { networkCredentials.PublicKey },
                Contents = Encoding.UTF8.GetBytes(PAYABLE_CONTRACT_BYTECODE)
            };
            fx.Client = networkCredentials.NewClient();
            fx.FileRecord = await fx.Client.CreateFileWithRecordAsync(fx.FileParams, ctx =>
            {
                ctx.Memo = "Payable Contract Create: Uploading Contract File " + Generator.Code(10);
            });
            Assert.Equal(ResponseCode.Success, fx.FileRecord.Status);
            fx.ContractParams = new CreateContractParams
            {
                File = fx.FileRecord.File,
                Administrator = networkCredentials.PublicKey,
                Gas = 500_000,
                InitialBalance = 1_000_000,
                RenewPeriod = TimeSpan.FromDays(Generator.Integer(2, 4))
            };
            return fx;
        }
        private async Task InternalCompleteCreateAsync()
        {
            ContractRecord = await Client.CreateContractWithRecordAsync(ContractParams, ctx =>
            {
                ctx.Memo = "Payable Contract Create: Instantiating Payable Instance " + Generator.Code(10);
            });
            Assert.Equal(ResponseCode.Success, FileRecord.Status);
        }
        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Stateful Contract Instance");
            try
            {
                await Client.DeleteFileAsync(FileRecord.File, ctx =>
                {
                    ctx.Memo = "Stateful Contract Teardown: Delete Contract File (may already be deleted)";
                });
                await Client.DeleteContractAsync(ContractRecord.Contract, Network.Payer, ctx =>
                {
                    ctx.Memo = "Stateful Contract Teardown: Delete Contract (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED Stateful Contract Instance");
        }
    }
}