using System.Net;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.DataTransferObjects.Filters;
using ApiServices.Helpers;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController]
[Route("logs")]
public class LogsController(ActivityLogService logs, AuthService auth) : ControllerBase {
	/// <summary> Lists activity logs based on optional filters, page number, and limit. </summary>
	/// <param name="filter">Optional filter criteria for activity logs (can be null).</param>
	/// <param name="page">The page number (optional).</param>
	/// <param name="size">The number of logs per page (optional).</param>
	[HttpPost("funds")]
	public async Task<ActionResult<BaseDto<PaginationDto<ActivityLogDto>>>> List([FromBody] ActivityLogFilter? filter,
		[FromQuery] int page = 0,
		[FromQuery] int size = 10) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext);
		if (validation != HttpStatusCode.OK) return this.CustomUnauthorized();

		try { return this.CustomOk(await logs.GetAll(page, size, filter, userSession)); } catch (Exception e) {
			return this.HandleErrors(e);
		}
	}
}