using MT.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MT
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_BeginRequest()
        {
            if (!Context.Request.IsSecureConnection)
            {
                // This is an insecure connection, so redirect to the secure version
                UriBuilder uri = new UriBuilder(Context.Request.Url);
                uri.Scheme = "https";
                if (uri.Port > 32000 && uri.Host.Equals("localhost"))
                {
                    // Development box - set uri.Port to 44300 by default
                    uri.Port = 44300;
                }
                else
                {
                    uri.Port = 443;
                }

                Response.Redirect(uri.ToString());
            }
        }
    }
}
