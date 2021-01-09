using System;

namespace Proto
{
    public sealed partial class Query
    {
        public QueryHeader? QueryHeader
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
    }
}
