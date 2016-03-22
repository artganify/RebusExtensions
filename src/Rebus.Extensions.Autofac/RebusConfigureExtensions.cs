using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Rebus.Config;

namespace Rebus.Extensions.Autofac
{

    /// <summary>
    ///     Extensions for <see cref="Configure"/>
    /// </summary>
    public static class RebusConfigureExtensions
    {

        /// <summary>
        ///     Returns a new <see cref="RebusConfigurer"/> with registered autofac handler activator based on the provided <see cref="ILifetimeScope"/>
        /// </summary>
        public static RebusConfigurer WithAutofacActivator(this Configure configure, ILifetimeScope lifetimeScope)
        {
            if(configure == null) throw new ArgumentNullException(nameof(configure));
            if(lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));

            // note: this could be some sort of builder instead of static invocation?
            return Configure.With(new AutofacHandlerActivator(lifetimeScope));
        }

    }
}
