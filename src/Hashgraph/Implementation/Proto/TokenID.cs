using Hashgraph;

namespace Proto
{
    public sealed partial class TokenID
    {
        internal TokenID(Address token) : this()
        {
            ShardNum = token.ShardNum;
            RealmNum = token.RealmNum;
            TokenNum = token.AccountNum;
        }
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, TokenNum);
        }
    }
}
