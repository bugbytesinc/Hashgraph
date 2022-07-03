/**
 * HAPI CHURN
 * 
 * This was implemented, then removed from HAPI after a month,
 * leaving this code here in case it is put back, if not put
 * back in a few months this will be deleted.
 */
//using Proto;
//using System;

//namespace Hashgraph;

//public sealed record ContractStorageChange
//{
//    /// <summary>
//    /// The storage slot changed.  Up to 32 bytes, 
//    /// big-endian, zero bytes left trimmed.
//    /// </summary>
//    public ReadOnlyMemory<byte> Slot { get; private init; }
//    /// <summary>
//    /// The value read from the storage slot.  Up to 32 bytes, 
//    /// big-endian, zero bytes left trimmed.
//    /// </summary>
//    /// <remarks>
//    /// Because of the way SSTORE operations are charged the 
//    /// slot is always read before being written to.
//    /// </remarks>
//    public ReadOnlyMemory<byte> Read { get; private init; }
//    /// <summary>
//    /// The new value written to the slot.  Up to 32 bytes, 
//    /// big-endian, zero bytes left trimmed, or null if 
//    /// no write operation occurred.
//    /// </summary>
//    /// <remarks>
//    /// If a value of zero is written the value will be present 
//    /// but have zero length (will be empty).  If no wirte 
//    /// operation occurred, the value will be <code>null</code>.
//    /// </remarks>
//    public ReadOnlyMemory<byte>? Written { get; private init; }
//    internal ContractStorageChange(StorageChange change)
//    {
//        Slot = change.Slot.Memory;
//        Read = change.ValueRead.Memory;
//        Written = change.ValueWritten?.Memory;
//    }
//}