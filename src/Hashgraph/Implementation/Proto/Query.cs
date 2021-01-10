#pragma warning disable CS0612
using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Proto
{
    public sealed partial class Query
    {
        internal QueryHeader? QueryHeader
        {
            get
            {
                return QueryCase switch
                {
                    QueryOneofCase.GetByKey => (query_ as GetByKeyQuery)?.Header,
                    QueryOneofCase.GetBySolidityID => (query_ as GetBySolidityIDQuery)?.Header,
                    QueryOneofCase.ContractCallLocal => (query_ as ContractCallLocalQuery)?.Header,
                    QueryOneofCase.ContractGetInfo => (query_ as ContractGetInfoQuery)?.Header,
                    QueryOneofCase.ContractGetBytecode => (query_ as ContractGetBytecodeQuery)?.Header,
                    QueryOneofCase.ContractGetRecords => (query_ as ContractGetRecordsQuery)?.Header,
                    QueryOneofCase.CryptogetAccountBalance => (query_ as CryptoGetAccountBalanceQuery)?.Header,
                    QueryOneofCase.CryptoGetAccountRecords => (query_ as CryptoGetAccountRecordsQuery)?.Header,
                    QueryOneofCase.CryptoGetInfo => (query_ as CryptoGetInfoQuery)?.Header,
                    QueryOneofCase.CryptoGetLiveHash => (query_ as CryptoGetLiveHashQuery)?.Header,
                    QueryOneofCase.CryptoGetProxyStakers => (query_ as CryptoGetStakersQuery)?.Header,
                    QueryOneofCase.FileGetContents => (query_ as FileGetContentsQuery)?.Header,
                    QueryOneofCase.FileGetInfo => (query_ as FileGetInfoQuery)?.Header,
                    QueryOneofCase.TransactionGetReceipt => (query_ as TransactionGetReceiptQuery)?.Header,
                    QueryOneofCase.TransactionGetRecord => (query_ as TransactionGetRecordQuery)?.Header,
                    QueryOneofCase.TransactionGetFastRecord => (query_ as TransactionGetFastRecordQuery)?.Header,
                    QueryOneofCase.ConsensusGetTopicInfo => (query_ as ConsensusGetTopicInfoQuery)?.Header,
                    QueryOneofCase.NetworkGetVersionInfo => (query_ as NetworkGetVersionInfoQuery)?.Header,
                    QueryOneofCase.TokenGetInfo => (query_ as TokenGetInfoQuery)?.Header,
                    _ => null
                };
            }
            set
            {
                switch (queryCase_)
                {
                    case QueryOneofCase.GetByKey:
                        ((GetByKeyQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.GetBySolidityID:
                        ((GetBySolidityIDQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.ContractCallLocal:
                        ((ContractCallLocalQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.ContractGetInfo:
                        ((ContractGetInfoQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.ContractGetBytecode:
                        ((ContractGetBytecodeQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.ContractGetRecords:
                        ((ContractGetRecordsQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.CryptogetAccountBalance:
                        ((CryptoGetAccountBalanceQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.CryptoGetAccountRecords:
                        ((CryptoGetAccountRecordsQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.CryptoGetInfo:
                        ((CryptoGetInfoQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.CryptoGetLiveHash:
                        ((CryptoGetLiveHashQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.CryptoGetProxyStakers:
                        ((CryptoGetStakersQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.FileGetContents:
                        ((FileGetContentsQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.FileGetInfo:
                        ((FileGetInfoQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.TransactionGetReceipt:
                        ((TransactionGetReceiptQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.TransactionGetRecord:
                        ((TransactionGetRecordQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.TransactionGetFastRecord:
                        ((TransactionGetFastRecordQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.ConsensusGetTopicInfo:
                        ((ConsensusGetTopicInfoQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.NetworkGetVersionInfo:
                        ((NetworkGetVersionInfoQuery)query_).Header = value;
                        break;
                    case QueryOneofCase.TokenGetInfo:
                        ((TokenGetInfoQuery)query_).Header = value;
                        break;
                    default:
                        throw new InvalidOperationException("Query has No Type Set, unable to set Query Header of Unknown Query Type.");
                };
            }
        }

        internal Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> InstantiateNetworkRequestMethod(Channel channel)
        {
            return QueryCase switch
            {
                //QueryOneofCase.GetByKey => (query_ as GetByKeyQuery)?.Header,
                //QueryOneofCase.GetBySolidityID => (query_ as GetBySolidityIDQuery)?.Header,
                QueryOneofCase.ContractCallLocal => new SmartContractService.SmartContractServiceClient(channel).contractCallLocalMethodAsync,
                QueryOneofCase.ContractGetInfo => new SmartContractService.SmartContractServiceClient(channel).getContractInfoAsync,
                QueryOneofCase.ContractGetBytecode => new SmartContractService.SmartContractServiceClient(channel).ContractGetBytecodeAsync,
                QueryOneofCase.ContractGetRecords => new SmartContractService.SmartContractServiceClient(channel).getTxRecordByContractIDAsync,
                QueryOneofCase.CryptogetAccountBalance => new CryptoService.CryptoServiceClient(channel).cryptoGetBalanceAsync,
                QueryOneofCase.CryptoGetAccountRecords => new CryptoService.CryptoServiceClient(channel).getAccountRecordsAsync,
                QueryOneofCase.CryptoGetInfo => new CryptoService.CryptoServiceClient(channel).getAccountInfoAsync,
                //QueryOneofCase.CryptoGetLiveHash => (query_ as CryptoGetLiveHashQuery)?.Header,
                QueryOneofCase.CryptoGetProxyStakers => new CryptoService.CryptoServiceClient(channel).getStakersByAccountIDAsync,
                QueryOneofCase.FileGetContents => new FileService.FileServiceClient(channel).getFileContentAsync,
                QueryOneofCase.FileGetInfo => new FileService.FileServiceClient(channel).getFileInfoAsync,
                QueryOneofCase.TransactionGetReceipt => new CryptoService.CryptoServiceClient(channel).getTransactionReceiptsAsync,
                QueryOneofCase.TransactionGetRecord => new CryptoService.CryptoServiceClient(channel).getTxRecordByTxIDAsync,
                //QueryOneofCase.TransactionGetFastRecord => (query_ as TransactionGetFastRecordQuery)?.Header,
                QueryOneofCase.ConsensusGetTopicInfo => new ConsensusService.ConsensusServiceClient(channel).getTopicInfoAsync,
                QueryOneofCase.NetworkGetVersionInfo => new NetworkService.NetworkServiceClient(channel).getVersionInfoAsync,
                QueryOneofCase.TokenGetInfo => new TokenService.TokenServiceClient(channel).getTokenInfoAsync,
                _ => throw new InvalidOperationException("Query has No Type Set, unable to find Network Request Method for Unknown Query Type.")
            };
        }

        internal async Task<Response> SignAndExecuteWithRetryAsync(GossipContextStack context, long supplementalCost = 0)
        {
            QueryHeader = new QueryHeader
            {
                Payment = new Transaction { SignedTransactionBytes = ByteString.Empty },
                ResponseType = ResponseType.CostAnswer
            };
            var response = await executeUnsignedAskRequestWithRetryAsync();
            ulong cost = response.ResponseHeader?.Cost ?? 0UL;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                QueryHeader = await createSignedQueryHeader((long)cost + supplementalCost, transactionId);
                response = await executeSignedQueryWithRetryAsync();
                response.Validate(transactionId);
            }
            return response;

            async Task<QueryHeader> createSignedQueryHeader(long queryFee, TransactionID transactionId)
            {
                var gateway = RequireInContext.Gateway(context);
                var payer = RequireInContext.Payer(context);
                var signatory = RequireInContext.Signatory(context);
                var fee = RequireInContext.QueryFee(context, queryFee);
                TransactionBody transactionBody = new TransactionBody
                {
                    TransactionID = transactionId,
                    NodeAccountID = new AccountID(gateway),
                    TransactionFee = (ulong)context.FeeLimit,
                    TransactionValidDuration = new Proto.Duration(context.TransactionDuration),
                    Memo = context.Memo ?? ""
                };
                var transfers = new TransferList();
                transfers.AccountAmounts.Add(new AccountAmount { AccountID = new AccountID(payer), Amount = -fee });
                transfers.AccountAmounts.Add(new AccountAmount { AccountID = new AccountID(gateway), Amount = fee });
                transactionBody.CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers };
                return new QueryHeader
                {
                    Payment = await transactionBody.SignAsync(signatory, context.SignaturePrefixTrimLimit)
                };
            }

            async Task<Response> executeUnsignedAskRequestWithRetryAsync()
            {
                var answer = await Transactions.ExecuteNetworkRequestWithRetryAsync(context, this, InstantiateNetworkRequestMethod, shouldRetryRequest);
                var code = answer.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                if (code != ResponseCodeEnum.Ok)
                {
                    throw new PrecheckException($"Transaction Failed Pre-Check: {code}", new TxId(), (ResponseCode)code, 0);
                }
                return answer;

                static bool shouldRetryRequest(Response response)
                {
                    return ResponseCodeEnum.Busy == response.ResponseHeader?.NodeTransactionPrecheckCode;
                }
            }

            Task<Response> executeSignedQueryWithRetryAsync()
            {
                return Transactions.ExecuteSignedRequestWithRetryImplementationAsync(context, this, InstantiateNetworkRequestMethod, getResponseCode);

                ResponseCodeEnum getResponseCode(Response response)
                {
                    return response.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                }
            }
        }
    }
}
