using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    public class Transaction : IData
    {
        private readonly byte[] _data;

        ReadOnlySpan<byte> IData.Data { get { return _data; } }

        internal Transaction(ReadOnlySpan<byte> data)
        {
            _data = data.ToArray();
        }
    }
}
