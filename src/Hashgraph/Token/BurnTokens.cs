﻿using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Removes token coins from the treasury.
    /// </summary>
    /// <param name="token">
    /// The identifier (Address/Symbol) of the token to remove coins from.
    /// </param>
    /// <param name="amount">
    /// The amount of coins to remove (specified in the smallest denomination).
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt indicating a successful operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TokenReceipt> BurnTokenAsync(Address token, ulong amount, Action<IContext>? configure = null)
    {
        return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, amount), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Removes token coins from the treasury.
    /// </summary>
    /// <param name="token">
    /// The identifier (Address/Symbol) of the token to remove coins from.
    /// </param>
    /// <param name="amount">
    /// The amount of coins to remove (specified in the smallest denomination).
    /// </param>
    /// <param name="signatory">
    /// Additional signing key matching the administrative endorsements
    /// associated with this token (if not already added in the context).
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt indicating a successful operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TokenReceipt> BurnTokenAsync(Address token, ulong amount, Signatory signatory, Action<IContext>? configure = null)
    {
        return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, amount), configure, false, signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Removes token coins from the treasury.
    /// </summary>
    /// <param name="token">
    /// The identifier (Address/Symbol) of the token to remove coins from.
    /// </param>
    /// <param name="amount">
    /// The amount of coins to remove (specified in the smallest denomination).
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record indicating a successful operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TokenRecord> BurnTokenWithRecordAsync(Address token, ulong amount, Action<IContext>? configure = null)
    {
        return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, amount), configure, true).ConfigureAwait(false));
    }
    /// <summary>
    /// Removes token coins from the treasury.
    /// </summary>
    /// <param name="token">
    /// The identifier (Address/Symbol) of the token to remove coins from.
    /// </param>
    /// <param name="amount">
    /// The amount of coins to remove (specified in the smallest denomination).
    /// </param>
    /// <param name="signatory">
    /// Additional signing key matching the administrative endorsements
    /// associated with this token (if not already added in the context).
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record indicating a successful operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TokenRecord> BurnTokenWithRecordAsync(Address token, ulong amount, Signatory signatory, Action<IContext>? configure = null)
    {
        return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, amount), configure, true, signatory).ConfigureAwait(false));
    }
}