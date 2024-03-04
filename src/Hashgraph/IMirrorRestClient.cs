using System.Collections.Generic;
using System.Threading.Tasks;
using Hashgraph.Mirror;
using Hashgraph.Mirror.Filters;

namespace Hashgraph;

public interface IMirrorRestClient
{
    /// <summary>
    /// The remote mirror node endpoint url.
    /// </summary>
    string EndpointUrl { get; }

    /// <summary>
    /// Retrieves the list of known Hedera Gossip Nodes.
    /// </summary>
    /// <returns>
    /// The list of known Hedera Gossip Nodes.
    /// </returns>
    IAsyncEnumerable<GossipNodeData> GetGossipNodesAsync();

    /// <summary>
    /// Retrieves information for the given token.
    /// </summary>
    /// <param name="tokenId">
    /// Id of the token to retrieve.
    /// </param>
    /// <param name="filters">
    /// Optional list of filter constraints for this query.
    /// </param>
    /// <returns>
    /// The token information.
    /// </returns>
    Task<TokenData?> GetTokenAsync(Address tokenId, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retreives the balances for a given token and filtering criteria.
    /// </summary>
    /// <param name="tokenId">
    /// The Token ID to retrieve.
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// An enumerable of balance and amount pairs, including possibly zero
    /// balance values indicating a token association without a balance.
    /// </returns>
    IAsyncEnumerable<AccountBalanceData> GetTokenBalancesAsync(Address tokenId, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retrieves an HCS message with the given token and sequence number.
    /// </summary>
    /// <param name="topicId">
    /// The HCS message topic to retrieve.
    /// </param>
    /// <param name="sequenceNumber">
    /// The sequence number of message within the token stream to retrieve.
    /// </param>
    /// <returns>
    /// The HCS Message information or null if not found.
    /// </returns>    
    Task<HcsMessageData?> GetHcsMessageAsync(Address topicId, ulong sequenceNumber);

    /// <summary>
    /// Retrieves a list of HCS message.  Messages may be filtered by a starting 
    /// sequence number or consensus timestamp.
    /// </summary>
    /// <param name="topicId">
    /// The topic id of the HCS stream.
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// An enumerable of HCS Messages meeting the given criteria, may be empty if 
    /// none are found.
    /// </returns>
    IAsyncEnumerable<HcsMessageData> GetHcsMessagesAsync(Address topicId, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retrieves information about an account.
    /// </summary>
    /// <param name="account">
    /// The address of the account to retrieve.
    /// </param>
    /// <returns>
    /// An Account information object or throws an exception if not found.
    /// </returns>
    Task<AccountData?> GetAccountDataAsync(Address account);

    /// <summary>
    /// Retrieves the list of token holdings for this account, which includes
    /// both fungible tokens and NFTs.
    /// </summary>
    /// <param name="account">
    /// The account to retrieve the token holdings.
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// An async enumerable of the native token holdings given the constraints.
    /// </returns>
    IAsyncEnumerable<TokenHoldingData> GetAccountTokenHoldingsAsync(Address account, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retrieves the token balance for an account and given token.
    /// </summary>
    /// <param name="account">
    /// The account ID.
    /// </param>
    /// <param name="token">
    /// The token ID
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// The amount of token held by the 
    /// target account, or null if the
    /// token has not been associated.
    /// </returns>
    Task<long?> GetAccountTokenBalanceAsync(Address account, Address token, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retrieves the crypto allowances associated with this account.
    /// </summary>
    /// <param name="account">
    /// The account ID
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// A list of crypto allowances associated with this account.
    /// </returns>
    IAsyncEnumerable<CryptoAllowanceData> GetAccountCryptoAllowancesAsync(Address account, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retrieves the token allowances associated with this account.
    /// </summary>
    /// <param name="account">
    /// The account ID
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// A list of token allowances granted by this account.
    /// </returns>
    IAsyncEnumerable<TokenAllowanceData> GetAccountTokenAllowancesAsync(Address account, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Returns a list of accounts matching the given public key endorsement value.
    /// </summary>
    /// <param name="endorsement">
    /// The endorsement to match against.
    /// </param>
    /// <returns>
    /// Array of account information objects with public keys matching the endorsment,
    /// or empty if no matches are found.
    /// </returns>
    IAsyncEnumerable<AccountData> GetAccountsFromEndorsementAsync(Endorsement endorsement);

    /// <summary>
    /// Retrieves the latest consensus timestamp known by the mirror node.
    /// </summary>
    /// <returns>
    /// The latest consensus timestamp known by the mirror node.
    /// </returns>
    Task<ConsensusTimeStamp> GetLatestConsensusTimestampAsync();

    /// <summary>
    /// Retrieves the entire list of parent and child transactions
    /// having the givin root transaction ID.
    /// </summary>
    /// <param name="txId">
    /// The transaction ID to search by.
    /// </param>
    /// <returns>
    /// A list of transactions (including child transactions with nonces)
    /// matching the given transaciton ID, or an empty list if none are found.
    /// </returns>
    Task<TransactionDetailData[]> GetTransactionGroupAsync(TxId txId);

    /// <summary>
    /// Retrieves a list of transactions associated with this account
    /// </summary>
    /// <param name="account">
    /// Address of the account to search for.
    /// </param>
    /// <param name="filters">
    /// Additional query filters if desired.
    /// </param>
    /// <returns>
    /// A list of transactions (which may be child transactions) that
    /// involve the specified account (regardless of payer status).
    /// </returns>
    IAsyncEnumerable<TransactionDetailData> GetTransactionsForAccountAsync(Address account, params IMirrorQueryFilter[] filters);

    /// <summary>
    /// Retreives the current and next exchange rate from
    /// the mirror node.
    /// </summary>
    /// <returns>
    /// Exchange rate information for the current and next
    /// rate utilized by the network for determining
    /// transaction and gas fees.
    /// </returns>
    Task<ExchangeRateData?> GetExchangeRateAsync();
}