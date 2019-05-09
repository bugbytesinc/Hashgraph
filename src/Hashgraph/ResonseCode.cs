using System.ComponentModel;

namespace Hashgraph
{
    /// <summary>
    /// Pre-Check and Receipt Response Codes - 1to1 mapping with protobuf ResponseCodeEnum
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// response codes for pre check validation
        /// </summary>
        [Description("OK")] Ok = 0,
        /// <summary>
        /// For any error not handled by specific error codes listed below.
        /// </summary>
        [Description("INVALID_TRANSACTION")] InvalidTransaction = 1,
        /// <summary>
        ///Payer account does not exist.
        /// </summary>
        [Description("PAYER_ACCOUNT_NOT_FOUND")] PayerAccountNotFound = 2,
        /// <summary>
        ///Node Account provided does not match the node account of the node the transaction was submitted to.
        /// </summary>
        [Description("INVALID_NODE_ACCOUNT")] InvalidNodeAccount = 3,
        /// <summary>
        /// Pre-Check TransactionValidStart + transactionValidDuration is less than current consensus time.
        /// </summary>
        [Description("TRANSACTION_EXPIRED")] TransactionExpired = 4,
        /// <summary>
        /// Transaction start time is greater than current consensus time
        /// </summary>
        [Description("INVALID_TRANSACTION_START")] InvalidTransactionStart = 5,
        /// <summary>
        ///valid transaction duration is a positive non zero number that does not exceed 120 seconds
        /// </summary>
        [Description("INVALID_TRANSACTION_DURATION")] InvalidTransactionDuration = 6,
        /// <summary>
        ///the transaction signature is not valid
        /// </summary>
        [Description("INVALID_SIGNATURE")] InvalidSignature = 7,
        /// <summary>
        ///Transaction memo size exceeded 100 bytes
        /// </summary>
        [Description("MEMO_TOO_LONG")] MemoTooLong = 8,
        /// <summary>
        /// the transaction fee is insufficient for this type of transaction
        /// </summary>
        [Description("INSUFFICIENT_TX_FEE")] InsufficientTxFee = 9,
        /// <summary>
        /// the payer account has insufficient cryptocurrency to pay the transaction fee
        /// </summary>
        [Description("INSUFFICIENT_PAYER_BALANCE")] InsufficientPayerBalance = 10,
        /// <summary>
        /// this transaction ID is a duplicate of one that was submitted to this node or reached consensus in the last 180 seconds (receipt period)
        /// </summary>
        [Description("DUPLICATE_TRANSACTION")] DuplicateTransaction = 11,
        /// <summary>
        ///If API is throttled out
        /// </summary>
        [Description("BUSY")] Busy = 12,
        /// <summary>
        ///not supported API
        /// </summary>
        [Description("NOT_SUPPORTED")] NotSupported = 13,
        /// <summary>
        ///response codes used in queries 
        /// </summary>
        [Description("INVALID_FILE_ID")] InvalidFileId = 14,
        /// <summary>
        ///the account id is invalid or does not exist
        /// </summary>
        [Description("INVALID_ACCOUNT_ID")] InvalidAccountId = 15,
        /// <summary>
        ///the contract id is invalid or does ont exist
        /// </summary>
        [Description("INVALID_CONTRACT_ID")] InvalidContractId = 16,
        /// <summary>
        ///transaction id is not valid
        /// </summary>
        [Description("INVALID_TRANSACTION_ID")] InvalidTransactionId = 17,
        /// <summary>
        ///receipt for given transaction id does not exist
        /// </summary>
        [Description("RECEIPT_NOT_FOUND")] ReceiptNotFound = 18,
        /// <summary>
        ///record for given transaction id does not exist
        /// </summary>
        [Description("RECORD_NOT_FOUND")] RecordNotFound = 19,
        /// <summary>
        ///the solidity id is invalid or entity with this solidity id does not exist
        /// </summary>
        [Description("INVALID_SOLIDITY_ID")] InvalidSolidityId = 20,
        /// <summary>
        /// response code for Transaction receipt
        /// </summary>
        [Description("UNKNOWN")] Unknown = 21,
        /// <summary>
        /// the transaction succeeded
        /// </summary>
        [Description("SUCCESS")] Success = 22,
        /// <summary>
        /// the transaction failed because it is invalid
        /// </summary>
        [Description("FAIL_INVALID")] FailInvalid = 23,
        /// <summary>
        /// the transaction fee was insufficient
        /// </summary>
        [Description("FAIL_FEE")] FailFee = 24,
        /// <summary>
        /// the paying account had insufficient cryptocurrency
        /// </summary>
        [Description("FAIL_BALANCE")] FailBalance = 25,
        /// <summary>
        ///Crypto Response codes
        /// </summary>
        [Description("KEY_REQUIRED")] KeyRequired = 26,
        /// <summary>
        ///Unsupported algorithm/encoding used for keys in the transaction
        /// </summary>
        [Description("BAD_ENCODING")] BadEncoding = 27,
        /// <summary>
        ///When the account balance is not sufficient for the transfer
        /// </summary>
        [Description("INSUFFICIENT_ACCOUNT_BALANCE")] InsufficientAccountBalance = 28,
        /// <summary>
        ///During an update transaction when the system is not able to find the Users Solidity address
        /// </summary>
        [Description("INVALID_SOLIDITY_ADDRESS")] InvalidSolidityAddress = 29,
        /// <summary>
        ///Smart contract creation or execution
        /// </summary>
        [Description("INSUFFICIENT_GAS")] InsufficientGas = 30,
        /// <summary>
        ///contract byte code size is over the limit
        /// </summary>
        [Description("CONTRACT_SIZE_LIMIT_EXCEEDED")] ContractSizeLimitExceeded = 31,
        /// <summary>
        ///local execution (query) is requested for a function which changes state
        /// </summary>
        [Description("LOCAL_CALL_MODIFICATION_EXCEPTION")] LocalCallModificationException = 32,
        /// <summary>
        ///Contract REVERT OPCODE executed
        /// </summary>
        [Description("CONTRACT_REVERT_EXECUTED")] ContractRevertExecuted = 33,
        /// <summary>
        ///For any contract execution related error not handled by specific error codes listed above.
        /// </summary>
        [Description("CONTRACT_EXECUTION_EXCEPTION")] ContractExecutionException = 34,
        /// <summary>
        ///In Query validation, account with +ve(amount) value should be Receiving node account, the receiver account should be only one account in the list 
        /// </summary>
        [Description("INVALID_RECEIVING_NODE_ACCOUNT")] InvalidReceivingNodeAccount = 35,
        /// <summary>
        /// Header is missing in Query request
        /// </summary>
        [Description("MISSING_QUERY_HEADER")] MissingQueryHeader = 36,
        /// <summary>
        /// the update of the account failed
        /// </summary>
        [Description("ACCOUNT_UPDATE_FAILED")] AccountUpdateFailed = 37,
        [Description("INVALID_KEY_ENCODING")] InvalidKeyEncoding = 38,
        /// <summary>
        /// null solidity address
        /// </summary>
        [Description("NULL_SOLIDITY_ADDRESS")] NullSolidityAddress = 39,
        /// <summary>
        /// update of the contract failed
        /// </summary>
        [Description("CONTRACT_UPDATE_FAILED")] ContractUpdateFailed = 40,
        /// <summary>
        /// the query header is invalid
        /// </summary>
        [Description("INVALID_QUERY_HEADER")] InvalidQueryHeader = 41,
        /// <summary>
        /// Invalid fee submitted*/
        /// </summary>
        [Description("INVALID_FEE_SUBMITTED")] InvalidFeeSubmitted = 42,
        /// <summary>
        ///  payer signature is invalid
        /// </summary>
        [Description("INVALID_PAYER_SIGNATURE")] InvalidPayerSignature = 43,
        /// <summary>
        /// KEY_NOT_PROVIDED
        /// </summary>
        [Description("KEY_NOT_PROVIDED")] KeyNotProvided = 44,
        /// <summary>
        /// INVALID_EXPIRATION_TIME
        /// </summary>
        [Description("INVALID_EXPIRATION_TIME")] InvalidExpirationTime = 45,
        /// <summary>
        /// NO_WACL_KEY
        /// </summary>
        [Description("NO_WACL_KEY")] NoWaclKey = 46,
        /// <summary>
        /// FILE_CONTENT_EMPTY
        /// </summary>
        [Description("FILE_CONTENT_EMPTY")] FileContentEmpty = 47,
        /// <summary>
        /// The crypto transfer credit and debit don't equal to 0
        /// </summary>
        [Description("INVALID_ACCOUNT_AMOUNTS")] InvalidAccountAmounts = 48,
        /// <summary>
        /// new response codes to be added
        /// </summary>
        [Description("EMPTY_TRANSACTION_BODY")] EmptyTransactionBody = 49,
        /// <summary>
        /// invalid transaction body
        /// </summary>
        [Description("INVALID_TRANSACTION_BODY")] InvalidTransactionBody = 50,
    }
}
