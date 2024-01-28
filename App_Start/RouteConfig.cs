using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Assignment
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

          routes.MapRoute(
            name: "PropertyDetail",
            url: "Home/PropertyDetail/{id}",
            defaults: new { controller = "Home", action = "PropertyDetail" }
            );

          routes.MapRoute(
            name: "Cart",
            url: "Order/Cart/{id}",
            defaults: new { controller = "Home", action = "Cart", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ConfirmOrder",
                url: "Home/ConfirmOrder{id}",
                defaults: new { controller = "Home", action = "ConfirmOrder" }
            );

        }
    }
}
