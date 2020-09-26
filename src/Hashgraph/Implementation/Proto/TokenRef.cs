using Hashgraph;
using System;

namespace Proto
{
    public sealed partial class TokenRef
    {
        internal TokenRef(TokenIdentifier token) : this()
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token), "Token Identifier can not be null.");
            }
            else if (!(token.Address is null) && token.Address != Address.None)
            {
                this.TokenId = new TokenID(token.Address);
            }
            else if (string.IsNullOrWhiteSpace(token.Symbol))
            {
                throw new ArgumentOutOfRangeException(nameof(token), "Token Symbol must be specified if the Address is not.");
            }
            else
            {
                this.Symbol = token.Symbol;
            }
        }
    }
}
