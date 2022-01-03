using System.ComponentModel;
namespace Hashgraph
{
    /// <summary>
    /// Pre-Check and Receipt Response Codes - 1to1 mapping with protobuf ResponseCodeEnum
    /// except RpcError indicating a fundamental inability to communicate with an Hedera Node
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// A RPC Error occurred preventing the transaction from being submitted to the network.
        /// </summary>
        [Description("RPC_ERROR")] RpcError = -1,
        /// <summary>
        /// The transaction passed the precheck validations.
        /// </summary>
        [Description("OK")] Ok = 0,
        /// <summary>
        /// For any error not handled by specific error codes listed below.
        /// </summary>
        [Description("INVALID_TRANSACTION")] InvalidTransaction = 1,
        /// <summary>
        /// Payer account does not exist.
        /// </summary>
        [Description("PAYER_ACCOUNT_NOT_FOUND")] PayerAccountNotFound = 2,
        /// <summary>
        /// Node Account provided does not match the node account of the node the transaction was submitted
        /// to.
        /// </summary>
        [Description("INVALID_NODE_ACCOUNT")] InvalidNodeAccount = 3,
        /// <summary>
        /// Pre-Check error when TransactionValidStart + transactionValidDuration is less than current
        /// consensus time.
        /// </summary>
        [Description("TRANSACTION_EXPIRED")] TransactionExpired = 4,
        /// <summary>
        /// Transaction start time is greater than current consensus time
        /// </summary>
        [Description("INVALID_TRANSACTION_START")] InvalidTransactionStart = 5,
        /// <summary>
        /// The given transactionValidDuration was either non-positive, or greater than the maximum 
        /// valid duration of 180 secs.
        /// 
        /// </summary>
        [Description("INVALID_TRANSACTION_DURATION")] InvalidTransactionDuration = 6,
        /// <summary>
        /// The transaction signature is not valid
        /// </summary>
        [Description("INVALID_SIGNATURE")] InvalidSignature = 7,
        /// <summary>
        /// Transaction memo size exceeded 100 bytes
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
        /// This transaction ID is a duplicate of one that was submitted to this node or reached consensus
        /// in the last 180 seconds (receipt period)
        /// </summary>
        [Description("DUPLICATE_TRANSACTION")] DuplicateTransaction = 11,
        /// <summary>
        /// If API is throttled out
        /// </summary>
        [Description("BUSY")] Busy = 12,
        /// <summary>
        /// The API is not currently supported
        /// </summary>
        [Description("NOT_SUPPORTED")] NotSupported = 13,
        /// <summary>
        /// The file id is invalid or does not exist
        /// </summary>
        [Description("INVALID_FILE_ID")] InvalidFileId = 14,
        /// <summary>
        /// The account id is invalid or does not exist
        /// </summary>
        [Description("INVALID_ACCOUNT_ID")] InvalidAccountId = 15,
        /// <summary>
        /// The contract id is invalid or does not exist
        /// </summary>
        [Description("INVALID_CONTRACT_ID")] InvalidContractId = 16,
        /// <summary>
        /// Transaction id is not valid
        /// </summary>
        [Description("INVALID_TRANSACTION_ID")] InvalidTransactionId = 17,
        /// <summary>
        /// Receipt for given transaction id does not exist
        /// </summary>
        [Description("RECEIPT_NOT_FOUND")] ReceiptNotFound = 18,
        /// <summary>
        /// Record for given transaction id does not exist
        /// </summary>
        [Description("RECORD_NOT_FOUND")] RecordNotFound = 19,
        /// <summary>
        /// The solidity id is invalid or entity with this solidity id does not exist
        /// </summary>
        [Description("INVALID_SOLIDITY_ID")] InvalidSolidityId = 20,
        /// <summary>
        /// The responding node has submitted the transaction to the network. Its final status is still
        /// unknown.
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
        /// Key not provided in the transaction body
        /// </summary>
        [Description("KEY_REQUIRED")] KeyRequired = 26,
        /// <summary>
        /// Unsupported algorithm/encoding used for keys in the transaction
        /// </summary>
        [Description("BAD_ENCODING")] BadEncoding = 27,
        /// <summary>
        /// When the account balance is not sufficient for the transfer
        /// </summary>
        [Description("INSUFFICIENT_ACCOUNT_BALANCE")] InsufficientAccountBalance = 28,
        /// <summary>
        /// During an update transaction when the system is not able to find the Users Solidity address
        /// </summary>
        [Description("INVALID_SOLIDITY_ADDRESS")] InvalidSolidityAddress = 29,
        /// <summary>
        /// Not enough gas was supplied to execute transaction
        /// </summary>
        [Description("INSUFFICIENT_GAS")] InsufficientGas = 30,
        /// <summary>
        /// contract byte code size is over the limit
        /// </summary>
        [Description("CONTRACT_SIZE_LIMIT_EXCEEDED")] ContractSizeLimitExceeded = 31,
        /// <summary>
        /// local execution (query) is requested for a function which changes state
        /// </summary>
        [Description("LOCAL_CALL_MODIFICATION_EXCEPTION")] LocalCallModificationException = 32,
        /// <summary>
        /// Contract REVERT OPCODE executed
        /// </summary>
        [Description("CONTRACT_REVERT_EXECUTED")] ContractRevertExecuted = 33,
        /// <summary>
        /// For any contract execution related error not handled by specific error codes listed above.
        /// </summary>
        [Description("CONTRACT_EXECUTION_EXCEPTION")] ContractExecutionException = 34,
        /// <summary>
        /// In Query validation, account with +ve(amount) value should be Receiving node account, the
        /// receiver account should be only one account in the list
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
        /// WriteAccess Control Keys are not provided for the file
        /// </summary>
        [Description("NO_WACL_KEY")] NoWaclKey = 46,
        /// <summary>
        /// The contents of file are provided as empty.
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
        /// the type of key (base ed25519 key, KeyList, or ThresholdKey) does not match the type of
        /// signature (base ed25519 signature, SignatureList, or ThresholdKeySignature)
        /// </summary>
        [Description("INVALID_SIGNATURE_TYPE_MISMATCHING_KEY")] InvalidSignatureTypeMismatchingKey = 51,
        /// <summary>
        /// the number of key (KeyList, or ThresholdKey) does not match that of signature (SignatureList,
        /// or ThresholdKeySignature). e.g. if a keyList has 3 base keys, then the corresponding
        /// signatureList should also have 3 base signatures.
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
        /// Contract is marked as deleted
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
        /// transaction not created by platform due to large backlog
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
        /// when deleting smart contract that has crypto balance either transfer account or transfer smart
        /// contract is required
        /// </summary>
        [Description("OBTAINER_REQUIRED")] ObtainerRequired = 76,
        /// <summary>
        /// when deleting smart contract that has crypto balance you can not use the same contract id as
        /// transferContractId as the one being deleted
        /// </summary>
        [Description("OBTAINER_SAME_CONTRACT_ID")] ObtainerSameContractId = 77,
        /// <summary>
        /// transferAccountId or transferContractId specified for contract delete does not exist
        /// </summary>
        [Description("OBTAINER_DOES_NOT_EXIST")] ObtainerDoesNotExist = 78,
        /// <summary>
        /// attempting to modify (update or delete a immutable smart contract, i.e. one created without a
        /// admin key)
        /// </summary>
        [Description("MODIFYING_IMMUTABLE_CONTRACT")] ModifyingImmutableContract = 79,
        /// <summary>
        /// Unexpected exception thrown by file system functions
        /// </summary>
        [Description("FILE_SYSTEM_EXCEPTION")] FileSystemException = 80,
        /// <summary>
        /// the duration is not a subset of [MINIMUM_AUTORENEW_DURATION,MAXIMUM_AUTORENEW_DURATION]
        /// </summary>
        [Description("AUTORENEW_DURATION_NOT_IN_RANGE")] AutorenewDurationNotInRange = 81,
        /// <summary>
        /// Decoding the smart contract binary to a byte array failed. Check that the input is a valid hex
        /// string.
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
        /// [Deprecated]. attempt to set negative receive record threshold
        /// </summary>
        [Description("INVALID_RECEIVE_RECORD_THRESHOLD")] InvalidReceiveRecordThreshold = 86,
        /// <summary>
        /// [Deprecated]. attempt to set negative send record threshold
        /// </summary>
        [Description("INVALID_SEND_RECORD_THRESHOLD")] InvalidSendRecordThreshold = 87,
        /// <summary>
        /// Special Account Operations should be performed by only Genesis account, return this code if it
        /// is not Genesis Account
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
        /// Exceeded the number of accounts (both from and to) allowed for crypto transfer list
        /// </summary>
        [Description("TRANSFER_LIST_SIZE_LIMIT_EXCEEDED")] TransferListSizeLimitExceeded = 92,
        /// <summary>
        /// Smart contract result size greater than specified maxResultSize
        /// </summary>
        [Description("RESULT_SIZE_LIMIT_EXCEEDED")] ResultSizeLimitExceeded = 93,
        /// <summary>
        /// The payer account is not a special account(account 0.0.55)
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
        /// Violating one of these rules: 1) treasury account can update all entities below 0.0.1000, 2)
        /// account 0.0.50 can update all entities from 0.0.51 - 0.0.80, 3) Network Function Master Account
        /// A/c 0.0.50 - Update all Network Function accounts &amp; perform all the Network Functions listed
        /// below, 4) Network Function Accounts: i) A/c 0.0.55 - Update Address Book files (0.0.101/102),
        /// ii) A/c 0.0.56 - Update Fee schedule (0.0.111), iii) A/c 0.0.57 - Update Exchange Rate
        /// (0.0.112).
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
        /// Gas exceeded currently allowable gas limit per transaction
        /// </summary>
        [Description("MAX_GAS_LIMIT_EXCEEDED")] MaxGasLimitExceeded = 111,
        /// <summary>
        /// File size exceeded the currently allowable limit
        /// </summary>
        [Description("MAX_FILE_SIZE_EXCEEDED")] MaxFileSizeExceeded = 112,
        /// <summary>
        /// When a valid signature is not provided for operations on account with receiverSigRequired=true
        /// </summary>
        [Description("RECEIVER_SIG_REQUIRED")] ReceiverSigRequired = 113,
        /// <summary>
        /// The Topic ID specified is not in the system.
        /// </summary>
        [Description("INVALID_TOPIC_ID")] InvalidTopicId = 150,
        /// <summary>
        /// A provided admin key was invalid.
        /// </summary>
        [Description("INVALID_ADMIN_KEY")] InvalidAdminKey = 155,
        /// <summary>
        /// A provided submit key was invalid.
        /// </summary>
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
        /// The topic has expired, was not automatically renewed, and is in a 7 day grace period before the
        /// topic will be deleted unrecoverably. This error response code will not be returned until
        /// autoRenew functionality is supported by HAPI.
        /// </summary>
        [Description("TOPIC_EXPIRED")] TopicExpired = 162,
        /// <summary>
        /// chunk number must be from 1 to total (chunks) inclusive.
        /// </summary>
        [Description("INVALID_CHUNK_NUMBER")] InvalidChunkNumber = 163,
        /// <summary>
        /// For every chunk, the payer account that is part of initialTransactionID must match the Payer Account of this transaction. The entire initialTransactionID should match the transactionID of the first chunk, but this is not checked or enforced by Hedera except when the chunk number is 1.
        /// </summary>
        [Description("INVALID_CHUNK_TRANSACTION_ID")] InvalidChunkTransactionId = 164,
        /// <summary>
        /// Account is frozen and cannot transact with the token
        /// </summary>
        [Description("ACCOUNT_FROZEN_FOR_TOKEN")] AccountFrozenForToken = 165,
        /// <summary>
        /// An involved account already has more than &lt;tt>tokens.maxPerAccount&lt;/tt> associations with non-deleted tokens.
        /// </summary>
        [Description("TOKENS_PER_ACCOUNT_LIMIT_EXCEEDED")] TokensPerAccountLimitExceeded = 166,
        /// <summary>
        /// The token is invalid or does not exist
        /// </summary>
        [Description("INVALID_TOKEN_ID")] InvalidTokenId = 167,
        /// <summary>
        /// Invalid token decimals
        /// </summary>
        [Description("INVALID_TOKEN_DECIMALS")] InvalidTokenDecimals = 168,
        /// <summary>
        /// Invalid token initial supply
        /// </summary>
        [Description("INVALID_TOKEN_INITIAL_SUPPLY")] InvalidTokenInitialSupply = 169,
        /// <summary>
        /// Treasury Account does not exist or is deleted
        /// </summary>
        [Description("INVALID_TREASURY_ACCOUNT_FOR_TOKEN")] InvalidTreasuryAccountForToken = 170,
        /// <summary>
        /// Token Symbol is not UTF-8 capitalized alphabetical string
        /// </summary>
        [Description("INVALID_TOKEN_SYMBOL")] InvalidTokenSymbol = 171,
        /// <summary>
        /// Freeze key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_FREEZE_KEY")] TokenHasNoFreezeKey = 172,
        /// <summary>
        /// Amounts in transfer list are not net zero
        /// </summary>
        [Description("TRANSFERS_NOT_ZERO_SUM_FOR_TOKEN")] TransfersNotZeroSumForToken = 173,
        /// <summary>
        /// A token symbol was not provided
        /// </summary>
        [Description("MISSING_TOKEN_SYMBOL")] MissingTokenSymbol = 174,
        /// <summary>
        /// The provided token symbol was too long
        /// </summary>
        [Description("TOKEN_SYMBOL_TOO_LONG")] TokenSymbolTooLong = 175,
        /// <summary>
        /// KYC must be granted and account does not have KYC granted
        /// </summary>
        [Description("ACCOUNT_KYC_NOT_GRANTED_FOR_TOKEN")] AccountKycNotGrantedForToken = 176,
        /// <summary>
        /// KYC key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_KYC_KEY")] TokenHasNoKycKey = 177,
        /// <summary>
        /// Token balance is not sufficient for the transaction
        /// </summary>
        [Description("INSUFFICIENT_TOKEN_BALANCE")] InsufficientTokenBalance = 178,
        /// <summary>
        /// Token transactions cannot be executed on deleted token
        /// </summary>
        [Description("TOKEN_WAS_DELETED")] TokenWasDeleted = 179,
        /// <summary>
        /// Supply key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_SUPPLY_KEY")] TokenHasNoSupplyKey = 180,
        /// <summary>
        /// Wipe key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_WIPE_KEY")] TokenHasNoWipeKey = 181,
        /// <summary>
        /// The requested token mint amount would cause an invalid total supply
        /// </summary>
        [Description("INVALID_TOKEN_MINT_AMOUNT")] InvalidTokenMintAmount = 182,
        /// <summary>
        /// The requested token burn amount would cause an invalid total supply
        /// </summary>
        [Description("INVALID_TOKEN_BURN_AMOUNT")] InvalidTokenBurnAmount = 183,
        /// <summary>
        /// A required token-account relationship is missing
        /// </summary>
        [Description("TOKEN_NOT_ASSOCIATED_TO_ACCOUNT")] TokenNotAssociatedToAccount = 184,
        /// <summary>
        /// The target of a wipe operation was the token treasury account
        /// </summary>
        [Description("CANNOT_WIPE_TOKEN_TREASURY_ACCOUNT")] CannotWipeTokenTreasuryAccount = 185,
        /// <summary>
        /// The provided KYC key was invalid.
        /// </summary>
        [Description("INVALID_KYC_KEY")] InvalidKycKey = 186,
        /// <summary>
        /// The provided wipe key was invalid.
        /// </summary>
        [Description("INVALID_WIPE_KEY")] InvalidWipeKey = 187,
        /// <summary>
        /// The provided freeze key was invalid.
        /// </summary>
        [Description("INVALID_FREEZE_KEY")] InvalidFreezeKey = 188,
        /// <summary>
        /// The provided supply key was invalid.
        /// </summary>
        [Description("INVALID_SUPPLY_KEY")] InvalidSupplyKey = 189,
        /// <summary>
        /// Token Name is not provided
        /// </summary>
        [Description("MISSING_TOKEN_NAME")] MissingTokenName = 190,
        /// <summary>
        /// Token Name is too long
        /// </summary>
        [Description("TOKEN_NAME_TOO_LONG")] TokenNameTooLong = 191,
        /// <summary>
        /// The provided wipe amount must not be negative, zero or bigger than the token holder balance
        /// </summary>
        [Description("INVALID_WIPING_AMOUNT")] InvalidWipingAmount = 192,
        /// <summary>
        /// Token does not have Admin key set, thus update/delete transactions cannot be performed
        /// </summary>
        [Description("TOKEN_IS_IMMUTABLE")] TokenIsImmutable = 193,
        /// <summary>
        /// An &lt;tt>associateToken&lt;/tt> operation specified a token already associated to the account
        /// </summary>
        [Description("TOKEN_ALREADY_ASSOCIATED_TO_ACCOUNT")] TokenAlreadyAssociatedToAccount = 194,
        /// <summary>
        /// An attempted operation is invalid until all token balances for the target account are zero
        /// </summary>
        [Description("TRANSACTION_REQUIRES_ZERO_TOKEN_BALANCES")] TransactionRequiresZeroTokenBalances = 195,
        /// <summary>
        /// An attempted operation is invalid because the account is a treasury
        /// </summary>
        [Description("ACCOUNT_IS_TREASURY")] AccountIsTreasury = 196,
        /// <summary>
        /// Same TokenIDs present in the token list
        /// </summary>
        [Description("TOKEN_ID_REPEATED_IN_TOKEN_LIST")] TokenIdRepeatedInTokenList = 197,
        /// <summary>
        /// Exceeded the number of token transfers (both from and to) allowed for token transfer list
        /// </summary>
        [Description("TOKEN_TRANSFER_LIST_SIZE_LIMIT_EXCEEDED")] TokenTransferListSizeLimitExceeded = 198,
        /// <summary>
        /// TokenTransfersTransactionBody has no TokenTransferList
        /// </summary>
        [Description("EMPTY_TOKEN_TRANSFER_BODY")] EmptyTokenTransferBody = 199,
        /// <summary>
        /// TokenTransfersTransactionBody has a TokenTransferList with no AccountAmounts
        /// </summary>
        [Description("EMPTY_TOKEN_TRANSFER_ACCOUNT_AMOUNTS")] EmptyTokenTransferAccountAmounts = 200,
        /// <summary>
        /// The Scheduled entity does not exist; or has now expired, been deleted, or been executed
        /// </summary>
        [Description("INVALID_SCHEDULE_ID")] InvalidScheduleId = 201,
        /// <summary>
        /// The Scheduled entity cannot be modified. Admin key not set
        /// </summary>
        [Description("SCHEDULE_IS_IMMUTABLE")] ScheduleIsImmutable = 202,
        /// <summary>
        /// The provided Scheduled Payer does not exist
        /// </summary>
        [Description("INVALID_SCHEDULE_PAYER_ID")] InvalidSchedulePayerId = 203,
        /// <summary>
        /// The Schedule Create Transaction TransactionID account does not exist
        /// </summary>
        [Description("INVALID_SCHEDULE_ACCOUNT_ID")] InvalidScheduleAccountId = 204,
        /// <summary>
        /// The provided sig map did not contain any new valid signatures from required signers of the scheduled transaction
        /// </summary>
        [Description("NO_NEW_VALID_SIGNATURES")] NoNewValidSignatures = 205,
        /// <summary>
        /// The required signers for a scheduled transaction cannot be resolved, for example because they do not exist or have been deleted
        /// </summary>
        [Description("UNRESOLVABLE_REQUIRED_SIGNERS")] UnresolvableRequiredSigners = 206,
        /// <summary>
        /// Only whitelisted transaction types may be scheduled
        /// </summary>
        [Description("SCHEDULED_TRANSACTION_NOT_IN_WHITELIST")] ScheduledTransactionNotInWhitelist = 207,
        /// <summary>
        /// At least one of the signatures in the provided sig map did not represent a valid signature for any required signer
        /// </summary>
        [Description("SOME_SIGNATURES_WERE_INVALID")] SomeSignaturesWereInvalid = 208,
        /// <summary>
        /// The scheduled field in the TransactionID may not be set to true
        /// </summary>
        [Description("TRANSACTION_ID_FIELD_NOT_ALLOWED")] TransactionIdFieldNotAllowed = 209,
        /// <summary>
        /// A schedule already exists with the same identifying fields of an attempted ScheduleCreate (that is, all fields other than scheduledPayerAccountID)
        /// </summary>
        [Description("IDENTICAL_SCHEDULE_ALREADY_CREATED")] IdenticalScheduleAlreadyCreated = 210,
        /// <summary>
        /// A string field in the transaction has a UTF-8 encoding with the prohibited zero byte
        /// </summary>
        [Description("INVALID_ZERO_BYTE_IN_STRING")] InvalidZeroByteInString = 211,
        /// <summary>
        /// A schedule being signed or deleted has already been deleted
        /// </summary>
        [Description("SCHEDULE_ALREADY_DELETED")] ScheduleAlreadyDeleted = 212,
        /// <summary>
        /// A schedule being signed or deleted has already been executed
        /// </summary>
        [Description("SCHEDULE_ALREADY_EXECUTED")] ScheduleAlreadyExecuted = 213,
        /// <summary>
        /// ConsensusSubmitMessage request's message size is larger than allowed.
        /// </summary>
        [Description("MESSAGE_SIZE_TOO_LARGE")] MessageSizeTooLarge = 214,
        /// <summary>
        /// An operation was assigned to more than one throttle group in a given bucket
        /// </summary>
        [Description("OPERATION_REPEATED_IN_BUCKET_GROUPS")] OperationRepeatedInBucketGroups = 215,
        /// <summary>
        /// The capacity needed to satisfy all opsPerSec groups in a bucket overflowed a signed 8-byte integral type
        /// </summary>
        [Description("BUCKET_CAPACITY_OVERFLOW")] BucketCapacityOverflow = 216,
        /// <summary>
        /// Given the network size in the address book, the node-level capacity for an operation would never be enough to accept a single request; usually means a bucket burstPeriod should be increased
        /// </summary>
        [Description("NODE_CAPACITY_NOT_SUFFICIENT_FOR_OPERATION")] NodeCapacityNotSufficientForOperation = 217,
        /// <summary>
        /// A bucket was defined without any throttle groups
        /// </summary>
        [Description("BUCKET_HAS_NO_THROTTLE_GROUPS")] BucketHasNoThrottleGroups = 218,
        /// <summary>
        /// A throttle group was granted zero opsPerSec
        /// </summary>
        [Description("THROTTLE_GROUP_HAS_ZERO_OPS_PER_SEC")] ThrottleGroupHasZeroOpsPerSec = 219,
        /// <summary>
        /// The throttle definitions file was updated, but some supported operations were not assigned a bucket
        /// </summary>
        [Description("SUCCESS_BUT_MISSING_EXPECTED_OPERATION")] SuccessButMissingExpectedOperation = 220,
        /// <summary>
        /// The new contents for the throttle definitions system file were not valid protobuf
        /// </summary>
        [Description("UNPARSEABLE_THROTTLE_DEFINITIONS")] UnparseableThrottleDefinitions = 221,
        /// <summary>
        /// The new throttle definitions system file were invalid, and no more specific error could be divined
        /// </summary>
        [Description("INVALID_THROTTLE_DEFINITIONS")] InvalidThrottleDefinitions = 222,
        /// <summary>
        /// The transaction references an account which has passed its expiration without renewal funds available, and currently remains in the ledger only because of the grace period given to expired entities
        /// </summary>
        [Description("ACCOUNT_EXPIRED_AND_PENDING_REMOVAL")] AccountExpiredAndPendingRemoval = 223,
        /// <summary>
        /// Invalid token max supply
        /// </summary>
        [Description("INVALID_TOKEN_MAX_SUPPLY")] InvalidTokenMaxSupply = 224,
        /// <summary>
        /// Invalid token nft serial number
        /// </summary>
        [Description("INVALID_TOKEN_NFT_SERIAL_NUMBER")] InvalidTokenNftSerialNumber = 225,
        /// <summary>
        /// Invalid nft id
        /// </summary>
        [Description("INVALID_NFT_ID")] InvalidNftId = 226,
        /// <summary>
        /// Nft metadata is too long
        /// </summary>
        [Description("METADATA_TOO_LONG")] MetadataTooLong = 227,
        /// <summary>
        /// Repeated operations count exceeds the limit
        /// </summary>
        [Description("BATCH_SIZE_LIMIT_EXCEEDED")] BatchSizeLimitExceeded = 228,
        /// <summary>
        /// The range of data to be gathered is out of the set boundaries
        /// </summary>
        [Description("INVALID_QUERY_RANGE")] InvalidQueryRange = 229,
        /// <summary>
        /// A custom fractional fee set a denominator of zero
        /// </summary>
        [Description("FRACTION_DIVIDES_BY_ZERO")] FractionDividesByZero = 230,
        /// <summary>
        /// The transaction payer could not afford a custom fee
        /// </summary>
        [Description("INSUFFICIENT_PAYER_BALANCE_FOR_CUSTOM_FEE")] InsufficientPayerBalanceForCustomFee = 231,
        /// <summary>
        /// More than 10 custom fees were specified
        /// </summary>
        [Description("CUSTOM_FEES_LIST_TOO_LONG")] CustomFeesListTooLong = 232,
        /// <summary>
        /// Any of the feeCollector accounts for customFees is invalid
        /// </summary>
        [Description("INVALID_CUSTOM_FEE_COLLECTOR")] InvalidCustomFeeCollector = 233,
        /// <summary>
        /// Any of the token Ids in customFees is invalid
        /// </summary>
        [Description("INVALID_TOKEN_ID_IN_CUSTOM_FEES")] InvalidTokenIdInCustomFees = 234,
        /// <summary>
        /// Any of the token Ids in customFees are not associated to feeCollector
        /// </summary>
        [Description("TOKEN_NOT_ASSOCIATED_TO_FEE_COLLECTOR")] TokenNotAssociatedToFeeCollector = 235,
        /// <summary>
        /// A token cannot have more units minted due to its configured supply ceiling
        /// </summary>
        [Description("TOKEN_MAX_SUPPLY_REACHED")] TokenMaxSupplyReached = 236,
        /// <summary>
        /// The transaction attempted to move an NFT serial number from an account other than its owner
        /// </summary>
        [Description("SENDER_DOES_NOT_OWN_NFT_SERIAL_NO")] SenderDoesNotOwnNftSerialNo = 237,
        /// <summary>
        /// A custom fee schedule entry did not specify either a fixed or fractional fee
        /// </summary>
        [Description("CUSTOM_FEE_NOT_FULLY_SPECIFIED")] CustomFeeNotFullySpecified = 238,
        /// <summary>
        /// Only positive fees may be assessed at this time
        /// </summary>
        [Description("CUSTOM_FEE_MUST_BE_POSITIVE")] CustomFeeMustBePositive = 239,
        /// <summary>
        /// Fee schedule key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_FEE_SCHEDULE_KEY")] TokenHasNoFeeScheduleKey = 240,
        /// <summary>
        /// A fractional custom fee exceeded the range of a 64-bit signed integer
        /// </summary>
        [Description("CUSTOM_FEE_OUTSIDE_NUMERIC_RANGE")] CustomFeeOutsideNumericRange = 241,
        /// <summary>
        /// A royalty cannot exceed the total fungible value exchanged for an NFT
        /// </summary>
        [Description("ROYALTY_FRACTION_CANNOT_EXCEED_ONE")] RoyaltyFractionCannotExceedOne = 242,
        /// <summary>
        /// Each fractional custom fee must have its maximum_amount, if specified, at least its minimum_amount
        /// </summary>
        [Description("FRACTIONAL_FEE_MAX_AMOUNT_LESS_THAN_MIN_AMOUNT")] FractionalFeeMaxAmountLessThanMinAmount = 243,
        /// <summary>
        /// A fee schedule update tried to clear the custom fees from a token whose fee schedule was already empty
        /// </summary>
        [Description("CUSTOM_SCHEDULE_ALREADY_HAS_NO_FEES")] CustomScheduleAlreadyHasNoFees = 244,
        /// <summary>
        /// Only tokens of type FUNGIBLE_COMMON can be used to as fee schedule denominations
        /// </summary>
        [Description("CUSTOM_FEE_DENOMINATION_MUST_BE_FUNGIBLE_COMMON")] CustomFeeDenominationMustBeFungibleCommon = 245,
        /// <summary>
        /// Only tokens of type FUNGIBLE_COMMON can have fractional fees
        /// </summary>
        [Description("CUSTOM_FRACTIONAL_FEE_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON")] CustomFractionalFeeOnlyAllowedForFungibleCommon = 246,
        /// <summary>
        /// The provided custom fee schedule key was invalid
        /// </summary>
        [Description("INVALID_CUSTOM_FEE_SCHEDULE_KEY")] InvalidCustomFeeScheduleKey = 247,
        /// <summary>
        /// The requested token mint metadata was invalid
        /// </summary>
        [Description("INVALID_TOKEN_MINT_METADATA")] InvalidTokenMintMetadata = 248,
        /// <summary>
        /// The requested token burn metadata was invalid
        /// </summary>
        [Description("INVALID_TOKEN_BURN_METADATA")] InvalidTokenBurnMetadata = 249,
        /// <summary>
        /// The treasury for a unique token cannot be changed until it owns no NFTs
        /// </summary>
        [Description("CURRENT_TREASURY_STILL_OWNS_NFTS")] CurrentTreasuryStillOwnsNfts = 250,
        /// <summary>
        /// An account cannot be dissociated from a unique token if it owns NFTs for the token
        /// </summary>
        [Description("ACCOUNT_STILL_OWNS_NFTS")] AccountStillOwnsNfts = 251,
        /// <summary>
        /// A NFT can only be burned when owned by the unique token's treasury
        /// </summary>
        [Description("TREASURY_MUST_OWN_BURNED_NFT")] TreasuryMustOwnBurnedNft = 252,
        /// <summary>
        /// An account did not own the NFT to be wiped
        /// </summary>
        [Description("ACCOUNT_DOES_NOT_OWN_WIPED_NFT")] AccountDoesNotOwnWipedNft = 253,
        /// <summary>
        /// An AccountAmount token transfers list referenced a token type other than FUNGIBLE_COMMON
        /// </summary>
        [Description("ACCOUNT_AMOUNT_TRANSFERS_ONLY_ALLOWED_FOR_FUNGIBLE_COMMON")] AccountAmountTransfersOnlyAllowedForFungibleCommon = 254,
        /// <summary>
        /// All the NFTs allowed in the current price regime have already been minted
        /// </summary>
        [Description("MAX_NFTS_IN_PRICE_REGIME_HAVE_BEEN_MINTED")] MaxNftsInPriceRegimeHaveBeenMinted = 255,
        /// <summary>
        /// The payer account has been marked as deleted
        /// </summary>
        [Description("PAYER_ACCOUNT_DELETED")] PayerAccountDeleted = 256,
        /// <summary>
        /// The reference chain of custom fees for a transferred token exceeded the maximum length of 2
        /// </summary>
        [Description("CUSTOM_FEE_CHARGING_EXCEEDED_MAX_RECURSION_DEPTH")] CustomFeeChargingExceededMaxRecursionDepth = 257,
        /// <summary>
        /// More than 20 balance adjustments were to satisfy a CryptoTransfer and its implied custom fee payments
        /// </summary>
        [Description("CUSTOM_FEE_CHARGING_EXCEEDED_MAX_ACCOUNT_AMOUNTS")] CustomFeeChargingExceededMaxAccountAmounts = 258,
        /// <summary>
        /// The sender account in the token transfer transaction could not afford a custom fee
        /// </summary>
        [Description("INSUFFICIENT_SENDER_ACCOUNT_BALANCE_FOR_CUSTOM_FEE")] InsufficientSenderAccountBalanceForCustomFee = 259,
        /// <summary>
        /// Currently no more than 4,294,967,295 NFTs may be minted for a given unique token type
        /// </summary>
        [Description("SERIAL_NUMBER_LIMIT_REACHED")] SerialNumberLimitReached = 260,
        /// <summary>
        /// Only tokens of type NON_FUNGIBLE_UNIQUE can have royalty fees
        /// </summary>
        [Description("CUSTOM_ROYALTY_FEE_ONLY_ALLOWED_FOR_NON_FUNGIBLE_UNIQUE")] CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique = 261,
        /// <summary>
        /// The account has reached the limit on the automatic associations count.
        /// </summary>
        [Description("NO_REMAINING_AUTOMATIC_ASSOCIATIONS")] NoRemainingAutomaticAssociations = 262,
        /// <summary>
        /// Already existing automatic associations are more than the new maximum automatic associations.
        /// </summary>
        [Description("EXISTING_AUTOMATIC_ASSOCIATIONS_EXCEED_GIVEN_LIMIT")] ExistingAutomaticAssociationsExceedGivenLimit = 263,
        /// <summary>
        /// Cannot set the number of automatic associations for an account more than the maximum allowed 
        /// token associations &lt;tt>tokens.maxPerAccount&lt;/tt>.
        /// </summary>
        [Description("REQUESTED_NUM_AUTOMATIC_ASSOCIATIONS_EXCEEDS_ASSOCIATION_LIMIT")] RequestedNumAutomaticAssociationsExceedsAssociationLimit = 264,
        /// <summary>
        /// Token is paused. This Token cannot be a part of any kind of Transaction until unpaused.
        /// </summary>
        [Description("TOKEN_IS_PAUSED")] TokenIsPaused = 265,
        /// <summary>
        /// Pause key is not set on token
        /// </summary>
        [Description("TOKEN_HAS_NO_PAUSE_KEY")] TokenHasNoPauseKey = 266,
        /// <summary>
        /// The provided pause key was invalid
        /// </summary>
        [Description("INVALID_PAUSE_KEY")] InvalidPauseKey = 267,
        /// <summary>
        /// The update file in a freeze transaction body must exist.
        /// </summary>
        [Description("FREEZE_UPDATE_FILE_DOES_NOT_EXIST")] FreezeUpdateFileDoesNotExist = 268,
        /// <summary>
        /// The hash of the update file in a freeze transaction body must match the in-memory hash.
        /// </summary>
        [Description("FREEZE_UPDATE_FILE_HASH_DOES_NOT_MATCH")] FreezeUpdateFileHashDoesNotMatch = 269,
        /// <summary>
        /// A FREEZE_UPGRADE transaction was handled with no previous update prepared.
        /// </summary>
        [Description("NO_UPGRADE_HAS_BEEN_PREPARED")] NoUpgradeHasBeenPrepared = 270,
        /// <summary>
        /// A FREEZE_ABORT transaction was handled with no scheduled freeze.
        /// </summary>
        [Description("NO_FREEZE_IS_SCHEDULED")] NoFreezeIsScheduled = 271,
        /// <summary>
        /// The update file hash when handling a FREEZE_UPGRADE transaction differs from the file
        /// hash at the time of handling the PREPARE_UPGRADE transaction.
        /// </summary>
        [Description("UPDATE_FILE_HASH_CHANGED_SINCE_PREPARE_UPGRADE")] UpdateFileHashChangedSincePrepareUpgrade = 272,
        /// <summary>
        /// The given freeze start time was in the (consensus) past.
        /// </summary>
        [Description("FREEZE_START_TIME_MUST_BE_FUTURE")] FreezeStartTimeMustBeFuture = 273,
        /// <summary>
        /// The prepared update file cannot be updated or appended until either the upgrade has
        /// been completed, or a FREEZE_ABORT has been handled.
        /// </summary>
        [Description("PREPARED_UPDATE_FILE_IS_IMMUTABLE")] PreparedUpdateFileIsImmutable = 274,
        /// <summary>
        /// Once a freeze is scheduled, it must be aborted before any other type of freeze can
        /// can be performed.
        /// </summary>
        [Description("FREEZE_ALREADY_SCHEDULED")] FreezeAlreadyScheduled = 275,
        /// <summary>
        /// If an NMT upgrade has been prepared, the following operation must be a FREEZE_UPGRADE.
        /// (To issue a FREEZE_ONLY, submit a FREEZE_ABORT first.)
        /// </summary>
        [Description("FREEZE_UPGRADE_IN_PROGRESS")] FreezeUpgradeInProgress = 276,
        /// <summary>
        /// If an NMT upgrade has been prepared, the subsequent FREEZE_UPGRADE transaction must 
        /// confirm the id of the file to be used in the upgrade.
        /// </summary>
        [Description("UPDATE_FILE_ID_DOES_NOT_MATCH_PREPARED")] UpdateFileIdDoesNotMatchPrepared = 277,
        /// <summary>
        /// If an NMT upgrade has been prepared, the subsequent FREEZE_UPGRADE transaction must 
        /// confirm the hash of the file to be used in the upgrade.
        /// </summary>
        [Description("UPDATE_FILE_HASH_DOES_NOT_MATCH_PREPARED")] UpdateFileHashDoesNotMatchPrepared = 278,
        /// <summary>
        /// Consensus throttle did not allow execution of this transaction. System is throttled at
        /// consensus level.
        /// </summary>
        [Description("CONSENSUS_GAS_EXHAUSTED")] ConsensusGasExhausted = 279,
        /// <summary>
        /// A precompiled contract succeeded, but was later reverted.
        /// </summary>
        [Description("REVERTED_SUCCESS")] RevertedSuccess = 280,
        /// <summary>
        /// All contract storage allocated to the current price regime has been consumed.
        /// </summary>
        [Description("MAX_STORAGE_IN_PRICE_REGIME_HAS_BEEN_USED")] MaxStorageInPriceRegimeHasBeenUsed = 281,
        /// <summary>
        /// An alias used in a CryptoTransfer transaction is not the serialization of a primitive Key
        /// message--that is, a Key with a single Ed25519 or ECDSA(secp256k1) public key and no 
        /// unknown protobuf fields.
        /// </summary>
        [Description("INVALID_ALIAS_KEY")] InvalidAliasKey = 282,
        /// <summary>
        /// A fungible token transfer expected a different number of decimals than the involved 
        /// type actually has.
        /// </summary>
        [Description("UNEXPECTED_TOKEN_DECIMALS")] UnexpectedTokenDecimals = 283,
    }
}
