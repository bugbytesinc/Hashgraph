using System.ComponentModel;

namespace Hashgraph
{
    /// <summary>
    /// Pre-Check and Receipt Response Codes - 1to1 mapping with protobuf ResponseCodeEnum
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// The transaction passed the precheck validations.
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
        /// Pre-Check error when TransactionValidStart + transactionValidDuration is less than current consensus time.
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
        /// The transaction signature is not valid
        /// </summary>
        [Description("INVALID_SIGNATURE")] InvalidSignature = 7,
        /// <summary>
        ///Transaction memo size exceeded 100 bytes
        /// </summary>
        [Description("MEMO_TOO_LONG")] MemoTooLong = 8,
        /// <summary>
        /// The fee provided in the transaction is insufficient for this type of transaction
        /// </summary>
        [Description("INSUFFICIENT_TX_FEE")] InsufficientTxFee = 9,
        /// <summary>
        /// The payer account has insufficient cryptocurrency to pay the transaction fee
        /// </summary>
        [Description("INSUFFICIENT_PAYER_BALANCE")] InsufficientPayerBalance = 10,
        /// <summary>
        /// This transaction ID is a duplicate of one that was submitted to this node or reached consensus in the last 180 seconds (receipt period)
        /// </summary>
        [Description("DUPLICATE_TRANSACTION")] DuplicateTransaction = 11,
        /// <summary>
        ///If API is throttled out
        /// </summary>
        [Description("BUSY")] Busy = 12,
        /// <summary>
        ///The API is not currently supported
        /// </summary>
        [Description("NOT_SUPPORTED")] NotSupported = 13,
        /// <summary>
        ///The file id is invalid or does not exist
        /// </summary>
        [Description("INVALID_FILE_ID")] InvalidFileId = 14,
        /// <summary>
        ///The account id is invalid or does not exist
        /// </summary>
        [Description("INVALID_ACCOUNT_ID")] InvalidAccountId = 15,
        /// <summary>
        ///The contract id is invalid or does not exist
        /// </summary>
        [Description("INVALID_CONTRACT_ID")] InvalidContractId = 16,
        /// <summary>
        ///Transaction id is not valid
        /// </summary>
        [Description("INVALID_TRANSACTION_ID")] InvalidTransactionId = 17,
        /// <summary>
        ///Receipt for given transaction id does not exist
        /// </summary>
        [Description("RECEIPT_NOT_FOUND")] ReceiptNotFound = 18,
        /// <summary>
        ///Record for given transaction id does not exist
        /// </summary>
        [Description("RECORD_NOT_FOUND")] RecordNotFound = 19,
        /// <summary>
        ///The solidity id is invalid or entity with this solidity id does not exist
        /// </summary>
        [Description("INVALID_SOLIDITY_ID")] InvalidSolidityId = 20,
        /// <summary>
        /// Transaction hasn't yet reached consensus, or has already expired
        /// </summary>
        [Description("UNKNOWN")] Unknown = 21,
        /// <summary>
        /// The transaction succeeded
        /// </summary>
        [Description("SUCCESS")] Success = 22,
        /// <summary>
        /// There was a system error and the transaction failed because of invalid request parameters.
        /// </summary>
        [Description("FAIL_INVALID")] FailInvalid = 23,
        /// <summary>
        /// There was a system error while performing fee calculation, reserved for future.
        /// </summary>
        [Description("FAIL_FEE")] FailFee = 24,
        /// <summary>
        /// There was a system error while performing balance checks, reserved for future.
        /// </summary>
        [Description("FAIL_BALANCE")] FailBalance = 25,
        /// <summary>
        ///Key not provided in the transaction body
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
        ///Not enough gas was supplied to execute transaction
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
        /// The update of the account failed
        /// </summary>
        [Description("ACCOUNT_UPDATE_FAILED")] AccountUpdateFailed = 37,
        /// <summary>
        /// Provided key encoding was not supported by the system
        /// </summary>
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
        /// Invalid fee submitted
        /// </summary>
        [Description("INVALID_FEE_SUBMITTED")] InvalidFeeSubmitted = 42,
        /// <summary>
        /// Payer signature is invalid
        /// </summary>
        [Description("INVALID_PAYER_SIGNATURE")] InvalidPayerSignature = 43,
        /// <summary>
        /// The keys were not provided in the request.
        /// </summary>
        [Description("KEY_NOT_PROVIDED")] KeyNotProvided = 44,
        /// <summary>
        /// Expiration time provided in the transaction was invalid.
        /// </summary>
        [Description("INVALID_EXPIRATION_TIME")] InvalidExpirationTime = 45,
        /// <summary>
        ///WriteAccess Control Keys are not provided for the file
        /// </summary>
        [Description("NO_WACL_KEY")] NoWaclKey = 46,
        /// <summary>
        ///The contents of file are provided as empty.
        /// </summary>
        [Description("FILE_CONTENT_EMPTY")] FileContentEmpty = 47,
        /// <summary>
        /// The crypto transfer credit and debit do not sum equal to 0
        /// </summary>
        [Description("INVALID_ACCOUNT_AMOUNTS")] InvalidAccountAmounts = 48,
        /// <summary>
        /// Transaction body provided is empty
        /// </summary>
        [Description("EMPTY_TRANSACTION_BODY")] EmptyTransactionBody = 49,
        /// <summary>
        /// Invalid transaction body provided
        /// </summary>
        [Description("INVALID_TRANSACTION_BODY")] InvalidTransactionBody = 50,
        /// <summary>
        /// the type of key (base ed25519 key, KeyList, or ThresholdKey) does not match the type of signature (base ed25519 signature, SignatureList, or ThresholdKeySignature)
        /// </summary>
        [Description("INVALID_SIGNATURE_TYPE_MISMATCHING_KEY")] InvalidSignatureTypeMismatchingKey = 51,
        /// <summary>
        /// the number of key (KeyList, or ThresholdKey) does not match that of signature (SignatureList, or ThresholdKeySignature). e.g. if a keyList has 3 base keys, then the corresponding signatureList should also have 3 base signatures.
        /// </summary>
        [Description("INVALID_SIGNATURE_COUNT_MISMATCHING_KEY")] InvalidSignatureCountMismatchingKey = 52,
        /// <summary>
        /// the livehash body is empty
        /// </summary>
        [Description("EMPTY_LIVE_HASH_BODY")] EmptyLiveHashBody = 53,
        /// <summary>
        /// the livehash data is missing
        /// </summary>
        [Description("EMPTY_LIVE_HASH")] EmptyLiveHash = 54,
        /// <summary>
        /// the keys for a livehash are missing
        /// </summary>
        [Description("EMPTY_LIVE_HASH_KEYS")] EmptyLiveHashKeys = 55,
        /// <summary>
        /// the livehash data is not the output of a SHA-384 digest
        /// </summary>
        [Description("INVALID_LIVE_HASH_SIZE")] InvalidLiveHashSize = 56,
        /// <summary>
        /// the query body is empty
        /// </summary>
        [Description("EMPTY_QUERY_BODY")] EmptyQueryBody = 57,
        /// <summary>
        /// the crypto livehash query is empty
        /// </summary>
        [Description("EMPTY_LIVE_HASH_QUERY")] EmptyLiveHashQuery = 58,
        /// <summary>
        /// the livehash is not present
        /// </summary>
        [Description("LIVE_HASH_NOT_FOUND")] LiveHashNotFound = 59,
        /// <summary>
        /// the account id passed has not yet been created.
        /// </summary>
        [Description("ACCOUNT_ID_DOES_NOT_EXIST")] AccountIdDoesNotExist = 60,
        /// <summary>
        /// the livehash already exists for a given account
        /// </summary>
        [Description("LIVE_HASH_ALREADY_EXISTS")] LiveHashAlreadyExists = 61,
        /// <summary>
        /// File WACL keys are invalid
        /// </summary>
        [Description("INVALID_FILE_WACL")] InvalidFileWacl = 62,
        /// <summary>
        /// Serialization failure
        /// </summary>
        [Description("SERIALIZATION_FAILED")] SerializationFailed = 63,
        /// <summary>
        /// The size of the Transaction is greater than transactionMaxBytes
        /// </summary>
        [Description("TRANSACTION_OVERSIZE")] TransactionOversize = 64,
        /// <summary>
        /// The Transaction has more than 50 levels
        /// </summary>
        [Description("TRANSACTION_TOO_MANY_LAYERS")] TransactionTooManyLayers = 65,
        /// <summary>
        ///Contract is marked as deleted
        /// </summary>
        [Description("CONTRACT_DELETED")] ContractDeleted = 66,
        /// <summary>
        /// the platform node is either disconnected or lagging behind.
        /// </summary>
        [Description("PLATFORM_NOT_ACTIVE")] PlatformNotActive = 67,
        /// <summary>
        /// one public key matches more than one prefixes on the signature map
        /// </summary>
        [Description("KEY_PREFIX_MISMATCH")] KeyPrefixMismatch = 68,
        /// <summary>
        /// transaction not created by platform due to either large backlog or message size exceeded transactionMaxBytes
        /// </summary>
        [Description("PLATFORM_TRANSACTION_NOT_CREATED")] PlatformTransactionNotCreated = 69,
        /// <summary>
        /// auto renewal period is not a positive number of seconds
        /// </summary>
        [Description("INVALID_RENEWAL_PERIOD")] InvalidRenewalPeriod = 70,
        /// <summary>
        /// the response code when a smart contract id is passed for a crypto API request
        /// </summary>
        [Description("INVALID_PAYER_ACCOUNT_ID")] InvalidPayerAccountId = 71,
        /// <summary>
        /// the account has been marked as deleted
        /// </summary>
        [Description("ACCOUNT_DELETED")] AccountDeleted = 72,
        /// <summary>
        /// the file has been marked as deleted
        /// </summary>
        [Description("FILE_DELETED")] FileDeleted = 73,
        /// <summary>
        /// same accounts repeated in the transfer account list
        /// </summary>
        [Description("ACCOUNT_REPEATED_IN_ACCOUNT_AMOUNTS")] AccountRepeatedInAccountAmounts = 74,
        /// <summary>
        /// attempting to set negative balance value for crypto account
        /// </summary>
        [Description("SETTING_NEGATIVE_ACCOUNT_BALANCE")] SettingNegativeAccountBalance = 75,
        /// <summary>
        /// when deleting smart contract that has crypto balance either transfer account or transfer smart contract is required
        /// </summary>
        [Description("OBTAINER_REQUIRED")] ObtainerRequired = 76,
        /// <summary>
        ///when deleting smart contract that has crypto balance you can not use the same contract id as transferContractId as the one being deleted
        /// </summary>
        [Description("OBTAINER_SAME_CONTRACT_ID")] ObtainerSameContractId = 77,
        /// <summary>
        ///transferAccountId or transferContractId specified for contract delete does not exist
        /// </summary>
        [Description("OBTAINER_DOES_NOT_EXIST")] ObtainerDoesNotExist = 78,
        /// <summary>
        ///attempting to modify (update or delete a immutable smart contract, i.e. one created without a admin key)
        /// </summary>
        [Description("MODIFYING_IMMUTABLE_CONTRACT")] ModifyingImmutableContract = 79,
        /// <summary>
        ///Unexpected exception thrown by file system functions
        /// </summary>
        [Description("FILE_SYSTEM_EXCEPTION")] FileSystemException = 80,
        /// <summary>
        /// the duration is not a subset of [MINIMUM_AUTORENEW_DURATION,MAXIMUM_AUTORENEW_DURATION]
        /// </summary>
        [Description("AUTORENEW_DURATION_NOT_IN_RANGE")] AutorenewDurationNotInRange = 81,
        /// <summary>
        /// Decoding the smart contract binary to a byte array failed. Check that the input is a valid hex string.
        /// </summary>
        [Description("ERROR_DECODING_BYTESTRING")] ErrorDecodingBytestring = 82,
        /// <summary>
        /// File to create a smart contract was of length zero
        /// </summary>
        [Description("CONTRACT_FILE_EMPTY")] ContractFileEmpty = 83,
        /// <summary>
        /// Bytecode for smart contract is of length zero
        /// </summary>
        [Description("CONTRACT_BYTECODE_EMPTY")] ContractBytecodeEmpty = 84,
        /// <summary>
        /// Attempt to set negative initial balance
        /// </summary>
        [Description("INVALID_INITIAL_BALANCE")] InvalidInitialBalance = 85,
        /// <summary>
        /// attempt to set negative receive record threshold
        /// </summary>
        [Description("INVALID_RECEIVE_RECORD_THRESHOLD")] InvalidReceiveRecordThreshold = 86,
        /// <summary>
        /// attempt to set negative send record threshold
        /// </summary>
        [Description("INVALID_SEND_RECORD_THRESHOLD")] InvalidSendRecordThreshold = 87,
        /// <summary>
        /// Special Account Operations should be performed by only Genesis account, return this code if it is not Genesis Account
        /// </summary>
        [Description("ACCOUNT_IS_NOT_GENESIS_ACCOUNT")] AccountIsNotGenesisAccount = 88,
        /// <summary>
        /// The fee payer account doesn't have permission to submit such Transaction
        /// </summary>
        [Description("PAYER_ACCOUNT_UNAUTHORIZED")] PayerAccountUnauthorized = 89,
        /// <summary>
        /// FreezeTransactionBody is invalid
        /// </summary>
        [Description("INVALID_FREEZE_TRANSACTION_BODY")] InvalidFreezeTransactionBody = 90,
        /// <summary>
        /// FreezeTransactionBody does not exist
        /// </summary>
        [Description("FREEZE_TRANSACTION_BODY_NOT_FOUND")] FreezeTransactionBodyNotFound = 91,
        /// <summary>
        ///Exceeded the number of accounts (both from and to) allowed for crypto transfer list
        /// </summary>
        [Description("TRANSFER_LIST_SIZE_LIMIT_EXCEEDED")] TransferListSizeLimitExceeded = 92,
        /// <summary>
        /// Smart contract result size greater than specified maxResultSize
        /// </summary>
        [Description("RESULT_SIZE_LIMIT_EXCEEDED")] ResultSizeLimitExceeded = 93,
        /// <summary>
        ///The payer account is not a special account(account 0.0.55)
        /// </summary>
        [Description("NOT_SPECIAL_ACCOUNT")] NotSpecialAccount = 94,
        /// <summary>
        /// Negative gas was offered in smart contract call
        /// </summary>
        [Description("CONTRACT_NEGATIVE_GAS")] ContractNegativeGas = 95,
        /// <summary>
        /// Negative value / initial balance was specified in a smart contract call / create
        /// </summary>
        [Description("CONTRACT_NEGATIVE_VALUE")] ContractNegativeValue = 96,
        /// <summary>
        /// Failed to update fee file
        /// </summary>
        [Description("INVALID_FEE_FILE")] InvalidFeeFile = 97,
        /// <summary>
        /// Failed to update exchange rate file
        /// </summary>
        [Description("INVALID_EXCHANGE_RATE_FILE")] InvalidExchangeRateFile = 98,
        /// <summary>
        /// Payment tendered for contract local call cannot cover both the fee and the gas
        /// </summary>
        [Description("INSUFFICIENT_LOCAL_CALL_GAS")] InsufficientLocalCallGas = 99,
        /// <summary>
        /// Entities with Entity ID below 1000 are not allowed to be deleted
        /// </summary>
        [Description("ENTITY_NOT_ALLOWED_TO_DELETE")] EntityNotAllowedToDelete = 100,
        /// <summary>
        /// Violating one of these rules: 1) treasury account can update all entities below 0.0.1000, 2) account 0.0.50 can update all entities from 0.0.51 - 0.0.80, 3) Network Function Master Account A/c 0.0.50 - Update all Network Function accounts &amp; perform all the Network Functions listed below, 4) Network Function Accounts: i) A/c 0.0.55 - Update Address Book files (0.0.101/102), ii) A/c 0.0.56 - Update Fee schedule (0.0.111), iii) A/c 0.0.57 - Update Exchange Rate (0.0.112).
        /// </summary>
        [Description("AUTHORIZATION_FAILED")] AuthorizationFailed = 101,
        /// <summary>
        /// Fee Schedule Proto uploaded but not valid (append or update is required)
        /// </summary>
        [Description("FILE_UPLOADED_PROTO_INVALID")] FileUploadedProtoInvalid = 102,
        /// <summary>
        /// Fee Schedule Proto uploaded but not valid (append or update is required)
        /// </summary>
        [Description("FILE_UPLOADED_PROTO_NOT_SAVED_TO_DISK")] FileUploadedProtoNotSavedToDisk = 103,
        /// <summary>
        /// Fee Schedule Proto File Part uploaded
        /// </summary>
        [Description("FEE_SCHEDULE_FILE_PART_UPLOADED")] FeeScheduleFilePartUploaded = 104,
        /// <summary>
        /// The change on Exchange Rate exceeds Exchange_Rate_Allowed_Percentage
        /// </summary>
        [Description("EXCHANGE_RATE_CHANGE_LIMIT_EXCEEDED")] ExchangeRateChangeLimitExceeded = 105,
        /// <summary>
        /// Contract permanent storage exceeded the currently allowable limit
        /// </summary>
        [Description("MAX_CONTRACT_STORAGE_EXCEEDED")] MaxContractStorageExceeded = 106,
        /// <summary>
        /// Transfer Account should not be same as Account to be deleted
        /// </summary>
        [Description("TRANSFER_ACCOUNT_SAME_AS_DELETE_ACCOUNT")] TransferAccountSameAsDeleteAccount = 107,
        [Description("TOTAL_LEDGER_BALANCE_INVALID")] TotalLedgerBalanceInvalid = 108,
        /// <summary>
        /// The expiration date/time on a smart contract may not be reduced
        /// </summary>
        [Description("EXPIRATION_REDUCTION_NOT_ALLOWED")] ExpirationReductionNotAllowed = 110,
        /// <summary>
        ///Gas exceeded currently allowable gas limit per transaction
        /// </summary>
        [Description("MAX_GAS_LIMIT_EXCEEDED")] MaxGasLimitExceeded = 111,
        /// <summary>
        /// File size exceeded the currently allowable limit
        /// </summary>
        [Description("MAX_FILE_SIZE_EXCEEDED")] MaxFileSizeExceeded = 112,
        /// <summary>
        /// The Topic ID specified is not in the system.
        /// </summary>
        [Description("INVALID_TOPIC_ID")] InvalidTopicId = 150,
        [Description("INVALID_ADMIN_KEY")] InvalidAdminKey = 155,
        [Description("INVALID_SUBMIT_KEY")] InvalidSubmitKey = 156,
        /// <summary>
        /// An attempted operation was not authorized (ie - a deleteTopic for a topic with no adminKey).
        /// </summary>
        [Description("UNAUTHORIZED")] Unauthorized = 157,
        /// <summary>
        /// A ConsensusService message is empty.
        /// </summary>
        [Description("INVALID_TOPIC_MESSAGE")] InvalidTopicMessage = 158,
        /// <summary>
        /// The autoRenewAccount specified is not a valid, active account.
        /// </summary>
        [Description("INVALID_AUTORENEW_ACCOUNT")] InvalidAutorenewAccount = 159,
        /// <summary>
        /// An adminKey was not specified on the topic, so there must not be an autoRenewAccount.
        /// </summary>
        [Description("AUTORENEW_ACCOUNT_NOT_ALLOWED")] AutorenewAccountNotAllowed = 160,
        /// <summary>
        /// The topic has expired, was not automatically renewed, and is in a 7 day grace period before the topic will be
        /// deleted unrecoverably. This error response code will not be returned until autoRenew functionality is supported
        /// by HAPI.
        /// </summary>
        [Description("TOPIC_EXPIRED")] TopicExpired = 162,
    }
}
