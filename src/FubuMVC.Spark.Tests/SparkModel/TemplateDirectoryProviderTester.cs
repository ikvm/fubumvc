﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Runtime.Files;
using FubuMVC.Core.View.Model;
using FubuMVC.Core.View.Model.Sharing;
using FubuMVC.Spark.SparkModel;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Spark.Tests.SparkModel
{

    [TestFixture]
    public class template_directory_provider_without_sharings : InteractionContext<TemplateDirectoryProvider<ISparkTemplate>>
    {
        private ISparkTemplate _template;
        private IEnumerable<string> _paths;

        protected override void beforeEach()
        {
            _paths = new[] {"a", "b", "c"};
            _template = new SparkTemplate("filepath", "rootpath", "origin");

            MockFor<ISharingGraph>()
                .Stub(x => x.SharingsFor(Arg<string>.Is.Anything))
                .Return(Enumerable.Empty<string>());

            created_builder(false);
        }

        private void created_builder(bool includeDirectAncestors)
        {
            var builder = MockRepository.GenerateMock<ISharedPathBuilder>();
            
            builder
                .Expect(x => x.BuildBy(_template.FilePath, _template.RootPath, includeDirectAncestors))
                .Return(_paths);                        
            
            Container.Inject(builder);
        }

        [Test]
        public void when_no_sharing_exists_only_local_paths_are_returned()
        {
            ClassUnderTest.SharedPathsOf(_template).SequenceEqual(_paths).ShouldBeTrue();
        }

        [Test]
        public void the_shared_path_builder_is_given_proper_args_when_sharedpaths_of()
        {
            ClassUnderTest.SharedPathsOf(_template);
            MockFor<ISharedPathBuilder>().VerifyAllExpectations();
        }

        [Test]
        public void the_shared_path_builder_is_given_proper_args_when_reachables_of()
        {
            created_builder(true);
            ClassUnderTest.ReachablesOf(_template);
            MockFor<ISharedPathBuilder>().VerifyAllExpectations();
        }
    }

    public class template_directory_provider_with_sharings : InteractionContext<TemplateDirectoryProvider<ISparkTemplate>>
    {
        private const string Shared = "S";
        private string _root;
        private string _pak1Root;
        private string _pak2Root;

        private TemplateRegistry<ISparkTemplate> _templates;
        private SharingGraph _graph;

        protected override void beforeEach()
        {
            _root = AppDomain.CurrentDomain.BaseDirectory;
            _pak1Root = FileSystem.Combine(_root, "Packs", "Pak1");
            _pak2Root = FileSystem.Combine(_root, "Packs", "Pak2");

            _templates = new TemplateRegistry<ISparkTemplate>(new[]
            {
                new SparkTemplate(FileSystem.Combine(_root, "Actions", "Home", "home.spark"), _root, ContentFolder.Application), 
                new SparkTemplate(FileSystem.Combine(_pak1Root, "Actions", "Home", "home.spark"), _pak1Root, "Pak1"),
                new SparkTemplate(FileSystem.Combine(_pak2Root, "Home", "home.spark"), _pak2Root, "Pak2"),
                new SparkTemplate(FileSystem.Combine(_pak2Root, "Home", Shared, "_test.spark"), _pak2Root, "Pak2"),
            });

            _graph = new SharingGraph();
            _graph.Dependency("Pak1", "Pak2");
            _graph.Dependency("Pak2", ContentFolder.Application);
            _graph.Dependency(ContentFolder.Application, "Pak3");

            Container.Inject<ISharedPathBuilder>(new SharedPathBuilder(new []{Shared}));
            Container.Inject<ISharingGraph>(_graph);
            Container.Inject<ITemplateRegistry<ISparkTemplate>>(_templates);
        }

        [Test]
        public void locals_and_sharings_are_combined_1()
        {
            var expected = new List<string>
            {
                FileSystem.Combine(_pak1Root, "Actions", "Home", Shared),
                FileSystem.Combine(_pak1Root, "Actions", Shared),
                FileSystem.Combine(_pak1Root, Shared),
                FileSystem.Combine(_pak2Root, Shared)                                   
            };

            ClassUnderTest.SharedPathsOf(templateAt(1)).ShouldHaveTheSameElementsAs(expected);
        }

        [Test]
        public void locals_and_sharings_are_combined_2()
        {
            var expected = new List<string>
            {
                FileSystem.Combine(_pak2Root, "Home", Shared),
                FileSystem.Combine(_pak2Root, Shared),
                FileSystem.Combine(_root, Shared)                                   
            };

            ClassUnderTest.SharedPathsOf(templateAt(2)).ShouldHaveTheSameElementsAs(expected);
        }

        [Test]
        public void locals_and_sharings_are_combined_3()
        {
            var expected = new List<string>
            {
                FileSystem.Combine(_pak1Root, "Actions", "Home"),
                FileSystem.Combine(_pak1Root, "Actions", "Home", Shared),
                FileSystem.Combine(_pak1Root, "Actions"),
                FileSystem.Combine(_pak1Root, "Actions", Shared),
                FileSystem.Combine(_pak1Root),
                FileSystem.Combine(_pak1Root, Shared),
                FileSystem.Combine(_pak2Root, Shared)                                   
            };

            ClassUnderTest.ReachablesOf(templateAt(1)).ShouldHaveTheSameElementsAs(expected);
        }

        [Test]
        public void locals_and_sharings_are_combined_4()
        {
            var expected = new List<string>
            {
                FileSystem.Combine(_pak2Root, "Home"),
                FileSystem.Combine(_pak2Root, "Home", Shared),
                FileSystem.Combine(_pak2Root),
                FileSystem.Combine(_pak2Root, Shared),
                FileSystem.Combine(_root, Shared)                                   
            };

            ClassUnderTest.ReachablesOf(templateAt(2)).ShouldHaveTheSameElementsAs(expected);
        }

        [Test]
        public void locals_and_sharings_are_combined_5()
        {
            var expected = new List<string>
            {
                FileSystem.Combine(_root, "Actions", "Home"),
                FileSystem.Combine(_root, "Actions", "Home", Shared),
                FileSystem.Combine(_root, "Actions"),
                FileSystem.Combine(_root, "Actions", Shared),                               
                _root,                               
                FileSystem.Combine(_root, Shared)                               
            };

            ClassUnderTest.ReachablesOf(templateAt(0)).ShouldHaveTheSameElementsAs(expected);
        }



        private ISparkTemplate templateAt(int index)
        {
            return _templates.ElementAt(index);
        }

    }
}