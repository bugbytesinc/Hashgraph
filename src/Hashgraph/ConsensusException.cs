﻿using System;

namespace Hashgraph;

/// <summary>
/// Exception thrown when a network call was accepted by the gateway 
/// node but did not achieve network consensus before the expiration 
/// timeout of the transaction request occurred.
/// </summary>
public sealed class ConsensusException : Exception
{
    /// <summary>
    /// The final <see cref="ResponseCode"/> returned by the network 
    /// prior to transaction request expiration.
    /// </summary>
    public ResponseCode Status { get; private set; }
    /// <summary>
    /// The Transaction ID generated by the library (or client code) 
    /// identifying the request.
    /// </summary>
    public TxId TxId { get; private set; }
    /// <summary>
    /// Public Constructor.
    /// </summary>
    /// <param name="message">
    /// The message generated by the library describing the 
    /// condition that raised the exception.
    /// </param>
    /// <param name="transaction">
    /// The Transaction ID of the request that failed.
    /// </param>
    /// <param name="code">
    /// The final <see cref="ResponseCode"/> returned by the network 
    /// prior to transaction request expiration.
    /// </param>
    public ConsensusException(string message, TxId transaction, ResponseCode code) : base(message)
    {
        Status = code;
        TxId = transaction;
    }
}