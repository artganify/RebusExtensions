Rebus Extensions
================

This repository contains various extensions, contributions and framework integrations for [Rebus](https://github.com/rebus-org/Rebus/),
the **as friendly as machinely possible**, lean and frictionless .NET service bus!

The following extensions are available:+1:

* **Rebus.Extensions.Autofac** - Autofac container integration for Rebus, similar to [Rebus.Autofac](https://github.com/rebus-org/Rebus/tree/master/Rebus.Autofac),
but following the *RRR* principle as well as separating bus registration from message handler resolving.

* **Rebus.Extensions.Topshelf** - Rebus integration into Topshelf. (Experimental)

### Rebus.Extensions.Autofac

The *Rebus.Extensions.Autofac* extension is similar to [Rebus.Autofac](https://github.com/rebus-org/Rebus/tree/master/Rebus.Autofac), except it doesn't
use the `IContainerAdapter` adapter for wiring up the `IBus` within the Autofac container because I think service registration
and resolving (especially resolving message handlers) should be separated. :)

#### How to use?

##### Using IContainerBuilder

There are two ways to use this extension. The first one is via the Autofac's `IContainerBuilder` and implemented as a
very convenient extension method:

    containerBuilder.UseRebus(x =>
    {
        x.Logging(log => log.Trace());
        x.Transport(transport => transport.UseMsmq("scary-frameworks-and-nice-queues"));
        x.Routing(routing => routing.TypeBased().MapAssemblyOf<Program>("another-input-queue"));
    });

This one provides an action delegate for the actual Rebus configuration using the familiar `RebusConfigurer`. After configuration,
the `IBus` is registered as a singleton instance within Autofac. Also, it has an additional extension to automatically register
all message handlers within the scope of a provided assembly:

containerBuilder.RegisterMessageHandlers(typeof (Program).Assembly);

##### Using RebusConfigurer

The second way is to just use the standard `RebusConfigurer` and the `AutofacHandlerActivator`. Either like this:

    Configure.With(new AutofacHandlerActivator(container))...

or...

    new Configure().WithAutofacActivator(container));


*Please note* that this simply hooks up an adapter for resolving message handlers. They do not register the Rebus bus within Autofac!

For more examples, check out the [example project](https://github.com/artganify/RebusExtensions/tree/master/src/Rebus.Extensions.Autofac.Example)!

### Rebus.Extensions.Topshelf

This extension provides a Rebus integration for Topshelf.

#### How to use?

The extension contains various extension methods for both the Topshelf `HostConfigurator` and `ServiceConfigurator<'T>`. E.g...

    HostFactory.Run(host => {

        // provide a IHandlerActivator (later used @ service configurator)
        host.UsingRebusHandlerActivator(new CustomHandlerActivator());

        // host rebus as the service itself
        host.UsingRebusAsService(new CustomHandlerActivator, rebus => {
            rebus.Logging(...)
        });

        // start a IBus along with the Topshelf service, accessible via RebusService.Bus
        host.Service<MyService>(service => {
            service.Rebus(rebus => {
                rebus.Logging(...);
            }
        }

    });
