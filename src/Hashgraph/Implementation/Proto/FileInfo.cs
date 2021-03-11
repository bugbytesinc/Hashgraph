using System;

namespace Proto
{
    public sealed partial class FileGetInfoResponse
    {
        public static partial class Types
        {
            public sealed partial class FileInfo
            {
                internal Hashgraph.FileInfo ToFileInfo()
                {
                    return new Hashgraph.FileInfo
                    {
                        File = FileID.AsAddress(),
                        Memo = Memo,
                        Size = Size,
                        Expiration = ExpirationTime.ToDateTime(),
                        Endorsements = Keys?.ToEndorsements() ?? Array.Empty<Hashgraph.Endorsement>(),
                        Deleted = Deleted
                    };
                }
            }
        }
    }
}
