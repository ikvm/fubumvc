﻿using FubuMVC.Core.Diagnostics.Packaging;
using FubuMVC.Core.ServiceBus.Polling;
using FubuMVC.Core.ServiceBus.Runtime;
using FubuMVC.Core.ServiceBus.ScheduledJobs.Execution;
using FubuMVC.Core.ServiceBus.Subscriptions;

namespace FubuMVC.Core.ServiceBus
{
    public class ServiceBusActivator : IActivator, IDeactivator
    {
        private readonly TransportActivator _transports;
        private readonly SubscriptionActivator _subscriptions;
        private readonly IScheduledJobController _scheduledJobs;
        private readonly PollingJobActivator _pollingJobs;
        private readonly TransportSettings _settings;

        public ServiceBusActivator(TransportActivator transports, SubscriptionActivator subscriptions,
            IScheduledJobController scheduledJobs, PollingJobActivator pollingJobs, TransportSettings settings)
        {
            _transports = transports;
            _subscriptions = subscriptions;
            _scheduledJobs = scheduledJobs;
            _pollingJobs = pollingJobs;
            _settings = settings;
        }

        public void Activate(IActivationLog log)
        {
            if (!_settings.Enabled)
            {
                log.Trace("Skipping activation because FubuTranportation is disabled.");
                return;
            }

            _transports.Activate(log);
            _subscriptions.Activate(log);
            _pollingJobs.Activate(log);

            /* TODO -- add timings back here!
            PackageRegistry.Timer.Record("Activating Transports and Starting Listening",
                () => _transports.Activate(packages, log));

            PackageRegistry.Timer.Record("Activating Subscriptions", () => _subscriptions.Activate(packages, log));

            PackageRegistry.Timer.Record("Activating Polling Jobs", () => _pollingJobs.Activate(packages, log));
             * */
        }

        public void Deactivate(IActivationLog log)
        {
            if(_settings.Enabled) return;

            log.Trace("Shutting down the scheduled jobs");
            _scheduledJobs.Deactivate();
        }
    }
}