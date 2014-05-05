using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

using RP.AspNet;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.ResourceProviderAspNetStart), "Start")]

namespace $rootnamespace$.App_Start {
    public static class ResourceProviderAspNetStart
    {
        public static void Start() {
            // Customize or disable virtual path provider from here if you need to generate an alternative.
            HostingEnvironment.RegisterVirtualPathProvider(new ResourceProviderVirtualPathProvider());
        }
    }
}
