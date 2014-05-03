using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RP.Core.Provider;
using RP.Test.Example;

namespace RP.Core.Test.Provider
{
    [TestClass]
    public class ResourceProviderTest
    {
        [TestMethod]
        public void ResourceProvider_Should_ConstructProperly()
        {
            Action action = () => new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            action.ShouldNotThrow("because it processed the metadata correctly");
        }

        [TestMethod]
        public void ResourceProvider_FolderExists_WorkWithEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.FolderExists("folder").Should().BeTrue();
            provider.FolderExists("flder").Should().BeFalse();
        }

        [TestMethod]
        public void ResourceProvider_FileExists_WorkWithEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.FileExists(@"folder\Class1.txt").Should().BeTrue();
            provider.FileExists(@"\folder\Class1.txt").Should().BeTrue();
            provider.FileExists(@"folder/Class1.txt").Should().BeTrue();
            provider.FileExists(@"/folder/Class1.txt").Should().BeTrue();
            provider.FileExists(@"folder.with.dots/Class1.js").Should().BeTrue();
            provider.FileExists(@"/folder.with.dots/Class1.js").Should().BeTrue();
            provider.FileExists(@"folder/with.dots/Class1.js").Should().BeFalse();
            provider.FileExists(@"/folder/with.dots/Class1.js").Should().BeFalse();
            provider.FileExists(@"Class1.css").Should().BeTrue();
            provider.FileExists(@"folder").Should().BeFalse();
        }

        [TestMethod]
        public void ResourceProvider_GetResourceFolder_WorkWithEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.GetResourceFolder("folder").Should().NotBeNull();
            provider.GetResourceFolder("flder").Should().BeNull();
        }

        [TestMethod]
        public void ResourceProvider_GetResourceFile_WorkWithEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.GetResourceFile(@"folder\Class1.txt").Should().NotBeNull();
            provider.GetResourceFile(@"\folder\Class1.txt").Should().NotBeNull();
            provider.GetResourceFile(@"folder/Class1.txt").Should().NotBeNull();
            provider.GetResourceFile(@"/folder/Class1.txt").Should().NotBeNull();
            provider.GetResourceFile(@"folder.with.dots/Class1.js").Should().NotBeNull();
            provider.GetResourceFile(@"/folder.with.dots/Class1.js").Should().NotBeNull();
            provider.GetResourceFile(@"folder/with.dots/Class1.js").Should().BeNull();
            provider.GetResourceFile(@"/folder/with.dots/Class1.js").Should().BeNull();
            provider.GetResourceFile(@"Class1.css").Should().NotBeNull();
            provider.GetResourceFile(@"folder").Should().BeNull();
        }

        [TestMethod]
        public void ResourceProvider_GetManifestResourceName_WorkWithEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.GetManifestResourceName(@"\folder\Class1.txt").Should().NotBeNullOrWhiteSpace();
            provider.GetManifestResourceName(@"folder/Class1.txt").Should().NotBeNullOrWhiteSpace();
            provider.GetManifestResourceName(@"/folder/Class1.txt").Should().NotBeNullOrWhiteSpace();
            provider.GetManifestResourceName(@"Class1.css").Should().NotBeNullOrWhiteSpace();
            provider.GetManifestResourceName(@"folder").Should().BeNullOrWhiteSpace();
        }

        [TestMethod]
        public void ResourceProvider_FolderExists_WorkWithTransposedEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.FolderExists("Content").Should().BeTrue();
            provider.FolderExists("Contet").Should().BeFalse();
        }

        [TestMethod]
        public void ResourceProvider_FileExists_WorkWithTransposedEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.FileExists(@"Content\Class1.txt").Should().BeTrue();
            provider.FileExists(@"\Content\Class1.txt").Should().BeTrue();
            provider.FileExists(@"Content/Class1.txt").Should().BeTrue();
            provider.FileExists(@"/Content/Class1.txt").Should().BeTrue();
            provider.FileExists(@"Scripts/Class1.js").Should().BeTrue();
            provider.FileExists(@"/Scripts/Class1.js").Should().BeTrue();
        }

        [TestMethod]
        public void ResourceProvider_GetResourceFolder_WorkWithTransposedEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.GetResourceFolder("Content").Should().NotBeNull();
            provider.GetResourceFolder("Scripts").Should().NotBeNull();
        }

        [TestMethod]
        public void ResourceProvider_GetResourceFile_WorkWithTransposedEmbeddedResources()
        {
            var provider = new Core.Provider.ResourceProvider(new[] { typeof(Anchor).Assembly });

            provider.GetResourceFile(@"folder\Class1.txt").Should().Be(provider.GetResourceFile(@"Content\Class1.txt"));
            provider.GetResourceFile(@"\folder\Class1.txt").Should().Be(provider.GetResourceFile(@"\Content\Class1.txt"));
            provider.GetResourceFile(@"folder/Class1.txt").Should().Be(provider.GetResourceFile(@"Content/Class1.txt"));
            provider.GetResourceFile(@"/folder/Class1.txt").Should().Be(provider.GetResourceFile(@"/Content/Class1.txt"));
            provider.GetResourceFile(@"folder.with.dots/Class1.js").Should().Be(provider.GetResourceFile(@"Scripts/Class1.js"));
            provider.GetResourceFile(@"/folder.with.dots/Class1.js").Should().Be(provider.GetResourceFile(@"/Scripts/Class1.js"));
        }

        [TestMethod]
        public void ResourceProvider_GetMostUniquePrefixStrings_WorksWithMultipleNamespaces()
        {
            var result = ResourceProvider.GetMostUniquePrefixStrings(
                new[]
                    {
                        "Blacklite.Website", "Blacklite.Website.Controllers", "Blacklite.Website.Views",
                        "Blacklite.Website.Views.ViewName",
                        "Science.Math",
                        "Science.Physics",
                        "Science.Bio",
                        "System",
                        "System.IO",
                        "A.B.C",
                        "A.B.C.D",
                        "A.B.C.E",
                        "A.B.C.D.F"
                    });

            result.Count().Should().Be(4);
            result.Any(z => z == "Blacklite.Website").Should().BeTrue();
            result.Any(z => z == "Science").Should().BeTrue();
            result.Any(z => z == "System").Should().BeTrue();
            result.Any(z => z == "A.B.C").Should().BeTrue();
        }
    }
}
