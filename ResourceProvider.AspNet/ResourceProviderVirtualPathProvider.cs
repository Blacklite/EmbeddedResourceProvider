using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

using RP.AspNet.FileSystem;
using RP.Core.Provider;

namespace RP.AspNet
{
    /// <summary>
    /// This class can be used by product UI.Resource projects to reference their custom scripts.
    /// 
    /// The GetRelativeResourcePath may be overridden if the transpose paths convention will not meet the needs of the provider.
    /// 
    /// The intent is for the virtual path to be converted to a path, that will then be found by the ResourceProvider
    /// </summary>
    public class ResourceProviderVirtualPathProvider : VirtualPathProvider
    {
        private readonly IResourceProvider _resourceProvider;

        public ResourceProviderVirtualPathProvider()
            : this(new ResourceProvider(AppDomain.CurrentDomain.GetAssemblies()))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceProviderVirtualPathProvider"/> class.
        /// </summary>
        /// <param name="resourceProvider">The embedded resource provider.</param>
        public ResourceProviderVirtualPathProvider(IResourceProvider resourceProvider)
        {
            this._resourceProvider = resourceProvider;
        }

        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns></returns>
        public override bool FileExists(string virtualPath)
        {
            var embeddedPath = PathUtils.GetAbsolutePath(virtualPath);
            var resourceExists = this._resourceProvider.FileExists(embeddedPath);

            var result = (resourceExists);

            return result;
        }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns></returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            var embeddedPath = PathUtils.GetAbsolutePath(virtualPath);
            var exists = this._resourceProvider.FileExists(embeddedPath);

            if (exists)
            {
                var file = this._resourceProvider.GetResourceFile(virtualPath);
                if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled)
                {
                    var filePath = this.FileExists(embeddedPath, file.ProjectPath);
                    if (filePath != null)
                    {
                        return new PhysicalFile(virtualPath, new FileInfo(filePath));
                    }
                }

                return new EmbeddedFile(virtualPath, file);
            }

            return null;
        }

        private static object _syncLock = new object();
        private static string hostMapPath = null;

        private static new ConcurrentDictionary<string, string> _fileExistsCache = new ConcurrentDictionary<string, string>();

        private string FileExists(string virtualPath, string relativeDirectoryPath)
        {
            if (hostMapPath == null)
            {
                lock (_syncLock)
                {
                    hostMapPath = HostingEnvironment.MapPath("~/");
                }
            }

            return _fileExistsCache.GetOrAdd(
                virtualPath, x =>
                {
                    string result = null;
                    var path = relativeDirectoryPath;
                    var partialPath = String.Empty;
                    var newVirtualPath = virtualPath.TrimStart('~');

                    while (path.IndexOf('\\') > -1)
                    {
                        var indexOf = path.LastIndexOf('\\');
                        partialPath = @"..\" + partialPath + @"\" + path.Substring(indexOf + 1);
                        path = path.Substring(0, indexOf + 1);

                        var temp = Path.GetFullPath(hostMapPath + path + newVirtualPath);
                        if (File.Exists(temp))
                        {
                            result = temp;
                            break;
                        }
                    }

                    return result;
                });
        }

        /// <summary>
        /// Gets the cache dependency.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="virtualPathDependencies">The virtual path dependencies.</param>
        /// <param name="utcStart">The UTC start.</param>
        /// <returns></returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            var fileNames = new string[] { };
            if (virtualPathDependencies != null)
            {
                fileNames = fileNames.Union(virtualPathDependencies.Cast<string>()
                    .Select(SimpleMapPath)).ToArray();
            }

            return new CacheDependency(fileNames, utcStart);
        }

        static string SimpleMapPath(string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath))
                throw new ArgumentNullException("virtualPath");

            if (virtualPath.StartsWith("~", StringComparison.OrdinalIgnoreCase))
            {
                var path = virtualPath.Replace("~/", "")
                    .Replace('/', '\\');

                var hostMapPath = HostingEnvironment.MapPath("~/");
                return Path.Combine(hostMapPath, path);
            }
            else throw new Exception("virtualPath must start with ~");
        }

        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="virtualDir">The virtual dir.</param>
        /// <returns></returns>
        public override bool DirectoryExists(string virtualDir)
        {
            var embeddedDir = PathUtils.GetAbsolutePath(virtualDir);
            if (this._resourceProvider.FolderExists(embeddedDir))
                return true;

            return false;
        }

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <param name="virtualDir">The virtual dir.</param>
        /// <returns></returns>
        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var embeddedDir = PathUtils.GetAbsolutePath(virtualDir);
            if (this._resourceProvider.FolderExists(embeddedDir))
                return new MergedDirectory(virtualDir, embeddedDir, this._resourceProvider);

            return null;
        }
    }
}
