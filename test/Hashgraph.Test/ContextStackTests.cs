﻿using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Tests
{
    public class ContextStackTests
    {
        [Fact(DisplayName = "Context Tests: Can Set and Reset Gateway Property")]
        public async Task CanSetAndUnsetGateway()
        {
            var gateway1 = new Gateway("Gateway1", 0, 0, Generator.Integer(3, 100));
            var gateway2 = new Gateway("Gateway2", 0, 0, Generator.Integer(3, 100));
            await using var client1 = new Client(context =>
            {
                Assert.Null(context.Gateway);
                context.Gateway = gateway1;
                Assert.Equal(gateway1, context.Gateway);
            });
            await using var client2 = client1.Clone(context =>
            {
                Assert.Equal(gateway1, context.Gateway);
                context.Gateway = gateway2;
                Assert.Equal(gateway2, context.Gateway);
            });
            client2.Configure(context =>
            {
                Assert.Equal(gateway2, context.Gateway);
                context.Reset(nameof(IContext.Gateway));
                Assert.Equal(gateway1, context.Gateway);
            });
            client1.Configure(context =>
            {
                Assert.Equal(gateway1, context.Gateway);
                context.Reset(nameof(context.Gateway));
                Assert.Null(context.Gateway);
            });
            client2.Configure(context =>
            {
                Assert.Null(context.Gateway);
            });
            client1.Configure(context =>
            {
                Assert.Null(context.Gateway);
                context.Gateway = gateway2;
                Assert.Equal(gateway2, context.Gateway);
            });
            client2.Configure(context =>
            {
                Assert.Equal(gateway2, context.Gateway);
                context.Gateway = null;
                Assert.Null(context.Gateway);
            });
            client1.Configure(context =>
            {
                Assert.Equal(gateway2, context.Gateway);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Payer Property")]
        public async Task CanSetAndUnsetPayer()
        {
            var account1 = new Account(0, 0, Generator.Integer(3, 100), Generator.KeyPair().privateKey);
            var account2 = new Account(0, 0, Generator.Integer(101, 200), Generator.KeyPair().privateKey);
            await using var client1 = new Client(context =>
            {
                Assert.Null(context.Payer);
                context.Payer = account1;
                Assert.Equal(account1, context.Payer);
            });
            await using var client2 = client1.Clone(context =>
            {
                Assert.Equal(account1, context.Payer);
                context.Payer = account2;
                Assert.Equal(account2, context.Payer);
            });
            client2.Configure(context =>
            {
                Assert.Equal(account2, context.Payer);
                context.Reset(nameof(IContext.Payer));
                Assert.Equal(account1, context.Payer);
            });
            client1.Configure(context =>
            {
                Assert.Equal(account1, context.Payer);
                context.Reset(nameof(context.Payer));
                Assert.Null(context.Payer);
            });
            client2.Configure(context =>
            {
                Assert.Null(context.Payer);
            });
            client1.Configure(context =>
            {
                Assert.Null(context.Payer);
                context.Payer = account2;
                Assert.Equal(account2, context.Payer);
            });
            client2.Configure(context =>
            {
                Assert.Equal(account2, context.Payer);
                context.Payer = null;
                Assert.Null(context.Payer);
            });
            client1.Configure(context =>
            {
                Assert.Equal(account2, context.Payer);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset FeeLimit Property")]
        public async Task CanSetAndUnsetFeeLimit()
        {
            var defaultValue = 0L;
            var newValue = Generator.Integer(3, 100);
            await using var client = new Client(context =>
            {
                defaultValue = context.FeeLimit;
                context.FeeLimit = newValue;
                Assert.Equal(newValue, context.FeeLimit);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.FeeLimit);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.FeeLimit);
                context.Reset(nameof(IContext.FeeLimit));
                Assert.Equal(defaultValue,context.FeeLimit);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.FeeLimit);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Transaction Duration Property")]
        public async Task CanSetAndUnsetTransactionDuration()
        {
            var defaultValue = TimeSpan.Zero;
            var newValue = TimeSpan.FromSeconds(Generator.Integer(200, 300));
            await using var client = new Client(context =>
            {
                defaultValue = context.TransactionDuration;
                context.TransactionDuration = newValue;
                Assert.Equal(newValue, context.TransactionDuration);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.TransactionDuration);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.TransactionDuration);
                context.Reset(nameof(IContext.TransactionDuration));
                Assert.Equal(defaultValue, context.TransactionDuration);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.TransactionDuration);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Create Account Create Record Send Threshold Property")]
        public async Task CanSetAndUnsetCreateRecordSendThreshold()
        {
            var newValue = 0UL;
            var defaultValue = (ulong)Generator.Integer(5000, 6000);
            await using var client = new Client(context =>
            {
                newValue = context.CreateAccountCreateRecordSendThreshold;
                context.CreateAccountCreateRecordSendThreshold = defaultValue;
                Assert.Equal(defaultValue, context.CreateAccountCreateRecordSendThreshold);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(defaultValue, context.CreateAccountCreateRecordSendThreshold);
            });
            client.Configure(context =>
            {
                Assert.Equal(defaultValue, context.CreateAccountCreateRecordSendThreshold);
                context.Reset(nameof(IContext.CreateAccountCreateRecordSendThreshold));
                Assert.Equal(newValue, context.CreateAccountCreateRecordSendThreshold);
            });
            clone.Configure(context =>
            {
                Assert.Equal(newValue, context.CreateAccountCreateRecordSendThreshold);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Create Account Require Signature Receive Threshold Property")]
        public async Task CanSetAndUnsetRequireSignatureReceiveThreshold()
        {
            var defaultValue = 0UL;
            var newValue = (ulong)Generator.Integer(5000, 6000);
            await using var client = new Client(context =>
            {
                defaultValue = context.CreateAcountRequireSignatureReceiveThreshold;
                context.CreateAcountRequireSignatureReceiveThreshold = newValue;
                Assert.Equal(newValue, context.CreateAcountRequireSignatureReceiveThreshold);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.CreateAcountRequireSignatureReceiveThreshold);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.CreateAcountRequireSignatureReceiveThreshold);
                context.Reset(nameof(IContext.CreateAcountRequireSignatureReceiveThreshold));
                Assert.Equal(defaultValue, context.CreateAcountRequireSignatureReceiveThreshold);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.CreateAcountRequireSignatureReceiveThreshold);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Always Require Receive Signature Property")]
        public async Task CanSetAndUnsetCreateAcountRequireSignatureReceiveThreshold()
        {
            var defaultValue = false;
            var newValue = false;
            await using var client = new Client(context =>
            {
                defaultValue = context.CreateAccountAlwaysRequireReceiveSignature;
                newValue = !defaultValue;
                context.CreateAccountAlwaysRequireReceiveSignature = newValue;
                Assert.Equal(newValue, context.CreateAccountAlwaysRequireReceiveSignature);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.CreateAccountAlwaysRequireReceiveSignature);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.CreateAccountAlwaysRequireReceiveSignature);
                context.Reset(nameof(IContext.CreateAccountAlwaysRequireReceiveSignature));
                Assert.Equal(defaultValue, context.CreateAccountAlwaysRequireReceiveSignature);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.CreateAccountAlwaysRequireReceiveSignature);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Create Account Auto Renew Period Property")]
        public async Task CanSetAndUnsetCreateAccountAutoRenewPeriod()
        {
            var defaultValue = TimeSpan.Zero;
            var newValue = TimeSpan.FromDays(Generator.Integer(200, 300));
            await using var client = new Client(context =>
            {
                defaultValue = context.CreateAccountAutoRenewPeriod;
                context.CreateAccountAutoRenewPeriod = newValue;
                Assert.Equal(newValue, context.CreateAccountAutoRenewPeriod);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.CreateAccountAutoRenewPeriod);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.CreateAccountAutoRenewPeriod);
                context.Reset(nameof(IContext.CreateAccountAutoRenewPeriod));
                Assert.Equal(defaultValue, context.CreateAccountAutoRenewPeriod);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.CreateAccountAutoRenewPeriod);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Default Memo Property")]
        public async Task CanSetAndUnsetMemo()
        {
            var newValue = Generator.String(10, 50);
            await using var client = new Client(context =>
            {
                Assert.Null(context.Memo);
                context.Memo = newValue;
                Assert.Equal(newValue, context.Memo);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.Memo);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.Memo);
                context.Reset(nameof(IContext.Memo));
                Assert.Null(context.Memo);
            });
            clone.Configure(context =>
            {
                Assert.Null(context.Memo);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Generate Record Property")]
        public async Task CanSetAndUnsetGenerateRecord()
        {
            var defaultValue = false;
            var newValue = false;
            await using var client = new Client(context =>
            {
                defaultValue = context.GenerateRecord;
                newValue = !defaultValue;
                context.GenerateRecord = newValue;
                Assert.Equal(newValue, context.GenerateRecord);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.GenerateRecord);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.GenerateRecord);
                context.Reset(nameof(IContext.GenerateRecord));
                Assert.Equal(defaultValue, context.GenerateRecord);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.GenerateRecord);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Retry Count Property")]
        public async Task CanSetAndUnsetRetryCount()
        {
            var defaultValue = 0;
            var newValue = Generator.Integer(5000, 6000);
            await using var client = new Client(context =>
            {
                defaultValue = context.RetryCount;
                context.RetryCount = newValue;
                Assert.Equal(newValue, context.RetryCount);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.RetryCount);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.RetryCount);
                context.Reset(nameof(IContext.RetryCount));
                Assert.Equal(defaultValue, context.RetryCount);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.RetryCount);
            });
        }
        [Fact(DisplayName = "Context Tests: Can Set and Reset Retry Delay Property")]
        public async Task CanSetAndUnsetRetryDelay()
        {
            var defaultValue = TimeSpan.Zero;
            var newValue = TimeSpan.FromMinutes(Generator.Integer(200, 300));
            await using var client = new Client(context =>
            {
                defaultValue = context.RetryDelay;
                context.RetryDelay = newValue;
                Assert.Equal(newValue, context.RetryDelay);
            });
            await using var clone = client.Clone(context =>
            {
                Assert.Equal(newValue, context.RetryDelay);
            });
            client.Configure(context =>
            {
                Assert.Equal(newValue, context.RetryDelay);
                context.Reset(nameof(IContext.RetryDelay));
                Assert.Equal(defaultValue, context.RetryDelay);
            });
            clone.Configure(context =>
            {
                Assert.Equal(defaultValue, context.RetryDelay);
            });
        }
    }
}