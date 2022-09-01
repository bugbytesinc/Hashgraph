namespace Proto;

public sealed partial class NftTransfer
{
    internal NftTransfer(Hashgraph.AssetTransfer xfer) : this()
    {
        SenderAccountID = new AccountID(xfer.From);
        ReceiverAccountID = new AccountID(xfer.To);
        SerialNumber = xfer.Asset.SerialNum;
        IsApproval = xfer.Delegated;
    }
}