﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

public static class GetActiveGatewaysExtension
{
    /// <summary>
    /// Retrieves a list of Hedera gRPC nodes known to the 
    /// mirror node that respond to a const_ask query within 
    /// the given timeout value.  This can be used to create
    /// a list of working gRPC nodes for submitting transactions.
    /// </summary>
    /// <param name="client">
    /// The Mirror Node REST Client
    /// </param>
    /// <param name="maxTimeoutInMiliseconds">
    /// The time value threshold, that if exceeded, will result
    /// in the node not being considered active and included
    /// on this list.
    /// </param>
    /// <returns>
    /// A dictionary of gateways and the corresponding response
    /// time (in miliseconds).
    /// </returns>
    public static async Task<IReadOnlyDictionary<Gateway, long>> GetActiveGatewaysAsync(this IMirrorRestClient client, int maxTimeoutInMiliseconds)
    {
        var list = new List<Task<(Gateway gatway, long response)>>();
        await foreach (var node in client.GetGossipNodesAsync())
        {
            foreach (var endpoint in node.Endpoints)
            {
                if (endpoint.Port == 50211)
                {
                    list.Add(Task.Run(async () =>
                    {
                        var uri = new Uri($"http://{endpoint.Address}:{endpoint.Port}");
                        var gateway = new Gateway(uri, node.Account);
                        var grpClient = new Client(cfg => cfg.Gateway = gateway);
                        var response = -1L;
                        var task = grpClient.PingAsync();
                        if (await Task.WhenAny(task, Task.Delay(maxTimeoutInMiliseconds)) == task)
                        {
                            try
                            {
                                response = task.Result;
                            }
                            catch
                            {
                                // fall thru with -1
                            }
                        }
                        return (gateway, response);
                    }));
                }
            }
        }
        var result = new Dictionary<Gateway, long>();
        foreach (var (gatway, response) in await Task.WhenAll(list))
        {
            if (response > -1)
            {
                result.Add(gatway, response);
            }
        }
        return result;
    }
}