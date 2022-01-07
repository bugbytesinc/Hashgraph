using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Retrieves the details regarding a file stored on the network.
    /// </summary>
    /// <param name="file">
    /// Address of the file to query.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The details of the network file, excluding content.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    public async Task<FileInfo> GetFileInfoAsync(Address file, Action<IContext>? configure = null)
    {
        return new FileInfo(await ExecuteQueryAsync(new FileGetInfoQuery(file), configure).ConfigureAwait(false));
    }
}