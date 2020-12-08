---
title: Concepts
---

# Concepts

## Address

The first concept is the **Address**.  An address is an identifier for an Account, Contract, File, Consensus Topic or Hedera Token.  The identifier consists of three parts: Shard, Realm and Number.  Currently, the Hedera network only has one instance of a shard and realm; for the time being, these values will be zero.  The third identifier is the address number.  The network generates this number upon item creation.

## Gateway

The **Gateway** is an object provided by the library for identifying an Hedera Gossip Network Node.  Each Hedera node has a public addressable internet endpoint and linked network Account address.  The internet endpoint represents the publicly available gRPC service hosted by the network node, this is the service the .NET library will connect to when making the balance request.  The account address represents the Hedera crypto account paired with the node that receives funds from transaction requests requiring payment.  Hedera lists the address book of gateways for the test networks at https://docs.hedera.com/guides/testnet/testnet-nodes. 

## Client

The **Client** object orchestrates communication with the Hedera network.  It takes on the role of encoding messages, sending them to the gateway, waiting for a response and then decoding the results, returning normal .NET class objects.  It provides one or more methods relating to each possible network function; each following the standard .net async/await pattern.  The client is resilient to busy network responses and generally waits for network consensus before returning results.

## Context

Some initial configuration is required when creating a Client object.  For example, for balance queries, the Client must know which Gateway to contact to ask for the information.  Each client instance maintains a **Context** representing the client’s configuration.  The client provides methods such as _configure_ and _clone_ enabling calling code to modify the client’s configuration.  These methods accept a callback method, that when called, receives the context represented as an _IContext_ interface containing properties that may be changed.

## Payer

Whereas querying the Hedera network for balances is presently free other actions, particularly those changing network state, require the payment of a small fee to execute.  The .NET library refers to the account paying these transaction fees as the **Payer**.  The payer consists of two pieces of information, the _Account_ identifying the payer, and a _Signatory_ authorizing the spending of funds from the account.  

## Signatory

The **Signatory** is a private key, or callback method that can sign a transaction.  Typically, a signatory is an account holder’s private key.  The .NET framework will accept Ed25519 keys as signatories and use them to sign transactions.  It also accepts callback functions for advanced scenarios such as distributed cooperative systems coordinating the signatures of a sigle transaction.

## Endorsement

Most accounts are secured by a single private Ed25519 key.  The hedera network never sees these private keys but has been given the public key corresponding to each account's private key during creation.  Accounts are not the only thing protected by keys in the hedera network.  Contracts, Topics and Tokens can be administered (modified) by parties holding administrative keys assigned to these assets.  For example, when creating a Token, there is an opportunity to provide a public key enabling access to various administrative functions against the token.  The .NET SDK provides the Endorsement object to hold this public key value.