using Hashgraph.Mirror;
using Hashgraph.Mirror.Filters;
using Hashgraph.Mirror.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace Hashgraph;
/// <summary>
/// The Mirror Node REST Client.
/// </summary>
public partial class MirrorRestClient
{
    /// <summary>
    /// The base REST endpoint for the mirror node.
    /// </summary>
    private readonly string _endpointUrl;
    /// <summary>
    /// The underlying http client connecting to the mirror node.
    /// </summary>
    private readonly HttpClient _client;
    /// <summary>
    /// The remote mirror node endpoint url.
    /// </summary>
    public string EndpointUrl => _endpointUrl;
    /// <summary>
    /// Constructor, requires the mirror node’s base REST API endpoint url.
    /// </summary>
    /// <param name="endpointUrl">
    /// The mirror node’s base REST API endpoint url.
    /// </param>
    public MirrorRestClient(string endpointUrl)
    {
        _endpointUrl = endpointUrl.EndsWith('/') ? endpointUrl[..^1] : endpointUrl;
        _client = new HttpClient();
    }
    /// <summary>
    /// Retrieves the list of known Hedera Gossip Nodes.
    /// </summary>
    /// <returns>
    /// The list of known Hedera Gossip Nodes.
    /// </returns>
    public IAsyncEnumerable<GossipNodeData> GetGossipNodesAsync()
    {
        return GetPagedItemsAsync<GossipNodePage, GossipNodeData>("network/nodes");
    }
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
    public Task<TokenData?> GetTokenAsync(Address tokenId, params IMirrorQueryFilter[] filters)
    {
        var path = GenerateInitialPath($"tokens/{tokenId}", filters);
        return GetSingleItemAsync<TokenData?>(path);
    }
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
    public IAsyncEnumerable<AccountBalanceData> GetTokenBalancesAsync(Address tokenId, params IMirrorQueryFilter[] filters)
    {
        var allFilters = new IMirrorQueryFilter[] { new LimitFilter(100) }.Concat(filters).ToArray();
        var path = GenerateInitialPath($"tokens/{tokenId}/balances", allFilters);
        return GetPagedItemsAsync<AccountBalancePage, AccountBalanceData>(path);
    }
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
    public Task<HcsMessageData?> GetHcsMessageAsync(Address topicId, ulong sequenceNumber)
    {
        return GetSingleItemAsync<HcsMessageData?>($"topics/{topicId}/messages/{sequenceNumber}");
    }
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
    public IAsyncEnumerable<HcsMessageData> GetHcsMessagesAsync(Address topicId, params IMirrorQueryFilter[] filters)
    {
        var allFilters = new IMirrorQueryFilter[] { new LimitFilter(100) }.Concat(filters).ToArray();
        var path = GenerateInitialPath($"topics/{topicId}/messages", allFilters);
        return GetPagedItemsAsync<HcsMessageDataPage, HcsMessageData>(path);
    }
    /// <summary>
    /// Retrieves information about an account.
    /// </summary>
    /// <param name="account">
    /// The address of the account to retrieve.
    /// </param>
    /// <returns>
    /// An Account information object or throws an exception if not found.
    /// </returns>
    public Task<AccountData?> GetAccountDataAsync(Address account)
    {
        return GetSingleItemAsync<AccountData>($"accounts/{account}");
    }
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
    public IAsyncEnumerable<TokenHoldingData> GetAccountTokenHoldingsAsync(Address account, params IMirrorQueryFilter[] filters)
    {
        var allFilters = new IMirrorQueryFilter[] { new LimitFilter(100) }.Concat(filters).ToArray();
        var path = GenerateInitialPath($"accounts/{account}/tokens", allFilters);
        return GetPagedItemsAsync<TokenHoldingDataPage, TokenHoldingData>(path);
    }
    /// <summary>
    /// Retrieves the token balance for an account and given token.
    /// </summary>
    /// <param name="account">
    /// The account ID.
    /// </param>
    /// <param name="token">
    /// The token ID
    /// </param>
    /// <param name="timestamp">
    /// Optional value indicating the information is required for the 
    /// specified consensus timestamp.
    /// </param>
    /// <returns>
    /// A token balance object if a record was found for the given 
    /// timestamp, otherwise null.
    /// </returns>
    public async Task<long?> GetAccountTokenBalanceAsync(Address account, Address token, params IMirrorQueryFilter[] filters)
    {
        var allFilters = new IMirrorQueryFilter[] { new TokenIsFilter(token) }.Concat(filters).ToArray();
        var path = GenerateInitialPath($"accounts/{account}/tokens", allFilters);
        var payload = await GetSingleItemAsync<TokenHoldingDataPage>(path);
        var record = payload?.TokenHoldings?.FirstOrDefault(r => r.Token == token);
        if (record is not null)
        {
            return record.Balance;
        }
        return null;
    }
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
    public IAsyncEnumerable<AccountData> GetAccountsFromEndorsementAsync(Endorsement endorsement)
    {
        var searchKey = Hex.FromBytes(endorsement.ToBytes(KeyFormat.Mirror));
        return GetPagedItemsAsync<AccountDataPage, AccountData>($"accounts?account.publickey={searchKey}&balance=true&limit=20&order=asc");
    }
    /// <summary>
    /// Retrieves the latest consensus timestamp known by the mirror node.
    /// </summary>
    /// <returns>
    /// The latest consensus timestamp known by the mirror node.
    /// </returns>
    public async Task<ConsensusTimeStamp> GetLatestConsensusTimestampAsync()
    {
        var list = await GetSingleItemAsync<TransactionTimestampDataPage>("transactions?limit=1&order=desc");
        if (list?.Transactions?.Length > 0)
        {
            return list.Transactions[0].Consensus;
        }
        return ConsensusTimeStamp.MinValue;
    }
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
    public async Task<TransactionDetailData[]> GetTransactionGroupAsync(TxId txId)
    {
        var list = await GetSingleItemAsync<TransactionDetailByIdResponse>($"transactions/{txId.Address}-{txId.ValidStartSeconds}-{txId.ValidStartNanos:000000000}");
        if (list?.Transactions?.Length > 0)
        {
            return list.Transactions;
        }
        return Array.Empty<TransactionDetailData>();
    }
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
    public IAsyncEnumerable<TransactionDetailData> GetTransactionsForAccountAsync(Address account, params IMirrorQueryFilter[] filters)
    {
        var allFilters = new IMirrorQueryFilter[] { new LimitFilter(100), new AccountIsFilter(account) }.Concat(filters).ToArray();
        var path = GenerateInitialPath($"transactions", allFilters);
        return GetPagedItemsAsync<TransactionDetailDataPage, TransactionDetailData>(path);
    }
    /// <summary>
    /// Internal helper function to retrieve a paged items structured
    /// object, converting it into an IAsyncEnumerable for consumption.
    /// </summary>
    private async IAsyncEnumerable<TItem> GetPagedItemsAsync<TList, TItem>(string path) where TList : Page<TItem>
    {
        var fullPath = "/api/v1/" + path;
        do
        {
            var payload = await _client.GetFromJsonAsync<TList>(_endpointUrl + fullPath);
            if (payload is not null)
            {
                foreach (var item in payload.GetItems())
                {
                    yield return item;
                }
            }
            fullPath = payload?.Links?.Next;
        }
        while (!string.IsNullOrWhiteSpace(fullPath));
    }
    /// <summary>
    /// Retreives the current and next exchange rate from
    /// the mirror node.
    /// </summary>
    /// <returns>
    /// Exchange rate information for the current and next
    /// rate utilized by the network for determining
    /// transaction and gas fees.
    /// </returns>
    public Task<ExchangeRateData?> GetExchangeRateAsync()
    {
        return GetSingleItemAsync<ExchangeRateData>("network/exchangerate");
    }
    /// <summary>
    /// Helper function to retreive a single item from the rest api call.
    /// </summary>
    private async Task<TItem?> GetSingleItemAsync<TItem>(string path)
    {
        return await _client.GetFromJsonAsync<TItem>($"{_endpointUrl}/api/v1/{path}");
    }
    /// <summary>
    /// Helper function to generate the root path to a mirror node query
    /// with potentail filter options.
    /// </summary>
    /// <param name="rootPath">
    /// Basic root path of the rest query.
    /// </param>
    /// <param name="limit">
    /// Optional parameter to set the limit of enumerable items
    /// returned (typically managed internally to the client library)
    /// </param>
    /// <param name="filters">
    /// Optional list of filters to translate into query parameters.
    /// </param>
    /// <returns>
    /// The mirror node rest query path including optional query parameters.
    /// </returns>
    private static string GenerateInitialPath(string rootPath, IMirrorQueryFilter[] filters)
    {
        if (filters.Length == 0)
        {
            return rootPath;
        }
        var query = string.Join("&", filters.Select(f => $"{HttpUtility.UrlEncode(f.Name)}={HttpUtility.UrlEncode(f.Value)}"));
        return $"{rootPath}?{query}";
    }
}
