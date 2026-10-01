using System;
using System.Web;
using System.Web.Hosting;
using MyKPPLearn.Data;

namespace MyKPPLearn
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            string appDataPath = HostingEnvironment.MapPath("~/App_Data");
            DatabaseInitializer.Initialize(appDataPath);
        }
    }
}
