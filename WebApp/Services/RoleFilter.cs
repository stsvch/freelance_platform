using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Services
{
    public class RoleFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");

            // Логирование
            Console.WriteLine($"RoleFilter invoked. UserId: {userId}");

            if (!string.IsNullOrEmpty(userId))
            {
                var role = context.HttpContext.Session.GetString("Role");

                // Приводим controller к типу Controller, чтобы получить доступ к ViewData
                if (context.Controller is Controller controller)
                {
                    controller.ViewData["Role"] = role; // Теперь можно использовать ViewData
                                                        // Логирование
                    Console.WriteLine($"RoleFilter: Role set to {role}");
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Можно оставить пустым, если нам не нужно выполнять действия после выполнения действия.
        }
    }

}
