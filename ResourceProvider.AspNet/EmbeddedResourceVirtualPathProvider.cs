using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using log4net;
using Upside.IoC;
using Upside.UI.EmbeddedResource;

namespace Upside.UI.Mvc.Util.VirtualPathProviders
{
    [NamedImplementation(typeof(IVirtualPathProvider), "PlatformEmbeddedResourceVirtualPathProvider")]
    class PlatformEmbeddedResourceVirtualPathProvider : IVirtualPathProvider
    {
        private readonly IEmbeddedResourceProvider _embeddedResourceProvider;

        private const string LOGGER_NAME = "General";
        private readonly ILog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformEmbeddedResourceVirtualPathProvider" /> class.
        /// </summary>
        /// <param name="embeddedResourceProvider">The embedded resource provider.</param>
        public PlatformEmbeddedResourceVirtualPathProvider(IEmbeddedResourceProvider embeddedResourceProvider)
        {
            _embeddedResourceProvider = embeddedResourceProvider;
            this.logger = log4net.LogManager.GetLogger(LOGGER_NAME);
        }

        private static bool FileExistsInFileSystem(string embeddedPath)
        {
            var mappedPath = PathUtils.MapPath("~" + embeddedPath);
            return DirectoryUtil.FileExists(mappedPath);
        }

        /// <summary>
        /// Determines whether the specified virtual path is valid.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>
        ///   <c>true</c> if the specified virtual path is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsValid(string virtualPath)
        {
            var embeddedPath = _embeddedResourceProvider.GetEmbeddedAbsolutePath(virtualPath);
            var fileExistsInFileSystem = FileExistsInFileSystem(embeddedPath);
            var resourceExists = _embeddedResourceProvider.ResourceExists(embeddedPath) || _embeddedResourceProvider.FolderExists(embeddedPath);

            var result = (!fileExistsInFileSystem && resourceExists);
            this.logger.DebugFormat("PlatformEmbeddedResourceVirtualPathProvider.IsValid: virtualPath \"{0}\" result \"{1}\".", virtualPath, result);

            return result;
        }

        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "tenantVirtualPathOrDefault")]
        public bool FileExists(string virtualPath)
        {
            var embeddedPath = _embeddedResourceProvider.GetEmbeddedAbsolutePath(virtualPath);
            var fileExistsInFileSystem = FileExistsInFileSystem(embeddedPath);
            var resourceExists = _embeddedResourceProvider.ResourceExists(embeddedPath);

            var result = (!fileExistsInFileSystem && resourceExists);
            this.logger.DebugFormat("PlatformEmbeddedResourceVirtualPathProvider.FileExists: virtualPath \"{0}\" result \"{1}\".", virtualPath, result);

            return result;
        }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns></returns>
        public VirtualFile GetFile(string virtualPath)
        {
            var embeddedPath = _embeddedResourceProvider.GetEmbeddedAbsolutePath(virtualPath);
            var fileExistsInFileSystem = FileExistsInFileSystem(embeddedPath);
            var resourceExists = _embeddedResourceProvider.ResourceExists(embeddedPath);

            // If the file exists locally use it instead.
            var exists = (!fileExistsInFileSystem && resourceExists);
            this.logger.DebugFormat("PlatformEmbeddedResourceVirtualPathProvider.GetFile: virtualPath \"{0}\" exists \"{1}\".", virtualPath, exists);

            if (exists)
            {
                return new EmbeddedResourceVirtualFile(embeddedPath, _embeddedResourceProvider.GetResourceFile(virtualPath));
            }

            return null;
        }

        /// <summary>
        /// Gets the cache dependency.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="virtualPathDependencies">The virtual path dependencies.</param>
        /// <param name="utcStart">The UTC start.</param>
        /// <returns></returns>
        public CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            var fileNames = new string[] { };
            if (virtualPathDependencies != null)
            {
                fileNames = fileNames.Union(virtualPathDependencies.Cast<string>()
                    .Select(x => PlatformRazorViewEngine.SimpleMapPath(x))
                    ).ToArray();
            }

            return new CacheDependency(fileNames, utcStart);
        }

        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="virtualDir">The virtual dir.</param>
        /// <returns></returns>
        public bool DirectoryExists(string virtualDir)
        {
            //var embeddedDir = _embeddedResourceProvider.GetEmbeddedAbsolutePath(virtualDir);
            if (_embeddedResourceProvider.FolderExists(virtualDir))
                return true;

            return false;
        }

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <param name="virtualDir">The virtual dir.</param>
        /// <returns></returns>
        public VirtualDirectory GetDirectory(string virtualDir)
        {
            var embeddedDir = _embeddedResourceProvider.GetEmbeddedAbsolutePath(virtualDir);
            if (_embeddedResourceProvider.FolderExists(embeddedDir))
                return new EmbeddedResourceVirtualDirectory(virtualDir, _embeddedResourceProvider);

            return null;
        }

        /// <summary>
        /// Gets the provider priority.
        /// </summary>
        /// <value>
        /// The provider priority.
        /// </value>
        public int ProviderPriority
        {
            get
            {
                return 50;
            }
        }
    }

    internal class EmbeddedResourceVirtualFile : VirtualFile
    {
        private IEmbeddedResourceFile _embeddedResourceFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceVirtualFile" /> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        /// <param name="embeddedResourceFile">The embedded resource file.</param>
        public EmbeddedResourceVirtualFile(string virtualPath, IEmbeddedResourceFile embeddedResourceFile)
            : base(virtualPath.StartsWith("~", StringComparison.OrdinalIgnoreCase) ? virtualPath : "~" + virtualPath)
        {
            _embeddedResourceFile = embeddedResourceFile;
        }

        public override System.IO.Stream Open()
        {
            return _embeddedResourceFile.GetResourceStream();
        }
    }

    internal class EmbeddedResourceVirtualDirectory : VirtualDirectory
    {
        IEnumerable<VirtualFile> _virtualFiles;
        IEnumerable<VirtualDirectory> _virtualDirectories;
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceVirtualDirectory" /> class.
        /// </summary>
        /// <param name="virtualDir">The virtual dir.</param>
        /// <param name="embeddedResourceProvider">The embedded resource provider.</param>
        public EmbeddedResourceVirtualDirectory(string virtualDir, IEmbeddedResourceProvider embeddedResourceProvider)
            : base(virtualDir)
        {
            // Can't make any assumptions on original directory structure...
            // embedded resources will always load everything that matchs this path, and all diretories under it.
            var item = embeddedResourceProvider.GetResourceFolder(virtualDir);

            if (item != null)
            {
                var embeddedFiles = item.Files;
                var embeddedFolders = item.Folders;

                _virtualFiles = embeddedFiles
                        .Where(x => x.VirtualPath != null)
                        .Select(x => new EmbeddedResourceVirtualFile(x.VirtualPath, x))
                        .OfType<VirtualFile>();

                _virtualDirectories = embeddedFolders
                    .Select(x => new EmbeddedResourceVirtualDirectory(x.VirtualPath, embeddedResourceProvider));

                var physicalPath = PathUtils.MapPath(virtualDir);
                if (DirectoryUtil.DirectoryExists(physicalPath))
                {
                    var directoryInfo = DirectoryUtil.GetDirectoryInfo(physicalPath);

                    var realFiles = directoryInfo.EnumerateFiles();
                    var physicalFiles = realFiles.Select(x => new PhysicalResourceVirtualFile(PathUtils.VirtualPath(x.FullName), DirectoryUtil.GetFileInfo(x.FullName)));

                    _virtualFiles = _virtualFiles
                        .Where(x => !physicalFiles.Any(z => x.VirtualPath.Equals(z.VirtualPath, StringComparison.OrdinalIgnoreCase)))
                        .Union(physicalFiles.OfType<VirtualFile>())
                        .OrderBy(x => x.VirtualPath);

                    var enumerateDirectories = directoryInfo.EnumerateDirectories();
                    var realDirectories = enumerateDirectories
                        .Select(x => PathUtils.VirtualPath(x.FullName))
                        .Except(embeddedFolders.Select(x => x.VirtualPath));

                    _virtualDirectories = _virtualDirectories.Union(realDirectories
                        .Select(x => new EmbeddedResourceVirtualDirectory(x, embeddedResourceProvider)))
                        .OrderBy(x => x.VirtualPath);
                }
            }
        }

        private ArrayList children;
        public override System.Collections.IEnumerable Children
        {
            get
            {
                if (children == null)
                {
                    children = new ArrayList();

                    children.AddRange(_virtualDirectories.ToArray());
                    children.AddRange(_virtualFiles.ToArray());
                }
                return children;
            }
        }

        private ArrayList directories;
        public override System.Collections.IEnumerable Directories
        {
            get
            {
                if (directories == null)
                {
                    directories = new ArrayList();
                    var virtualDirectories = _virtualDirectories.ToArray();
                    directories.AddRange(virtualDirectories);
                }
                return directories;
            }
        }

        private ArrayList files;
        public override System.Collections.IEnumerable Files
        {
            get
            {
                if (files == null)
                {
                    files = new ArrayList();
                    files.AddRange(_virtualFiles.ToArray());
                }
                return files;
            }
        }
    }

    internal class PhysicalResourceVirtualFile : VirtualFile
    {
        private readonly FileInfo _fileInfo;
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalResourceVirtualFile" /> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        /// <param name="fileInfo">The file info.</param>
        public PhysicalResourceVirtualFile(string virtualPath, FileInfo fileInfo)
            : base(virtualPath.StartsWith("~", StringComparison.OrdinalIgnoreCase) ? virtualPath : "~" + virtualPath)
        {
            _fileInfo = fileInfo;
        }

        public override System.IO.Stream Open()
        {
            return File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}