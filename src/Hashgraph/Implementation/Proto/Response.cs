#pragma warning disable CS0612 // Type or member is obsolete
namespace Proto;

public sealed partial class Response
{
    public ResponseHeader? ResponseHeader
    {
        get
        {
            return responseCase_ switch
            {
                ResponseOneofCase.GetByKey => (response_ as GetByKeyResponse)?.Header,
                ResponseOneofCase.GetBySolidityID => (response_ as GetBySolidityIDResponse)?.Header,
                ResponseOneofCase.ContractCallLocal => (response_ as ContractCallLocalResponse)?.Header,
                ResponseOneofCase.ContractGetBytecodeResponse => (response_ as ContractGetBytecodeResponse)?.Header,
                ResponseOneofCase.ContractGetInfo => (response_ as ContractGetInfoResponse)?.Header,
                ResponseOneofCase.ContractGetRecordsResponse => (response_ as ContractGetRecordsResponse)?.Header,
                ResponseOneofCase.CryptogetAccountBalance => (response_ as CryptoGetAccountBalanceResponse)?.Header,
                ResponseOneofCase.CryptoGetAccountRecords => (response_ as CryptoGetAccountRecordsResponse)?.Header,
                ResponseOneofCase.CryptoGetInfo => (response_ as CryptoGetInfoResponse)?.Header,
                ResponseOneofCase.CryptoGetLiveHash => (response_ as CryptoGetLiveHashResponse)?.Header,
                ResponseOneofCase.CryptoGetProxyStakers => (response_ as CryptoGetStakersResponse)?.Header,
                ResponseOneofCase.FileGetContents => (response_ as FileGetContentsResponse)?.Header,
                ResponseOneofCase.FileGetInfo => (response_ as FileGetInfoResponse)?.Header,
                ResponseOneofCase.TransactionGetReceipt => (response_ as TransactionGetReceiptResponse)?.Header,
                ResponseOneofCase.TransactionGetRecord => (response_ as TransactionGetRecordResponse)?.Header,
                ResponseOneofCase.TransactionGetFastRecord => (response_ as TransactionGetFastRecordResponse)?.Header,
                ResponseOneofCase.ConsensusGetTopicInfo => (response_ as ConsensusGetTopicInfoResponse)?.Header,
                ResponseOneofCase.NetworkGetVersionInfo => (response_ as NetworkGetVersionInfoResponse)?.Header,
                ResponseOneofCase.TokenGetInfo => (response_ as TokenGetInfoResponse)?.Header,
                ResponseOneofCase.ScheduleGetInfo => (response_ as ScheduleGetInfoResponse)?.Header,
                ResponseOneofCase.TokenGetAccountNftInfos => (response_ as TokenGetAccountNftInfosResponse)?.Header,
                ResponseOneofCase.TokenGetNftInfo => (response_ as TokenGetNftInfoResponse)?.Header,
                ResponseOneofCase.TokenGetNftInfos => (response_ as TokenGetNftInfosResponse)?.Header,
                ResponseOneofCase.NetworkGetExecutionTime => (response_ as NetworkGetExecutionTimeResponse)?.Header,
                _ => null
            };
        }
    }
}