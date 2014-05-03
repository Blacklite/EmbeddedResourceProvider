using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;


using RP.Core.FileSystem;
using RP.Test.Example;

namespace RP.Core.Test.FileSystem
{
    [TestClass]
    public class ResourceFolderTest
    {
        [TestMethod]
        public void ResourceFolder_Should_AddFiles()
        {
            var folder = new ResourceFolder();

            var file = Substitute.For<IResourceFile>();
            folder.AddFile(file);

            folder.Files.Count().Should().Be(1);

            file = Substitute.For<IResourceFile>();
            folder.AddFile(file);

            folder.Files.Count().Should().Be(2);
        }

        [TestMethod]
        public void ResourceFolder_Should_AddFolders()
        {
            var folder = new ResourceFolder();

            var newFolder = Substitute.For<IResourceFolder>();
            folder.AddFolder(newFolder);

            folder.Folders.Count().Should().Be(1);

            newFolder = Substitute.For<IResourceFolder>();
            folder.AddFolder(newFolder);

            folder.Folders.Count().Should().Be(2);
        }

        [TestMethod]
        public void ResourceFolder_Should_FindFolders()
        {
            // A/B/C
            var a = new ResourceFolder() { Name= "A" };
            var b = new ResourceFolder() { Name = "B" };
            var c = new ResourceFolder() { Name = "C" };
            var d = new ResourceFolder() { Name = "D" };
            var e = new ResourceFolder() { Name = "E" };
            var f = new ResourceFolder() { Name = "F" };
            var g = new ResourceFolder() { Name = "G" };
            var h = new ResourceFolder() { Name = "H" };
            var i = new ResourceFolder() { Name = "I" };
            a.AddFolder(b);
            b.AddFolder(c);
            b.AddFolder(d);
            c.AddFolder(e);
            c.AddFolder(f);
            e.AddFolder(g);
            f.AddFolder(h);
            g.AddFolder(i);

            var found = a.FindFolder(@"B\C\F\H");

            found.Should().Be(h);

            found = a.FindFolder(@"B/C/F\H");

            found.Should().Be(h);

            found = a.FindFolder(@"B/C/F/H");

            found.Should().Be(h);

        }
    }
}
