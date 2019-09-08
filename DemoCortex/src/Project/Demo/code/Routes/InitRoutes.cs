using System.Web.Http;
using Sitecore.Pipelines;

namespace Demo.Project.Demo.Routes
{
    public class InitRoutes
    {
        public virtual void Process(PipelineArgs args)
        {
            GlobalConfiguration.Configure(config =>
            {
                config.Routes.MapHttpRoute("SalesForecastApiRoute", "api/{controller}/{action}/", new
                {
                    controller = "ProductApi"
                });
            });
        }
    }
}