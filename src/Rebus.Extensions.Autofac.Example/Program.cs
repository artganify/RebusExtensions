using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Rebus.Bus;
using Rebus.Routing.TypeBased;
using Rebus.Transport.Msmq;

namespace Rebus.Extensions.Autofac.Example
{
    class Program
    {
        static void Main(string[] args)
        {

            var containerBuilder = new ContainerBuilder();

            // register all implementations of IHandleMessage<'T> within Autofac
            containerBuilder.RegisterMessageHandlers(typeof (Program).Assembly);

            // build up rebus
            containerBuilder.UseRebus(x =>
            {
                x.Logging(log => log.Trace());
                x.Transport(transport => transport.UseMsmq("scary-frameworks-and-nice-queues"));
                x.Routing(routing => routing.TypeBased().MapAssemblyOf<Program>("another-input-queue"));
            });

            // create container
            var container = containerBuilder.Build();

            // resolve bus
            var bus = container.Resolve<IBus>();
        }
    }
}
