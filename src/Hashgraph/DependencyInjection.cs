using System;
using Hashgraph;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddHashgraph(this IServiceCollection services, HederaNetwork hederaNetwork)
    {
        services.AddHttpClient<IMirrorRestClient, MirrorRestClient>(
            client =>
            {
                client.BaseAddress = new Uri(GetMirrorRestUrl(hederaNetwork));
            }
        );

        services.AddScoped<IMirrorGrpcClient>((_) => new MirrorGrpcClient(ctx =>
        {
            ctx.Uri = new Uri(GetMirrorNodeGrpcUrl(hederaNetwork));
            // ctx.OnSendingRequest = OutputSendingRequest; //TODO: Need to think about this
        }));
    }

    //TODO: double check the urls
    private static string GetMirrorRestUrl(HederaNetwork hederaNetwork) =>
        hederaNetwork switch
        {
            HederaNetwork.Testnet => "https://testnet.mirrornode.hedera.com",
            HederaNetwork.Previewnet => "https://previewnet.mirrornode.hedera.com",
            _ => "https://mainnet.mirrornode.hedera.com",
        };

    private static string GetMirrorNodeGrpcUrl(HederaNetwork hederaNetwork) => 
        hederaNetwork switch
        {
            HederaNetwork.Testnet => "testnet.mirrornode.hedera.com:5600",
            HederaNetwork.Previewnet => "previewnet.mirrornode.hedera.com:5600",
            _ => "mainnet.mirrornode.hedera.com:5600"
        };
}

//TODO: Move to a different file
public enum HederaNetwork
{
    Mainnet,
    Testnet,
    Previewnet
}