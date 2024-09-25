using System.Net;
using ApiServices.Services;
using Common.Constants;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiServices.Decorators;

public class AuthorizeRoleAttribute : TypeFilterAttribute
{
	public AuthorizeRoleAttribute(params UserRole.Type[] requiredRoles) : base(typeof(AuthorizeFilter))
	{
		Arguments = [new HashSet<UserRole.Type>(requiredRoles)];
	}
}

public class AuthorizeFilter(AuthService authService, HashSet<UserRole.Type>? requiredRoles = null) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		if (requiredRoles == null) return;
		try
		{
			var (status, _,_) = await authService.Authorize(context.HttpContext, requiredRoles);
			if (status != HttpStatusCode.OK)
			{
				context.Result = new UnauthorizedObjectResult(new Response<string>(status,
					Detail: "Unauthorized (user attempting to register without administrator privileges)."));
			}
			else await next();
		}
		catch (Exception error)
		{
			context.Result = new ObjectResult(new Response<string>(HttpStatusCode.InternalServerError,
				Detail: error.Message)) { StatusCode = 500 };
		}
	}
}