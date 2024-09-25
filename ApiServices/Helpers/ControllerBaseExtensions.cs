using System.Net;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Helpers;

public static class ControllerBaseExtensions
{
	public static ObjectResult InternalError(this ControllerBase _, string? detail = default) =>
		new(new Response<string>(HttpStatusCode.InternalServerError, Detail: detail))
			{ StatusCode = 500 };

	public static OkObjectResult CustomOk(this ControllerBase controller, object? value) =>
		controller.Ok(new Response<object?>(HttpStatusCode.OK, value));

	public static UnauthorizedObjectResult CustomUnauthorized(
		this ControllerBase controller, object? value = default, string? detail = default) =>
		controller.Unauthorized(new Response<object?>(HttpStatusCode.Unauthorized, value, detail));

	public static NotFoundObjectResult CustomNotFound(
		this ControllerBase controller, object? value = default, string? detail = default) =>
		controller.NotFound(new Response<object?>(HttpStatusCode.NotFound, value, detail));

	public static BadRequestObjectResult CustomBadRequest(
		this ControllerBase controller, object? value = default, string? detail = default) =>
		controller.BadRequest(new Response<object?>(HttpStatusCode.BadRequest, value, detail));
}