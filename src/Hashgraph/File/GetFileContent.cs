using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the contents of a file from the network.
        /// </summary>
        /// <param name="file">
        /// The address of the file contents to retrieve.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The contents of the file as a blob of bytes.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<ReadOnlyMemory<byte>> GetFileContentAsync(Address file, Action<IContext>? configure = null)
        {
            file = RequireInputParameter.File(file);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                FileGetContents = new FileGetContentsQuery
                {
                    FileID = new FileID(file)
                }
            };
            var response = await query.SignAndExecuteWithRetryAsync(context);
            return new ReadOnlyMemory<byte>(response.FileGetContents.FileContents.Contents.ToByteArray());
        }
    }
}
