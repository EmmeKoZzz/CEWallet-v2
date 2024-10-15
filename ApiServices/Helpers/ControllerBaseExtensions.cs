using System.Net;
using ApiServices.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ApiServices.Helpers;

public static class ControllerBaseExtensions {
	public static ObjectResult InternalError(this ControllerBase _, object? value = default, string? detail = default) =>
		new(new BaseDto<object>(HttpStatusCode.InternalServerError, value, Detail: detail)) { StatusCode = 500 };

	public static OkObjectResult CustomOk(this ControllerBase controller, object? value) =>
		controller.Ok(new BaseDto<object?>(HttpStatusCode.OK, value));

	public static UnauthorizedObjectResult CustomUnauthorized(
	this ControllerBase controller,
	object? value = default,
	string? detail = default
	) => controller.Unauthorized(new BaseDto<object?>(HttpStatusCode.Unauthorized, value, detail));

	public static NotFoundObjectResult CustomNotFound(this ControllerBase controller, object? value = default,
	string? detail = default) =>
		controller.NotFound(new BaseDto<object?>(HttpStatusCode.NotFound, value, detail));

	public static BadRequestObjectResult CustomBadRequest(this ControllerBase controller, object? value = default,
	string? detail = default) =>
		controller.BadRequest(new BaseDto<object?>(HttpStatusCode.BadRequest, value, detail));

	public static ObjectResult HandleErrors(this ControllerBase controller, Exception e) {
		return e.InnerException is MySqlException error
			? controller.InternalError(new { error.ErrorCode, error.SqlState, error.Message }, error.Message)
			: controller.InternalError(detail: e.Message);
	}
}