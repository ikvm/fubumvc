﻿using System;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class LightningUriTester
    {
        private LightningUri theUri;

        [SetUp]
        public void SetUp()
        {
            theUri = new LightningUri("lq.tcp://localhost:2424/some_queue");
        }

        [Test]
        public void blows_up_if_protocol_is_not_lightning_queues()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                new LightningUri("foo://bar");
            });
        }

        [Test]
        public void finds_the_port()
        {
            theUri.Port.ShouldEqual(2424);
        }

        [Test]
        public void parses_the_queue_name()
        {
            theUri.QueueName.ShouldEqual("some_queue");
        }
    }
}