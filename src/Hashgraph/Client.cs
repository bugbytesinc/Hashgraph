using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Collections.Concurrent;

namespace Hashgraph
{
    public sealed partial class Client
    {
        private readonly ContextStack _context;
        private readonly ConcurrentDictionary<string, Channel> _channels;
        public Client(Action<IContext>? configure = null) : this(configure, null)
        {
        }
        private Client(Action<IContext>? configure, ContextStack? parent)
        {
            _context = new ContextStack(parent);
            if (parent == null)
            {
                // Hard Code Defaults for Now.
                _context.Fee = 100000;
                _context.TransactionDuration = TimeSpan.FromSeconds(120);
                _context.RetryCount = 5;
                _context.RetryDelay = TimeSpan.FromMilliseconds(200);
                _context.CreateAccountAutoRenewPeriod = TimeSpan.FromDays(31);
                _context.CreateAccountCreateRecordSendThreshold = int.MaxValue;
                _context.CreateAcountRequireSignatureReceiveThreshold = int.MaxValue;
                _context.CreateAccountAlwaysRequireReceiveSignature = false;
            }
            configure?.Invoke(_context);
            _channels = new ConcurrentDictionary<string, Channel>();
        }
        public void Configure(Action<IContext> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure), "Configuration action cannot be null.");
            }
            configure(_context);
        }
        public Client Clone(Action<IContext>? configure = null)
        {
            return new Client(configure, _context);
        }
        private ContextStack CreateChildContext(Action<IContext>? configure)
        {
            var context = new ContextStack(_context);
            configure?.Invoke(context);
            return context;
        }
    }
}
