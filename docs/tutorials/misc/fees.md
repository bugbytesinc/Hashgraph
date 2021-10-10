---
title: Get Fee Schedule
layout: default
---

# Get Fee Schedule

Queries the network for current and 'next' fee schedule.

```csharp
class Program
{
    static async Task Main(string[] args)
    {                                                 // For Example:
        var gatewayUrl = args[0];                     //   2.testnet.hedera.com:50211
        var gatewayAccountNo = long.Parse(args[1]);   //   5 (gateway node 0.0.5)
        var payerAccountNo = long.Parse(args[2]);     //   20 (account 0.0.20)
        var payerPrivateKey = Hex.ToBytes(args[3]);   //   302e0201... (Ed25519 private in hex)
        try
        {
            await using var client = new Client(ctx =>
            {
                ctx.Gateway = new Gateway(gatewayUrl, 0, 0, gatewayAccountNo);
                ctx.Payer = new Address(0, 0, payerAccountNo);
                ctx.Signatory = new Signatory(payerPrivateKey);
            });

            var schedules = await client.GetFeeScheduleAsync();

            foreach (var schedule in schedules.Current.Data)
            {
                Console.WriteLine(schedule.Key);
                foreach (var detail in schedule.Value)
                {
                    Console.WriteLine(detail);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
```
Example output:
```
CryptoCreate
{ "nodedata": { "max": "1000000000000000", "constant": "11461413665", "bpt": "508982", "vpt": "1272455960", "rbh": "339", "sbh": "25", "gas": "3393", "bpr": "508982", "sbpr": "12725" }, "networkdata": { "max": "1000000000000000", "constant": "229228273302", "bpt": "10179648", "vpt": "25449119198", "rbh": "6786", "sbh": "509", "gas": "67864", "bpr": "10179648", "sbpr": "254491" }, "servicedata": { "max": "1000000000000000", "constant": "229228273302", "bpt": "10179648", "vpt": "25449119198", "rbh": "6786", "sbh": "509", "gas": "67864", "bpr": "10179648", "sbpr": "254491" } }
CryptoAccountAutoRenew
{ "nodedata": { "max": "1000000000000000", "constant": "59972622", "bpt": "95878", "vpt": "239695625", "rbh": "64", "sbh": "5", "gas": "639", "bpr": "95878", "sbpr": "2397" }, "networkdata": { "max": "1000000000000000", "constant": "959561945", "bpt": "1534052", "vpt": "3835129999", "rbh": "1023", "sbh": "77", "gas": "10227", "bpr": "1534052", "sbpr": "38351" }, "servicedata": { "max": "1000000000000000", "constant": "959561945", "bpt": "1534052", "vpt": "3835129999", "rbh": "1023", "sbh": "77", "gas": "10227", "bpr": "1534052", "sbpr": "38351" } }
CryptoUpdate
{ "nodedata": { "max": "1000000000000000", "constant": "16302371", "bpt": "26063", "vpt": "65156516", "rbh": "17", "sbh": "1", "gas": "174", "bpr": "26063", "sbpr": "652" }, "networkdata": { "max": "1000000000000000", "constant": "326047427", "bpt": "521252", "vpt": "1303130322", "rbh": "348", "sbh": "26", "gas": "3475", "bpr": "521252", "sbpr": "13031" }, "servicedata": { "max": "1000000000000000", "constant": "326047427", "bpt": "521252", "vpt": "1303130322", "rbh": "348", "sbh": "26", "gas": "3475", "bpr": "521252", "sbpr": "13031" } }
CryptoTransfer
{ "nodedata": { "max": "1000000000000000", "constant": "7574478", "bpt": "12109", "vpt": "30273301", "rbh": "8", "sbh": "1", "gas": "81", "bpr": "12109", "sbpr": "303" }, "networkdata": { "max": "1000000000000000", "constant": "151489557", "bpt": "242186", "vpt": "605466012", "rbh": "161", "sbh": "12", "gas": "1615", "bpr": "242186", "sbpr": "6055" }, "servicedata": { "max": "1000000000000000", "constant": "151489557", "bpt": "242186", "vpt": "605466012", "rbh": "161", "sbh": "12", "gas": "1615", "bpr": "242186", "sbpr": "6055" } }
{ "nodedata": { "max": "1000000000000000", "constant": "7983519", "bpt": "12763", "vpt": "31908136", "rbh": "9", "sbh": "1", "gas": "85", "bpr": "12763", "sbpr": "319" }, "networkdata": { "max": "1000000000000000", "constant": "159670382", "bpt": "255265", "vpt": "638162730", "rbh": "170", "sbh": "13", "gas": "1702", "bpr": "255265", "sbpr": "6382" }, "servicedata": { "max": "1000000000000000", "constant": "159670382", "bpt": "255265", "vpt": "638162730", "rbh": "170", "sbh": "13", "gas": "1702", "bpr": "255265", "sbpr": "6382" }, "subType": "TOKEN_FUNGIBLE_COMMON" }
{ "nodedata": { "max": "1000000000000000", "constant": "15967037", "bpt": "25527", "vpt": "63816270", "rbh": "17", "sbh": "1", "gas": "170", "bpr": "25527", "sbpr": "638" }, "networkdata": { "max": "1000000000000000", "constant": "319340747", "bpt": "510530", "vpt": "1276325394", "rbh": "340", "sbh": "26", "gas": "3404", "bpr": "510530", "sbpr": "12763" }, "servicedata": { "max": "1000000000000000", "constant": "319340747", "bpt": "510530", "vpt": "1276325394", "rbh": "340", "sbh": "26", "gas": "3404", "bpr": "510530", "sbpr": "12763" }, "subType": "TOKEN_FUNGIBLE_COMMON_WITH_CUSTOM_FEES" }
{ "nodedata": { "max": "1000000000000000", "constant": "22833842", "bpt": "36504", "vpt": "91261178", "rbh": "24", "sbh": "2", "gas": "243", "bpr": "36504", "sbpr": "913" }, "networkdata": { "max": "1000000000000000", "constant": "456676846", "bpt": "730089", "vpt": "1825223562", "rbh": "487", "sbh": "37", "gas": "4867", "bpr": "730089", "sbpr": "18252" }, "servicedata": { "max": "1000000000000000", "constant": "456676846", "bpt": "730089", "vpt": "1825223562", "rbh": "487", "sbh": "37", "gas": "4867", "bpr": "730089", "sbpr": "18252" }, "subType": "TOKEN_NON_FUNGIBLE_UNIQUE" }
{ "nodedata": { "max": "1000000000000000", "constant": "45667678", "bpt": "73009", "vpt": "182522330", "rbh": "49", "sbh": "4", "gas": "487", "bpr": "73009", "sbpr": "1825" }, "networkdata": { "max": "1000000000000000", "constant": "913353558", "bpt": "1460179", "vpt": "3650446591", "rbh": "973", "sbh": "73", "gas": "9735", "bpr": "1460179", "sbpr": "36504" }, "servicedata": { "max": "1000000000000000", "constant": "913353558", "bpt": "1460179", "vpt": "3650446591", "rbh": "973", "sbh": "73", "gas": "9735", "bpr": "1460179", "sbpr": "36504" }, "subType": "TOKEN_NON_FUNGIBLE_UNIQUE_WITH_CUSTOM_FEES" }
...
```
