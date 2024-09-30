using ApiServices.Helpers;
using ApiServices.Models.DataTransferObjects;
using ApiServices.Models.DataTransferObjects.ApiResponses;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController, Route("logs/funds")]
public class ActivityLogsController(ActivityLogService logs) : ControllerBase {
	
	/// <summary> Lists activity logs based on optional filters, page number, and limit. </summary>
	/// <param name="filter">Optional filter criteria for activity logs (can be null).</param>
	/// <param name="page">The page number (optional).</param>
	/// <param name="limit">The number of logs per page (optional).</param>
	[HttpPost]
	public async Task<ActionResult<Response<ActivityLogDto>>> List(
		[FromBody] ActivityLogFilterDto? filter,
		[FromQuery] int? page,
		[FromQuery] int? limit
	) {
		try {
			// Call the GetAll method to retrieve activity logs
			return this.CustomOk(await logs.GetAll(page ?? 0, limit ?? 10, filter));
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
}