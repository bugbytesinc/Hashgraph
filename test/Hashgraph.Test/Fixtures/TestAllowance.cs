﻿namespace Hashgraph.Test.Fixtures;

public class TestAllowance : IAsyncDisposable
{
    public TestToken TestToken;
    public TestAsset TestAsset;
    public TestAccount Owner;
    public TestAccount Agent;
    public TestAccount DelegatedAgent;
    public Client Client;
    public NetworkCredentials Network;
    public TransactionRecord AssociateRecord;
    public TransactionRecord DelegationRecord;

    public static async Task<TestAllowance> CreateAsync(NetworkCredentials networkCredentials)
    {
        var fx = new TestAllowance();
        networkCredentials.Output?.WriteLine("STARTING SETUP: Test Allowance Instance");
        fx.Network = networkCredentials;
        fx.Agent = await TestAccount.CreateAsync(networkCredentials, fxA => fxA.CreateParams.InitialBalance = 50_00_000_000);
        fx.DelegatedAgent = await TestAccount.CreateAsync(networkCredentials, fxA => fxA.CreateParams.InitialBalance = 50_00_000_000);
        fx.Owner = await TestAccount.CreateAsync(networkCredentials, fxO => fxO.CreateParams.InitialBalance = 50_00_000_000);
        fx.TestToken = await TestToken.CreateAsync(networkCredentials, fxT => fxT.Params.GrantKycEndorsement = null, fx.Owner);
        fx.TestAsset = await TestAsset.CreateAsync(networkCredentials, fxA => fxA.Params.GrantKycEndorsement = null, fx.Owner);
        await fx.Owner.Client.TransferAsync(new TransferParams
        {
            TokenTransfers = new[] {
                new TokenTransfer(fx.TestToken.Record.Token, fx.TestToken.TreasuryAccount, -(long)fx.TestToken.Params.Circulation),
                new TokenTransfer(fx.TestToken.Record.Token, fx.Owner, (long)fx.TestToken.Params.Circulation)
            },
            AssetTransfers = fx.TestAsset.MintRecord.SerialNumbers.Select(s => new AssetTransfer(new Asset(fx.TestAsset.Record.Token, s), fx.TestAsset.TreasuryAccount, fx.Owner)),
            Signatory = new Signatory(fx.TestToken.TreasuryAccount.PrivateKey, fx.TestAsset.TreasuryAccount.PrivateKey)
        });
        fx.AssociateRecord = await fx.Owner.Client.AllocateWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fx.Owner, fx.Agent, (long)fx.Owner.CreateParams.InitialBalance) },
            TokenAllowances = new[] { new TokenAllowance(fx.TestToken.Record.Token, fx.Owner, fx.Agent, (long)fx.TestToken.Params.Circulation) },
            AssetAllowances = new[] { new AssetAllowance(fx.TestAsset.Record.Token, fx.Owner, fx.Agent) },
            Signatory = fx.Owner.PrivateKey
        });
        fx.DelegationRecord = await fx.Owner.Client.AllocateWithRecordAsync(new AllowanceParams
        {
            AssetAllowances = new[] { new AssetAllowance(new Asset(fx.TestAsset.Record.Token, 1), fx.Owner, fx.DelegatedAgent, fx.Agent) },
            Signatory = fx.Agent.PrivateKey
        });
        fx.Client = fx.Owner.Client;
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Allowance Instance");
        return fx;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Test Allowance Instance");
        await Agent.DisposeAsync();
        await Owner.DisposeAsync();
        await TestToken.DisposeAsync();
        await TestAsset.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED: Test Account Instance");
    }
}