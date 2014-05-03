using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;


using RP.Core.FileSystem;
using RP.Test.Example;

namespace RP.Core.Test.FileSystem
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ResourceFileTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ResourceFile_GetResource_ShouldGetResourceStream()
        {
            var anchorAssembly = typeof(Anchor).Assembly;
            var file = new ResourceFile()
            {
                Assembly = anchorAssembly,
                ResourcePath = "ResourceProvider.Test.Example.folder.Class1.txt"
            };

            Action action = (() => file.GetResourceStream());
            action.ShouldNotThrow("It should return the resource stream");
        }

        [TestMethod]
        public void ResourceFile_GetResource_ShouldGetResourceString()
        {
            var anchorAssembly = typeof(Anchor).Assembly;
            var file = new ResourceFile()
            {
                Assembly = anchorAssembly,
                ResourcePath = "RP.Test.Example.folder.Class1.txt"
            };
            
            Action action = (() => file.GetResourceString());
            action.ShouldNotThrow("It should return the resource string");

            var result = file.GetResourceString();
            result.Should().NotBeNullOrWhiteSpace("The file should have contents from the stream");
        }
    }
}
