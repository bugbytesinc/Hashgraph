using Hashgraph.Extensions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hashgraph.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Hashgraph services with the DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hederaNetwork"> passing null will result in using the mainnet</param>
    public static void AddConsensusServices(this IServiceCollection services, HederaNetwork hederaNetwork = HederaNetwork.Testnet)
    {
        services.AddScoped<IMirrorGrpcClient>((_) => 
            new MirrorGrpcClient(ctx =>
            {
                ctx.Uri = new Uri(GetMirrorNodeGrpcUrl(hederaNetwork));
            })
        );
        services.AddScoped<IMirrorRestClient>((_) => 
            new MirrorRestClient(GetMirrorRestUrl(hederaNetwork))
        );
        services.AddScoped<IConsensusService, ConsensusService>();
    }
    
    private static string GetMirrorRestUrl(HederaNetwork hederaNetwork) =>
        hederaNetwork switch
        {
            HederaNetwork.Testnet => "https://testnet.mirrornode.hedera.com/api/v1",
            HederaNetwork.Previewnet => "https://previewnet.mirrornode.hedera.com/api/v1",
            _ => "https://mainnet.mirrornode.hedera.com/api/v1",
        };
    
    private static string GetMirrorNodeGrpcUrl(HederaNetwork hederaNetwork) => 
        hederaNetwork switch
        {
            HederaNetwork.Testnet => "testnet.mirrornode.hedera.com:5600",
            HederaNetwork.Previewnet => "previewnet.mirrornode.hedera.com:5600",
            _ => "mainnet.mirrornode.hedera.com:5600"
        };
}