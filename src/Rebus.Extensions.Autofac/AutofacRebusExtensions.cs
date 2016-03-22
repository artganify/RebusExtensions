using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Registration;
using Autofac.Features.Scanning;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;

namespace Rebus.Extensions.Autofac
{
    /// <summary>
    ///     Extensions for <see cref="ContainerBuilder"/>
    /// </summary>
    public static class AutofacRebusExtensions
    {

        /// <summary>
        ///     Registers the rebus <see cref="IBus"/> within Autofac
        /// </summary>
        public static IModuleRegistrar UseRebus(this ContainerBuilder containerBuilder, Action<RebusConfigurer> rebusBuilder = null)
        {
            if (containerBuilder == null)
                throw new ArgumentNullException(nameof(containerBuilder));
            return containerBuilder.RegisterModule(new AutofacRebusModule(rebusBuilder));
        }

        /// <summary>
        ///     Registers all implementations of <see cref="IHandleMessages{TMessage}"/> declared in the provided assemblies within Autofac
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterMessageHandlers(this ContainerBuilder containerBuilder, params Assembly[] assembliesToScan)
        {
            if (containerBuilder == null)
                throw new ArgumentNullException(nameof(containerBuilder));
            return containerBuilder.RegisterAssemblyTypes(assembliesToScan)
                // make sure this works :S
                .Where(type => type.GetInterfaces()
                    .Any(i => i.IsAssignableFrom(typeof (IHandleMessages<>)) && !type.IsAbstract))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

    }
}
