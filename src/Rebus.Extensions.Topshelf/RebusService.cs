using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Rebus.Config;
using Rebus.Testing;
using Topshelf;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;

namespace Rebus.Extensions.Topshelf
{

    /// <summary>
    ///     Provides Rebus extensions for <see cref="ServiceConfigurator{T}"/>
    /// </summary>
    public static class RebusService
    {

        internal static IHandlerActivator    CustomHandlerActivator;

        private static IBus                 _busInstance;

        /// <summary>
        ///     Returns the registered <see cref="IBus"/> instance
        /// </summary>
        public static IBus Bus
        {
            get { return _busInstance ?? new FakeBus(); }
            set { _busInstance = value; }
        }

        /// <summary>
        ///     Registers the specified <see cref="IHandlerActivator"/> for the Rebus bus
        /// </summary>
        public static HostConfigurator UsingRebusHandlerActivator(this HostConfigurator configurator, IHandlerActivator handlerActivator)
        {
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            if (handlerActivator == null) throw new ArgumentNullException(nameof(handlerActivator));

            CustomHandlerActivator = handlerActivator;

            return configurator;
        }

        /// <summary>
        ///     Registers a Rebus bus as Topshelf service
        /// </summary>
        public static HostConfigurator UsingRebusAsService(this HostConfigurator configurator,
            IHandlerActivator handlerActivator, Action<RebusConfigurer> rebusConfigurer)
        {
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            if (rebusConfigurer == null) throw new ArgumentNullException(nameof(rebusConfigurer));
            if (handlerActivator == null) throw new ArgumentNullException(nameof(handlerActivator));

            return configurator.Service<NullService>(service =>
            {
                service.Rebus(handlerActivator, rebusConfigurer);
                service.WhenStarted(x => NullService.Start());
                service.WhenStopped(x => NullService.Stop());
                service.ConstructUsing(x => new NullService());
            });
        }

        /// <summary>
        ///     Registers a Rebus bus which will be automatically started along with the Topshelf service
        /// </summary>
        /// <remarks>
        ///     The current bus instance can be accessed on <see cref="RebusService.Bus"/>
        /// </remarks>
        public static ServiceConfigurator<T> Rebus<T>(this ServiceConfigurator<T> configurator,
            IHandlerActivator handlerActivator, Action<RebusConfigurer> rebusConfigurer)
            where T : class
        {
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            if (rebusConfigurer == null) throw new ArgumentNullException(nameof(rebusConfigurer));
            if (handlerActivator == null) throw new ArgumentNullException(nameof(handlerActivator));

            var configurer = Configure.With(handlerActivator);
            rebusConfigurer.Invoke(configurer);

            configurator.BeforeStartingService(host => Bus = configurer.Start());
            configurator.BeforeStoppingService(host => Bus?.Dispose());

            return configurator;
        }

        /// <summary>
        ///     Registers a Rebus bus which will be automatically started along with the Topshelf service
        /// </summary>
        /// <remarks>
        ///     The current bus instance can be accessed on <see cref="RebusService.Bus"/>
        /// </remarks>
        public static ServiceConfigurator<T> Rebus<T>(this ServiceConfigurator<T> configurator,
            Action<RebusConfigurer> rebusConfigurer)
            where T : class => Rebus(configurator, CustomHandlerActivator ?? new BuiltinHandlerActivator(), rebusConfigurer);

        private class NullService
        {
            public static void Start()
            {

            }

            public static void Stop()
            {

            }
        }

    }

}
