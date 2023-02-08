using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class CreateAccountByMonikerTests
{
    private readonly NetworkCredentials _network;
    public CreateAccountByMonikerTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Create Account By Moniker: Can Create Account Having Moniker By Regular Means")]
    public async Task CanCreateAccountHavingMonikerByRegularMeans()
    {
        var initialPayment = 1_000_000ul;
        //var (publicKey, privateKey) = Generator.Secp256k1KeyPair();
        var privateKey = Hex.ToBytes("3082024b0201003081ec06072a8648ce3d02013081e0020101302c06072a8648ce3d0101022100fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f3044042000000000000000000000000000000000000000000000000000000000000000000420000000000000000000000000000000000000000000000000000000000000000704410479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8022100fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd036414102010104820155308201510201010420ac2fd3460bd0d2c6b4d275b7fa37616fe315df4478db51cffcd8e58e41d9ac9da081e33081e0020101302c06072a8648ce3d0101022100fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f3044042000000000000000000000000000000000000000000000000000000000000000000420000000000000000000000000000000000000000000000000000000000000000704410479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8022100fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141020101a14403420004b0d1e10c403e22aa3debeab84f1b5765390dcc08255b49ce98126dcd957317f8575f5d458fc55633753d91a823bc550362b4f033e3629a18d094d21e9aa27fd4");
        var publicKey = Hex.ToBytes("308201333081ec06072a8648ce3d02013081e0020101302c06072a8648ce3d0101022100fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f3044042000000000000000000000000000000000000000000000000000000000000000000420000000000000000000000000000000000000000000000000000000000000000704410479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8022100fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd036414102010103420004b0d1e10c403e22aa3debeab84f1b5765390dcc08255b49ce98126dcd957317f8575f5d458fc55633753d91a823bc550362b4f033e3629a18d094d21e9aa27fd4");
        var endorsement = new Endorsement(publicKey);
        var moniker = new Moniker(endorsement);

        var client = _network.NewClient();

        var receipt = await client.CreateAccountAsync(new CreateAccountParams
        {
            Endorsement = endorsement,
            InitialBalance = initialPayment
        });
        Assert.NotNull(receipt);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        _network.Output.WriteLine("PRIVATE");
        _network.Output.WriteLine(Hex.FromBytes(privateKey));
        _network.Output.WriteLine("PUBLIC");
        _network.Output.WriteLine(Hex.FromBytes(publicKey));
        _network.Output.WriteLine("ADDRESS");
        _network.Output.WriteLine(receipt.Address.ToString());
        _network.Output.WriteLine("MONIKER");
        _network.Output.WriteLine(moniker.ToString());

        var xferReceipt1 = await client.TransferAsync(receipt.Address, _network.Payer, 1, new Signatory(privateKey));
        Assert.NotNull(xferReceipt1);
        Assert.Equal(ResponseCode.Success, xferReceipt1.Status);

        var xferReceipt2 = await client.TransferAsync(moniker, _network.Payer, 1, new Signatory(privateKey));
        Assert.NotNull(xferReceipt2);
        Assert.Equal(ResponseCode.Success, xferReceipt2.Status);


        //var xferReceipt2 = await client.TransferAsync(_network.Payer, moniker, 1);
        //Assert.NotNull(xferReceipt2);
        //Assert.Equal(ResponseCode.Success, xferReceipt2.Status);


        //var createReceipt = allReceipts[1] as CreateAccountReceipt;
        //Assert.NotNull(createReceipt);
        //Assert.NotNull(createReceipt.Address);
        //Assert.Equal(_network.ServerRealm, createReceipt.Address.RealmNum);
        //Assert.Equal(_network.ServerShard, createReceipt.Address.ShardNum);
        //Assert.True(createReceipt.Address.AccountNum > 0);
        //Assert.Equal(1, createReceipt.Id.Nonce);

        //var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
        //Assert.NotNull(createReceiptByTx);
        //Assert.NotNull(createReceiptByTx.Address);
        //Assert.Equal(_network.ServerRealm, createReceiptByTx.Address.RealmNum);
        //Assert.Equal(_network.ServerShard, createReceiptByTx.Address.ShardNum);
        //Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
        //Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

        ////var balances = await client.GetAccountBalancesAsync(moniker);
        ////Assert.NotNull(balances);
        ////Assert.Equal(createReceipt.Address, balances.Address);
        ////Assert.True(balances.Crypto > 0);
        ////Assert.Empty(balances.Tokens);

        //var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
        //Assert.Equal(createReceipt.Address, infoFromAccount.Address);
        ////Assert.Equal(moniker, infoFromAccount.Alias);
        //Assert.NotNull(infoFromAccount.ContractId);
        //Assert.False(infoFromAccount.Deleted);
        //Assert.Equal(0, infoFromAccount.ContractNonce);
        //Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
        //Assert.True(infoFromAccount.Balance > 0);
        //Assert.False(infoFromAccount.ReceiveSignatureRequired);
        //Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
        //Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
        //Assert.Equal(0, infoFromAccount.AssetCount);
        //Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
        //Assert.Equal("auto-created account", infoFromAccount.Memo);
        //AssertHg.NotEmpty(infoFromAccount.Ledger);
        //Assert.NotNull(infoFromAccount.StakingInfo);
        //Assert.False(infoFromAccount.StakingInfo.Declined);
        //Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
        //Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
        //Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
        //Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
        //Assert.Equal(0, infoFromAccount.StakingInfo.Node);

        //var infoFromAlias = await client.GetAccountInfoAsync(moniker);
        //Assert.Equal(createReceipt.Address, infoFromAlias.Address);
        ////Assert.Equal(moniker, infoFromAlias.Alias);
        //Assert.NotNull(infoFromAlias.ContractId);
        //Assert.False(infoFromAlias.Deleted);
        //Assert.Equal(0, infoFromAlias.ContractNonce);
        //Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
        //Assert.True(infoFromAlias.Balance > 0);
        //Assert.False(infoFromAlias.ReceiveSignatureRequired);
        //Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, infoFromAlias.AutoRenewAccount);
        //Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        //Assert.Equal(0, infoFromAlias.AssetCount);
        //Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        //Assert.Equal("auto-created account", infoFromAlias.Memo);
        //AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
        //Assert.NotNull(infoFromAlias.StakingInfo);
        //Assert.False(infoFromAlias.StakingInfo.Declined);
        //Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
        //Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
        //Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
        //Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
        //Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    }

    //[Fact(DisplayName = "Create Account By Moniker: Can Create Account")]
    //public async Task CanCreateAccountAsync()
    //{
    //    var initialPayment = 1_00_000_000;


    //    X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
    //    ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

    //    var privateKey = EcdsaSecp256k1Util.PrivateParamsFromDerOrRaw(Hex.ToBytes("f1132e2cb5489bcacc7ce5e1df88ca301604926fa21a876dd300698758c898ce"));


    //    _network.Output.WriteLine("private key");
    //    _network.Output.WriteLine(Hex.FromBytes(privateKey.D.ToByteArrayUnsigned()));

    //    var publicKey = domain.G.Multiply(privateKey.D).Normalize().GetEncoded(true);

    //    _network.Output.WriteLine("public key");
    //    _network.Output.WriteLine(Hex.FromBytes(publicKey));


    //    var endorsement = new Endorsement(KeyType.ECDSASecp256K1, Hex.ToBytes("0381995fba5d20d71a4e18267559bb82359cf901f919ec6e9a1b4d8e9e7f244039"));
    //    //var x = EcdsaSecp256k1Util.PublicParamsFromDerOrRaw(Hex.ToBytes("0381995fba5d20d71a4e18267559bb82359cf901f919ec6e9a1b4d8e9e7f244039"));
    //    //var publicKey = x.Q.GetEncoded(false);

    //    //var publicKey = domain.G.Multiply(privateKey.D).GetEncoded(false);
    //    //var publicKey = Hex.ToBytes("0481995fba5d20d71a4e18267559bb82359cf901f919ec6e9a1b4d8e9e7f244039b672dd5d0ff0cd0cb192491c5176362034d95059cc47c21b2ccd6a5856e1364b").ToArray();

    //    //_network.Output.WriteLine("public key");
    //    //_network.Output.WriteLine(Hex.FromBytes(publicKey));

    //    //var digest = new KeccakDigest(256);
    //    //digest.BlockUpdate(publicKey, 1, publicKey.Length - 1);
    //    //var hash = new byte[digest.GetDigestSize()];
    //    //digest.DoFinal(hash, 0);

    //    //_network.Output.WriteLine("hash");
    //    //_network.Output.WriteLine(Hex.FromBytes(hash));


    //    //var moniker = new Moniker(hash.AsMemory()[^20..]);

    //    var moniker = new Moniker(endorsement);

    //    _network.Output.WriteLine("moniker");
    //    _network.Output.WriteLine(moniker.ToString());

    //    var client = _network.NewClient();
    //    var receipt = await client.TransferAsync(_network.Payer, moniker, initialPayment);
    //    Assert.NotNull(receipt);

    //    //// If an account was created by the moniker, the receipt
    //    //// with the address is a "child receipt" of the transfer
    //    //// receipt and must be explictly asked for.
    //    //var allReceipts = await client.GetAllReceiptsAsync(receipt.Id);
    //    //Assert.Equal(2, allReceipts.Count);
    //    //Assert.Equal(receipt, allReceipts[0]);

    //    //var createReceipt = allReceipts[1] as CreateAccountReceipt;
    //    //Assert.NotNull(createReceipt);
    //    //Assert.NotNull(createReceipt.Address);
    //    //Assert.Equal(_network.ServerRealm, createReceipt.Address.RealmNum);
    //    //Assert.Equal(_network.ServerShard, createReceipt.Address.ShardNum);
    //    //Assert.True(createReceipt.Address.AccountNum > 0);
    //    //Assert.Equal(1, createReceipt.Id.Nonce);

    //    //var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
    //    //Assert.NotNull(createReceiptByTx);
    //    //Assert.NotNull(createReceiptByTx.Address);
    //    //Assert.Equal(_network.ServerRealm, createReceiptByTx.Address.RealmNum);
    //    //Assert.Equal(_network.ServerShard, createReceiptByTx.Address.ShardNum);
    //    //Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
    //    //Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

    //    ////var balances = await client.GetAccountBalancesAsync(moniker);
    //    ////Assert.NotNull(balances);
    //    ////Assert.Equal(createReceipt.Address, balances.Address);
    //    ////Assert.True(balances.Crypto > 0);
    //    ////Assert.Empty(balances.Tokens);

    //    //var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
    //    //Assert.Equal(createReceipt.Address, infoFromAccount.Address);
    //    ////Assert.Equal(moniker, infoFromAccount.Alias);
    //    //Assert.NotNull(infoFromAccount.ContractId);
    //    //Assert.False(infoFromAccount.Deleted);
    //    //Assert.Equal(0, infoFromAccount.ContractNonce);
    //    //Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
    //    //Assert.True(infoFromAccount.Balance > 0);
    //    //Assert.False(infoFromAccount.ReceiveSignatureRequired);
    //    //Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
    //    //Assert.Equal(Address.None, infoFromAccount.AutoRenewAccount);
    //    //Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
    //    //Assert.Equal(0, infoFromAccount.AssetCount);
    //    //Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
    //    //Assert.Equal("auto-created account", infoFromAccount.Memo);
    //    //AssertHg.NotEmpty(infoFromAccount.Ledger);
    //    //Assert.NotNull(infoFromAccount.StakingInfo);
    //    //Assert.False(infoFromAccount.StakingInfo.Declined);
    //    //Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
    //    //Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
    //    //Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
    //    //Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
    //    //Assert.Equal(0, infoFromAccount.StakingInfo.Node);

    //    //var infoFromAlias = await client.GetAccountInfoAsync(moniker);
    //    //Assert.Equal(createReceipt.Address, infoFromAlias.Address);
    //    ////Assert.Equal(moniker, infoFromAlias.Alias);
    //    //Assert.NotNull(infoFromAlias.ContractId);
    //    //Assert.False(infoFromAlias.Deleted);
    //    //Assert.Equal(0, infoFromAlias.ContractNonce);
    //    //Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
    //    //Assert.True(infoFromAlias.Balance > 0);
    //    //Assert.False(infoFromAlias.ReceiveSignatureRequired);
    //    //Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
    //    //Assert.Equal(Address.None, infoFromAlias.AutoRenewAccount);
    //    //Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
    //    //Assert.Equal(0, infoFromAlias.AssetCount);
    //    //Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
    //    //Assert.Equal("auto-created account", infoFromAlias.Memo);
    //    //AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
    //    //Assert.NotNull(infoFromAlias.StakingInfo);
    //    //Assert.False(infoFromAlias.StakingInfo.Declined);
    //    //Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
    //    //Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
    //    //Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
    //    //Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
    //    //Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    //}

    //[Fact(DisplayName = "Create Account By Alias: Can Schedule Create Account")]
    //public async Task CanScheduleCreateAccountAsync()
    //{
    //    await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
    //    var initialPayment = 1_00_000_000;
    //    var (publicKey, privateKey) = Generator.KeyPair();
    //    var alias = new Alias(publicKey);
    //    var client = _network.NewClient();
    //    var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment, new PendingParams { PendingPayer = fxPayer });
    //    Assert.NotNull(receipt);
    //    Assert.NotNull(receipt.Pending);

    //    var signingReceipt = await client.SignPendingTransactionAsync(receipt.Pending.Id, fxPayer);
    //    Assert.Equal(ResponseCode.Success, signingReceipt.Status);

    //    // If an account was created by the alias, the receipt
    //    // with the address is a "child receipt" of the transfer
    //    // receipt and must be explictly asked for.
    //    var allReceipts = await client.GetAllReceiptsAsync(receipt.Pending.TxId);
    //    Assert.Equal(2, allReceipts.Count);
    //    Assert.Equal(receipt.Pending.TxId, allReceipts[0].Id);

    //    var createReceipt = allReceipts[1] as CreateAccountReceipt;
    //    Assert.NotNull(createReceipt);
    //    Assert.NotNull(createReceipt.Address);
    //    Assert.Equal(_network.ServerRealm, createReceipt.Address.RealmNum);
    //    Assert.Equal(_network.ServerShard, createReceipt.Address.ShardNum);
    //    Assert.True(createReceipt.Address.AccountNum > 0);
    //    Assert.Equal(1, createReceipt.Id.Nonce);

    //    var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
    //    Assert.NotNull(createReceiptByTx);
    //    Assert.NotNull(createReceiptByTx.Address);
    //    Assert.Equal(_network.ServerRealm, createReceiptByTx.Address.RealmNum);
    //    Assert.Equal(_network.ServerShard, createReceiptByTx.Address.ShardNum);
    //    Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
    //    Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

    //    var balances = await client.GetAccountBalancesAsync(alias);
    //    Assert.NotNull(balances);
    //    Assert.Equal(createReceipt.Address, balances.Address);
    //    Assert.True(balances.Crypto > 0);
    //    Assert.Empty(balances.Tokens);

    //    var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
    //    Assert.Equal(createReceipt.Address, infoFromAccount.Address);
    //    Assert.Equal(alias, infoFromAccount.Alias);
    //    Assert.NotNull(infoFromAccount.ContractId);
    //    Assert.False(infoFromAccount.Deleted);
    //    Assert.Equal(0, infoFromAccount.ContractNonce);
    //    Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
    //    Assert.True(infoFromAccount.Balance > 0);
    //    Assert.False(infoFromAccount.ReceiveSignatureRequired);
    //    Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
    //    Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
    //    Assert.Equal(0, infoFromAccount.AssetCount);
    //    Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
    //    Assert.Equal("auto-created account", infoFromAccount.Memo);
    //    AssertHg.NotEmpty(infoFromAccount.Ledger);
    //    Assert.NotNull(infoFromAccount.StakingInfo);
    //    Assert.False(infoFromAccount.StakingInfo.Declined);
    //    Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
    //    Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Node);

    //    var infoFromAlias = await client.GetAccountInfoAsync(alias);
    //    Assert.Equal(createReceipt.Address, infoFromAlias.Address);
    //    Assert.Equal(alias, infoFromAlias.Alias);
    //    Assert.NotNull(infoFromAlias.ContractId);
    //    Assert.False(infoFromAlias.Deleted);
    //    Assert.Equal(0, infoFromAlias.ContractNonce);
    //    Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
    //    Assert.True(infoFromAlias.Balance > 0);
    //    Assert.False(infoFromAlias.ReceiveSignatureRequired);
    //    Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
    //    Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
    //    Assert.Equal(0, infoFromAlias.AssetCount);
    //    Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
    //    Assert.Equal("auto-created account", infoFromAlias.Memo);
    //    AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
    //    Assert.NotNull(infoFromAccount.StakingInfo);
    //    Assert.False(infoFromAccount.StakingInfo.Declined);
    //    Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
    //    Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Node);
    //}
    //[Fact(DisplayName = "Create Account By Alias: Can Create Account and get Records")]
    //public async Task CanCreateAccountAndGetRecords()
    //{
    //    var initialPayment = 1_00_000_000;
    //    var (publicKey, privateKey) = Generator.KeyPair();
    //    var alias = new Alias(publicKey);
    //    var client = _network.NewClient();
    //    var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment);
    //    Assert.NotNull(receipt);

    //    // If an account was created by the alias, the receipt
    //    // with the address is a "child receipt" of the transfer
    //    // receipt and must be explictly asked for.
    //    var allRecords = await client.GetAllTransactionRecordsAsync(receipt.Id);
    //    Assert.Equal(2, allRecords.Count);
    //    Assert.Null(allRecords[0].ParentTransactionConcensus);

    //    var createRecord = allRecords[1] as CreateAccountRecord;
    //    Assert.NotNull(createRecord);
    //    Assert.NotNull(createRecord.Address);
    //    Assert.Equal(_network.ServerRealm, createRecord.Address.RealmNum);
    //    Assert.Equal(_network.ServerShard, createRecord.Address.ShardNum);
    //    Assert.True(createRecord.Address.AccountNum > 0);
    //    Assert.Equal(1, createRecord.Id.Nonce);
    //    // NETWORK V0.21.0 UNSUPPORTED vvvv
    //    // NOT IMPLEMENTED YET
    //    //Assert.Equal(allRecords[0].Concensus, createRecord.ParentTransactionConcensus);
    //    Assert.Null(createRecord.ParentTransactionConcensus);
    //    // NETWORK V0.21.0 UNSUPPORTED ^^^^

    //    var createRecordByTx = await client.GetTransactionRecordAsync(createRecord.Id) as CreateAccountRecord;
    //    Assert.NotNull(createRecordByTx);
    //    Assert.NotNull(createRecordByTx.Address);
    //    Assert.Equal(_network.ServerRealm, createRecordByTx.Address.RealmNum);
    //    Assert.Equal(_network.ServerShard, createRecordByTx.Address.ShardNum);
    //    Assert.Equal(createRecord.Address, createRecordByTx.Address);
    //    Assert.Equal(createRecord.Id, createRecordByTx.Id);
    //    // NETWORK V0.21.0 UNSUPPORTED vvvv
    //    // NOT IMPLEMENTED YET
    //    //Assert.Equal(allRecords[0].Concensus, createRecordByTx.ParentTransactionConcensus);
    //    Assert.Null(createRecordByTx.ParentTransactionConcensus);
    //    // NETWORK V0.21.0 UNSUPPORTED ^^^^            

    //    var balances = await client.GetAccountBalancesAsync(alias);
    //    Assert.NotNull(balances);
    //    Assert.Equal(createRecord.Address, balances.Address);
    //    Assert.True(balances.Crypto > 0);
    //    Assert.Empty(balances.Tokens);

    //    var infoFromAccount = await client.GetAccountInfoAsync(createRecord.Address);
    //    Assert.Equal(createRecord.Address, infoFromAccount.Address);
    //    Assert.Equal(alias, infoFromAccount.Alias);
    //    Assert.NotNull(infoFromAccount.ContractId);
    //    Assert.False(infoFromAccount.Deleted);
    //    Assert.Equal(0, infoFromAccount.ContractNonce);
    //    Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
    //    Assert.True(infoFromAccount.Balance > 0);
    //    Assert.False(infoFromAccount.ReceiveSignatureRequired);
    //    Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
    //    Assert.True(infoFromAccount.Expiration > ConsensusTimeStamp.MinValue);
    //    Assert.Equal(0, infoFromAccount.AssetCount);
    //    Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
    //    Assert.Equal("auto-created account", infoFromAccount.Memo);
    //    AssertHg.NotEmpty(infoFromAccount.Ledger);
    //    Assert.NotNull(infoFromAccount.StakingInfo);
    //    Assert.False(infoFromAccount.StakingInfo.Declined);
    //    Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAccount.StakingInfo.PeriodStart);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.PendingReward);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Proxied);
    //    Assert.Equal(Address.None, infoFromAccount.StakingInfo.Proxy);
    //    Assert.Equal(0, infoFromAccount.StakingInfo.Node);

    //    var infoFromAlias = await client.GetAccountInfoAsync(alias);
    //    Assert.Equal(createRecord.Address, infoFromAlias.Address);
    //    Assert.Equal(alias, infoFromAlias.Alias);
    //    Assert.NotNull(infoFromAlias.ContractId);
    //    Assert.False(infoFromAlias.Deleted);
    //    Assert.Equal(0, infoFromAlias.ContractNonce);
    //    Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
    //    Assert.True(infoFromAlias.Balance > 0);
    //    Assert.False(infoFromAlias.ReceiveSignatureRequired);
    //    Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
    //    Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
    //    Assert.Equal(0, infoFromAlias.AssetCount);
    //    Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
    //    Assert.Equal("auto-created account", infoFromAlias.Memo);
    //    AssertHg.Equal(infoFromAccount.Ledger, infoFromAlias.Ledger);
    //    Assert.NotNull(infoFromAlias.StakingInfo);
    //    Assert.False(infoFromAlias.StakingInfo.Declined);
    //    Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
    //    Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
    //    Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
    //    Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
    //    Assert.Equal(0, infoFromAlias.StakingInfo.Node);
    //}
}