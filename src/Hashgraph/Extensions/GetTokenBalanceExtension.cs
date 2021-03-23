#pragma warning disable CS8604 // Possible null reference argument.
using System;
using System.Threading.Tasks;

namespace Hashgraph.Extensions
{
    /// <summary>
    /// Extends the client functionality to simplify retrieving
    /// just the balance of a single token for an account.
    /// </summary>
    public static class GetTokenBalanceExtension
    {
        /// <summary>
        /// Helper method to retrieve just the balance of a single token for
        /// a given address.  Under the hood it is calling <see cref="Client.GetAccountBalancesAsync(Address, Action{IContext}?)"/>
        /// </summary>
        /// <param name="client">
        /// The client
        /// </param>
        /// <param name="address">
        /// Address of the account to look up.
        /// </param>
        /// <param name="token">
        /// Address of the token to look up (symbol name is not supported)
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns></returns>
        public static async Task<ulong> GetAccountTokenBalanceAsync(this Client client, Address address, Address token, Action<IContext>? configure = null)
        {
            var balances = await client.GetAccountBalancesAsync(address, configure);
            return balances.Tokens.TryGetValue(token, out CryptoBalance? crypto) ? crypto.Balance : 0;
        }
        /// <summary>
        /// Helper method to retrieve just the balance of a single token for
        /// a given contract.  Under the hood it is calling <see cref="Client.GetContractBalancesAsync(Address, Action{IContext}?)"/>
        /// </summary>
        /// <param name="client">
        /// The client
        /// </param>
        /// <param name="contract">
        /// Address of the contract to look up.
        /// </param>
        /// <param name="token">
        /// Address of the token to look up (symbol name is not supported)
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns></returns>
        public static async Task<ulong> GetContractTokenBalanceAsync(this Client client, Address contract, Address token, Action<IContext>? configure = null)
        {
            var balances = await client.GetContractBalancesAsync(contract, configure);
            return balances.Tokens.TryGetValue(token, out CryptoBalance? crypto) ? crypto.Balance : 0;
        }
    }
}
