using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Services
{
    public class RoleFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");

            Console.WriteLine($"RoleFilter UserId: {userId}");

            if (!string.IsNullOrEmpty(userId))
            {
                var role = context.HttpContext.Session.GetString("Role");

                if (context.Controller is Controller controller)
                {
                    controller.ViewData["Role"] = role; 
                                                      
                    Console.WriteLine($"RoleFilter: Role set to {role}");
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
