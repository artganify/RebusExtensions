using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Rebus.Activation;
using Rebus.Handlers;
using Rebus.Transport;

namespace Rebus.Extensions.Autofac
{

    /// <summary>
    ///     Implementation of <see cref="IHandlerActivator"/> using Autofac for resolving
    /// </summary>
    public class AutofacHandlerActivator : IHandlerActivator
    {

        private readonly ILifetimeScope _lifetimeScope;

        public AutofacHandlerActivator(ILifetimeScope lifetimeScope)
        {
            if(lifetimeScope == null)
                throw new ArgumentNullException(nameof(lifetimeScope));
            _lifetimeScope = lifetimeScope;
        }

        /// <summary>
        ///     Must return all relevant handler instances for the given message
        /// </summary>
#pragma warning disable 1998
        public async Task<IEnumerable<IHandleMessages<TMessage>>> GetHandlers<TMessage>(TMessage message, ITransactionContext transactionContext)
#pragma warning restore 1998
        {
            var lifetimeScope = transactionContext
                .GetOrAdd("current-autofac-lifetime-scope", () =>
                {
                    var scope = _lifetimeScope.BeginLifetimeScope();

                    transactionContext.OnDisposed(() => scope.Dispose());

                    return scope;
                });

            var handledMessageTypes = typeof(TMessage).GetBaseTypes()
                .Concat(new[] { typeof(TMessage) });

            return handledMessageTypes
                .SelectMany(handledMessageType =>
                {
                    var implementedInterface = typeof(IHandleMessages<>).MakeGenericType(handledMessageType);
                    var implementedInterfaceSequence = typeof(IEnumerable<>).MakeGenericType(implementedInterface);

                    return (IEnumerable<IHandleMessages>)lifetimeScope.Resolve(implementedInterfaceSequence);
                })
                .Cast<IHandleMessages<TMessage>>();
        }
    }
}
