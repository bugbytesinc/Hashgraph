using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    public sealed partial class Client
    {
        private readonly ContextStack _context;
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
                _context.BusyRetryCount = 5;
                _context.BusyRetryDelay = TimeSpan.FromMilliseconds(200);
            }
            configure?.Invoke(_context);
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
