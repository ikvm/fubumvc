﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Diagnostics.ModelBinding;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Diagnostics.Tests
{
    [TestFixture]
    public class Bootstrapping_Integration_Testing
    {
        private FubuRuntime runtime;
        private DashboardModel model;

        [TestFixtureSetUp]
        public void SetUp()
        {
            runtime = FubuApplication.DefaultPolicies().StructureMap().Bootstrap();

            model = runtime.Factory.Get<FubuDiagnosticsEndpoint>().get__fubu();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            runtime.Dispose();
        }

        [Test]
        public void find_the_routes_and_chains_from_bottles()
        {
            runtime.Behaviors.BehaviorFor<StructureMapFubuDiagnostics>(x => x.get_plugin_types())
                .ShouldNotBeNull();

            runtime.Behaviors.BehaviorFor<ModelBindingFubuDiagnostics>(x => x.get_binding_all())
                .ShouldNotBeNull();
        }

        [Test]
        public void builds_partials_for_Visualize_methods()
        {
            var chain = runtime.Behaviors.BehaviorFor<ModelBindingFubuDiagnostics>(x => x.VisualizePartial(null));

            chain.GetType().ShouldEqual(typeof (BehaviorChain));

            chain.IsPartialOnly.ShouldBeTrue();
        }

        [Test]
        public void got_all_the_routes_in_the_diagnostic_javascript_router()
        {
            var routes = runtime.Factory.Get<DiagnosticJavascriptRoutes>();
            var names = routes.Routes().Select(x => x.Name).ToArray();

            names.ShouldContain("StructureMap:plugin_types");
            names.ShouldContain("Chain:chain_Id");
            names.ShouldContain("Requests:requests");
        }

        [Test]
        public void build_styles()
        {
            model.StyleTags.ToString()
                .ShouldContain("<link href=\"/fubu-diagnostics/structuremap.css\" rel=\"stylesheet\" type=\"text/css\" />");
        }

        [Test]
        public void build_scripts()
        {
            model.ScriptTags.ToString().ShouldContain("<script type=\"text/javascript\" src=\"/fubu-diagnostics/structuremap.js\"></script>");
        }

        [Test, Explicit("not reliable in CI, but works fine otherwise. Not gonna worry about it")]
        public void build_rsx_files()
        {
            model.ReactTags.ToString().ShouldContain("<script type=\"text/jsx\" src=\"/fubu-diagnostics/structuremap.jsx\"></script>");
        }
    }
}