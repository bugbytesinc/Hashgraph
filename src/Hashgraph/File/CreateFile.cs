using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created file.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<FileReceipt> CreateFileAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return new FileReceipt(await CreateFileImplementationAsync(createParameters, configure, false));
        }
        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created file & fees.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<FileRecord> CreateFileWithRecordAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return new FileRecord(await CreateFileImplementationAsync(createParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the Create File service.
        /// </summary>
        private async Task<NetworkResult> CreateFileImplementationAsync(CreateFileParams createParameters, Action<IContext>? configure, bool includeRecord)
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                FileCreate = new FileCreateTransactionBody
                {
                    ExpirationTime = new Timestamp(createParameters.Expiration),
                    Keys = new KeyList(createParameters.Endorsements),
                    Contents = ByteString.CopyFrom(createParameters.Contents.ToArray()),
                    Memo = createParameters.Memo ?? ""
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to create file, status: {0}", createParameters.Signatory);
        }
    }
}
