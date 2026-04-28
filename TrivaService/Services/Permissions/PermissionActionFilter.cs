using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TrivaService.Services.Permissions
{
    public class PermissionActionFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> IgnoredControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Auth",
            "Home"
        };

        private readonly IPermissionService _permissionService;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public PermissionActionFilter(IPermissionService permissionService, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _permissionService = permissionService;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrWhiteSpace(controllerName)
                || string.IsNullOrWhiteSpace(actionName)
                || IgnoredControllers.Contains(controllerName))
            {
                await next();
                return;
            }

            var entityName = ResolveEntityName(controllerName);
            if (entityName is null)
            {
                await next();
                return;
            }

            var operation = ResolveOperation(actionName);
            var isAllowed = await _permissionService.HasEntityPermissionAsync(context.HttpContext.User, entityName, operation);
            if (!isAllowed)
            {
                var tempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext);
                tempData["DangerMessage"] = "Bu işleme yetkiniz yoksun.";

                var returnUrl = context.HttpContext.Request.Headers.Referer.ToString();
                if (!string.IsNullOrWhiteSpace(returnUrl)
                    && Uri.TryCreate(returnUrl, UriKind.Absolute, out var refererUri)
                    && string.Equals(refererUri.Host, context.HttpContext.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new RedirectResult(refererUri.PathAndQuery);
                    return;
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            if (context.Controller is Controller controller && operation is PermissionOperation.Read or PermissionOperation.Update or PermissionOperation.Create)
            {
                var readableProperties = await _permissionService.GetReadablePropertiesAsync(context.HttpContext.User, entityName);
                var writableProperties = await _permissionService.GetWritablePropertiesAsync(context.HttpContext.User, entityName);
                controller.ViewData["ReadableProperties"] = readableProperties;
                controller.ViewData["WritableProperties"] = writableProperties;
                controller.ViewData["EntityName"] = entityName;
            }

            await next();
        }

        private static string? ResolveEntityName(string controllerName)
        {
            var normalized = controllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                ? controllerName[..^10]
                : controllerName;

            return normalized switch
            {
                "Customers" => "Customer",
                "Services" => "Service",
                "ServiceItems" => "ServiceItem",
                "Items" => "Item",
                "Suppliers" => "Supplier",
                _ => normalized
            };
        }

        private static PermissionOperation ResolveOperation(string actionName)
        {
            return actionName switch
            {
                "Create" => PermissionOperation.Create,
                "Edit" => PermissionOperation.Update,
                "Delete" or "DeleteConfirmed" => PermissionOperation.Delete,
                _ => PermissionOperation.Read
            };
        }
    }
}
