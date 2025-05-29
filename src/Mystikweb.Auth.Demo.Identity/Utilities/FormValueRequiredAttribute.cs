using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Mystikweb.Auth.Demo.Identity.Utilities;

public sealed class FormValueRequiredAttribute(string name) : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
    {
        return !string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(context.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(context.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(context.HttpContext.Request.ContentType) &&
            context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(context.HttpContext.Request.Form[name]);
    }
}
