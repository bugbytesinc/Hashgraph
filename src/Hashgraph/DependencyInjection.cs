using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hashgraph;

public static class DependencyInjection
{
    public static void AddHashgraph(this IServiceCollection services, HederaNetwork hederaNetwork)
    {
        services.AddHttpClient<IMirrorRestClient, MirrorRestClient>(
            client =>
            {
                client.BaseAddress = new Uri(GetMirrorNodeRestUrl(hederaNetwork));
            }
        );

        services.AddScoped<IMirrorGrpcClient>((_) => new MirrorGrpcClient(ctx =>
        {
            ctx.Uri = new Uri(GetMirrorNodeGrpcUrl(hederaNetwork));
            // ctx.OnSendingRequest = OutputSendingRequest; //TODO: Need to think about this
        }));
    }
    
    private static string GetMirrorNodeRestUrl(HederaNetwork network) => 
        network switch
    {
        HederaNetwork.Testnet => "https://testnet.hedera.com:50002",
        HederaNetwork.Previewnet => "previewnet.hedera.com:50211",
        _ => "mainnet.hedera.com:50211"
    };
    
    private static string GetMirrorNodeGrpcUrl(HederaNetwork network) => 
        network switch
        {
            HederaNetwork.Testnet => "https://testnet.hedera.com:50002",
            HederaNetwork.Previewnet => "previewnet.hedera.com:50211",
            _ => "mainnet.hedera.com:50211"
        };
}

//TODO: Move to a different file
public enum HederaNetwork
{
    Mainnet,
    Testnet,
    Previewnet
}