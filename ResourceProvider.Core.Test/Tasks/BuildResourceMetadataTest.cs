using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using RP.Core.Tasks;

namespace RP.Core.Test.Tasks
{
    [TestClass]
    public class BuildResourceMetadataTest
    {
        [TestMethod]
        public void BuildResourceMetadata_WorksWithNoKeys()
        {
            var task = new BuildResourceMetadata();
            task.RelativePath = @"E:\";

            var files =
                new[]
                    {
                        @"E:\A\B\File1.txt",
                        @"E:\A\C\File2.txt",
                        @"E:\A\File3.txt",
                    }.Select(x =>
                    {
                        var s = Substitute.For<ITaskItem>();
                        s.GetMetadata("Fullpath").Returns(x);
                        return s;
                    }).ToArray();

            task.Resources = files;

            var result = task.ProcessTask();

            result.Should().Be("{\"Files\":{\"A\\\\B\\\\File1.txt\":[\"A\\\\B\\\\File1.txt\"],\"A\\\\C\\\\File2.txt\":[\"A\\\\C\\\\File2.txt\"],\"A\\\\File3.txt\":[\"A\\\\File3.txt\"]},\"ProjectPath\":\"E:\\\\..\\\\\"}");
        }

        [TestMethod]
        public void BuildResourceMetadata_WorksWithKeys()
        {
            var task = new BuildResourceMetadata();
            task.RelativePath = @"E:\";

            task.Keys = @"A\B;A\C";
            task.Values = @"Scripts\A\B;thirdparty\src";

            var files =
                new[]
                    {
                        @"E:\A\B\File1.txt",
                        @"E:\A\C\File2.txt",
                        @"E:\A\File3.txt",
                    }.Select(x =>
                    {
                        var s = Substitute.For<ITaskItem>();
                        s.GetMetadata("Fullpath").Returns(x);
                        return s;
                    }).ToArray();

            task.Resources = files;

            var result = task.ProcessTask();

            result.Should().Be("{\"Files\":{\"A\\\\B\\\\File1.txt\":[\"Scripts\\\\A\\\\B\\\\File1.txt\",\"A\\\\B\\\\File1.txt\"],\"A\\\\C\\\\File2.txt\":[\"thirdparty\\\\src\\\\File2.txt\",\"A\\\\C\\\\File2.txt\"],\"A\\\\File3.txt\":[\"A\\\\File3.txt\"]},\"ProjectPath\":\"E:\\\\..\\\\\"}");
        }

        [TestMethod]
        public void BuildResourceMetadata_WorksWithKeys2()
        {
            var task = new BuildResourceMetadata();
            task.RelativePath = @"E:\";

            task.Keys = @"\A\B;\A\C";
            task.Values = @"Scripts\A\B;thirdparty\src";

            var files =
                new[]
                    {
                        @"E:\A\B\File1.txt",
                        @"E:\A\C\File2.txt",
                        @"E:\A\File3.txt",
                    }.Select(x =>
                    {
                        var s = Substitute.For<ITaskItem>();
                        s.GetMetadata("Fullpath").Returns(x);
                        return s;
                    }).ToArray();

            task.Resources = files;

            var result = task.ProcessTask();

            result.Should().Be("{\"Files\":{\"A\\\\B\\\\File1.txt\":[\"Scripts\\\\A\\\\B\\\\File1.txt\",\"A\\\\B\\\\File1.txt\"],\"A\\\\C\\\\File2.txt\":[\"thirdparty\\\\src\\\\File2.txt\",\"A\\\\C\\\\File2.txt\"],\"A\\\\File3.txt\":[\"A\\\\File3.txt\"]},\"ProjectPath\":\"E:\\\\..\\\\\"}");
        }
    }
}
