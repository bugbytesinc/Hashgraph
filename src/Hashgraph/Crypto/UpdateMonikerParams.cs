#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using System;

namespace Hashgraph;

/// <summary>
/// Represents a change to the Moniker list, 
/// or virtual address list, of a given acount.
/// </summary>
public sealed class UpdateMonikerParams
{
    /// <summary>
    /// The Moniker (or Virtual Address) to be added or
    /// removed from the account.
    /// </summary>
    public Moniker Moniker { get; set; }
    /// <summary>
    /// Update action to take, it can be one of: add as default,
    /// add not as default, or remove from list.
    /// </summary>
    public UpdateMonikerAction Action { get; set; } = UpdateMonikerAction.Add;
    /// <summary>
    /// Convenience operator to cast an existing moniker
    /// to an update parameter, create a simple add command.
    /// To add as the default addres, one must explicitly
    /// construct the command with the <code>AddAsDefault</code>
    /// action.
    /// </summary>
    /// <param name="moniker">
    /// The 20-byte Moniker (EVM address) that is being added or removed.
    /// </param>
    public static implicit operator UpdateMonikerParams(Moniker moniker)
    {
        return new UpdateMonikerParams
        {
            Moniker = moniker,
            Action = UpdateMonikerAction.Add
        };
    }
    /// <summary>
    /// Convenice operator to cast an existing 20-byte array
    /// to an update parameter, creating a simple add command.
    /// To add as a default address (or remove from the list), 
    /// one must explicitly construct the command with the
    /// desired action code.
    /// </summary>
    /// <param name="bytes">
    /// A read-only memory object containing the 20-byte public
    /// key to add or remove from the list of account addresses.
    /// </param>
    public static implicit operator UpdateMonikerParams(ReadOnlyMemory<byte> bytes)
    {
        return new UpdateMonikerParams
        {
            Moniker = new Moniker(bytes),
            Action = UpdateMonikerAction.Add
        };
    }
}
/// <summary>
/// Identifies the action to take on the list
/// of monikers for the account.
/// </summary>
public enum UpdateMonikerAction
{
    /// <summary>
    /// Add a new moniker address and mark it
    /// as the default address.
    /// </summary>
    AddAsDefault = 1,
    /// <summary>
    /// Add a new moniker address without
    /// marking it as the default.
    /// </summary>
    Add = 2,
    /// <summary>
    /// Remove the specified address from the
    /// list of moniker address for this account.
    /// </summary>
    Remove = 3
}