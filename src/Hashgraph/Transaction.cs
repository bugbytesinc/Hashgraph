using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    public class Transaction : IData
    {
        private readonly ReadOnlyMemory<byte> _data;

        ReadOnlyMemory<byte> IData.Data { get { return _data; } }

        internal Transaction(ReadOnlyMemory<byte> data)
        {
            _data = data;
        }
    }
}
