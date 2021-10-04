#pragma warning disable CS0612
using Hashgraph;
using System;

namespace Proto
{
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
                    _ => null
                };
            }
            set
            {
                switch (responseCase_)
                {
                    case ResponseOneofCase.GetByKey:
                        ((GetByKeyResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.GetBySolidityID:
                        ((GetBySolidityIDResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.ContractCallLocal:
                        ((ContractCallLocalResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.ContractGetBytecodeResponse:
                        ((ContractGetBytecodeResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.ContractGetInfo:
                        ((ContractGetInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.ContractGetRecordsResponse:
                        ((ContractGetRecordsResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.CryptogetAccountBalance:
                        ((CryptoGetAccountBalanceResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.CryptoGetAccountRecords:
                        ((CryptoGetAccountRecordsResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.CryptoGetInfo:
                        ((CryptoGetInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.CryptoGetLiveHash:
                        ((CryptoGetLiveHashResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.CryptoGetProxyStakers:
                        ((CryptoGetStakersResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.FileGetContents:
                        ((FileGetContentsResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.FileGetInfo:
                        ((FileGetInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TransactionGetReceipt:
                        ((TransactionGetReceiptResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TransactionGetRecord:
                        ((TransactionGetRecordResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TransactionGetFastRecord:
                        ((TransactionGetFastRecordResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.ConsensusGetTopicInfo:
                        ((ConsensusGetTopicInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.NetworkGetVersionInfo:
                        ((NetworkGetVersionInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TokenGetInfo:
                        ((TokenGetInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TokenGetAccountNftInfos:
                        ((TokenGetAccountNftInfosResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TokenGetNftInfo:
                        ((TokenGetNftInfoResponse)response_).Header = value;
                        break;
                    case ResponseOneofCase.TokenGetNftInfos:
                        ((TokenGetNftInfosResponse)response_).Header = value;
                        break;
                    default:
                        throw new InvalidOperationException("Query has No Type Set, unable to set Query Header of Unknown Query Type.");
                };
            }
        }
        internal void ValidateQueryResponse(TransactionID transactionId)
        {
            switch (responseCase_)
            {
                case ResponseOneofCase.GetByKey:
                    defaultValidate(transactionId, ((GetByKeyResponse)response_).Header);
                    break;
                case ResponseOneofCase.GetBySolidityID:
                    defaultValidate(transactionId, ((GetBySolidityIDResponse)response_).Header);
                    break;
                case ResponseOneofCase.ContractCallLocal:
                    // NOOP - Calling code has a special Case for "ThrowOnFail"
                    break;
                case ResponseOneofCase.ContractGetBytecodeResponse:
                    defaultValidate(transactionId, ((ContractGetBytecodeResponse)response_).Header);
                    break;
                case ResponseOneofCase.ContractGetInfo:
                    defaultValidate(transactionId, ((ContractGetInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.ContractGetRecordsResponse:
                    defaultValidate(transactionId, ((ContractGetRecordsResponse)response_).Header);
                    break;
                case ResponseOneofCase.CryptogetAccountBalance:
                    defaultValidate(transactionId, ((CryptoGetAccountBalanceResponse)response_).Header);
                    break;
                case ResponseOneofCase.CryptoGetAccountRecords:
                    cryptoGetAccountRecordsValidate(transactionId, ((CryptoGetAccountRecordsResponse)response_).Header);
                    break;
                case ResponseOneofCase.CryptoGetInfo:
                    defaultValidate(transactionId, ((CryptoGetInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.CryptoGetLiveHash:
                    defaultValidate(transactionId, ((CryptoGetLiveHashResponse)response_).Header);
                    break;
                case ResponseOneofCase.CryptoGetProxyStakers:
                    defaultValidate(transactionId, ((CryptoGetStakersResponse)response_).Header);
                    break;
                case ResponseOneofCase.FileGetContents:
                    defaultValidate(transactionId, ((FileGetContentsResponse)response_).Header);
                    break;
                case ResponseOneofCase.FileGetInfo:
                    defaultValidate(transactionId, ((FileGetInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.TransactionGetReceipt:
                    defaultValidate(transactionId, ((TransactionGetReceiptResponse)response_).Header);
                    break;
                case ResponseOneofCase.TransactionGetRecord:
                    // NOOP - Calling code has a special Case for "includeDuplicates"
                    break;
                case ResponseOneofCase.TransactionGetFastRecord:
                    defaultValidate(transactionId, ((TransactionGetFastRecordResponse)response_).Header);
                    break;
                case ResponseOneofCase.ConsensusGetTopicInfo:
                    defaultValidate(transactionId, ((ConsensusGetTopicInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.NetworkGetVersionInfo:
                    defaultValidate(transactionId, ((NetworkGetVersionInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.TokenGetInfo:
                    defaultValidate(transactionId, ((TokenGetInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.ScheduleGetInfo:
                    defaultValidate(transactionId, ((ScheduleGetInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.TokenGetAccountNftInfos:
                    defaultValidate(transactionId, ((TokenGetAccountNftInfosResponse)response_).Header);
                    break;
                case ResponseOneofCase.TokenGetNftInfo:
                    defaultValidate(transactionId, ((TokenGetNftInfoResponse)response_).Header);
                    break;
                case ResponseOneofCase.TokenGetNftInfos:
                    defaultValidate(transactionId, ((TokenGetNftInfosResponse)response_).Header);
                    break;
                default:
                    throw new InvalidOperationException("Query has No Type Set, unable to set Query Header of Unknown Query Type.");
            };
        }
        private static void defaultValidate(TransactionID transactionId, ResponseHeader? header)
        {
            if (header == null)
            {
                throw new PrecheckException($"Query Failed to Produce a Response.", transactionId.AsTxId(), ResponseCode.Unknown, 0);
            }
            if (header.NodeTransactionPrecheckCode == ResponseCodeEnum.InsufficientTxFee)
            {
                throw new PrecheckException($"Query Failed because the network changed the published price of the Query before the paying transaction could be signed and submitted: {header.NodeTransactionPrecheckCode}", transactionId.AsTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
            }
            if (header.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
            {
                throw new PrecheckException($"Query Transaction Failed Pre-Check: {header.NodeTransactionPrecheckCode}", transactionId.AsTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
            }
        }
        private static void cryptoGetAccountRecordsValidate(TransactionID transactionId, ResponseHeader? header)
        {
            var precheckCode = header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            if (precheckCode != ResponseCodeEnum.Ok)
            {
                throw new TransactionException("Unable to retrieve transaction records.", transactionId.AsTxId(), (ResponseCode)precheckCode);
            }
        }
    }
}
