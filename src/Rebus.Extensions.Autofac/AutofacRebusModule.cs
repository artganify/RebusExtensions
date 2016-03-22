using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;

namespace Rebus.Extensions.Autofac
{

    /// <summary>
    ///     Represents a module for wiring up Rebus within Autofac
    /// </summary>
    internal class AutofacRebusModule : Module
    {

        private readonly Action<RebusConfigurer> _rebusConfigurer;

        /// <summary>
        ///     Creates a new <see cref="AutofacRebusModule"/> using the specified <see cref="RebusConfigurer"/>
        /// </summary>
        public AutofacRebusModule(Action<RebusConfigurer> rebusConfigurer = null)
        {
            _rebusConfigurer = rebusConfigurer;
        }

        /// <summary>
        ///     Wires up the Rebus <see cref="IBus"/> within Autofac upon loading
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c =>
                {
                    var currentMessageContext = MessageContext.Current;
                    if (currentMessageContext == null)
                        throw new InvalidOperationException("Attempted to inject the current message context from MessageContext.Current, but it was null! " +
                                                            "Did you attempt to resolve IMessageContext from outside of a Rebus message handler?");
                    return currentMessageContext;
                })
                .InstancePerDependency()
                .ExternallyOwned();

            builder.Register(context =>
            {
                var lifetimeScope   = context.Resolve<ILifetimeScope>();
                var configurer      = new Configure().WithAutofacActivator(lifetimeScope);

                // optionally apply configuration
                _rebusConfigurer?.Invoke(configurer);

                return configurer.Start();
            })
            .As<IBus>()
            .SingleInstance();
        }
    }
}
