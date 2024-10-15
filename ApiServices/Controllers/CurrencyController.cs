using System.Net;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController, Route("/currency")]
public class CurrencyController(
	ActivityLogService logService,
	AuthService auth,
	CurrencyService currency,
	AppDbContext dbContext)
	: ControllerBase {
	/// <summary> Retrieves a list of all currencies. </summary>
	/// <param name="funds">Optional parameter indicating whether to include related Fund information.</param>
	/// <response code="200"> List of CurrencyDto objects representing retrieved currencies. </response>
	/// <response code="400"> Bad request (e.g., invalid data in request body). </response>
	/// <response code="401"> Unauthorized (missing or invalid authorization token). </response>
	[HttpGet, AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<IEnumerable<CurrencyDto>>>> GetAll([FromQuery] bool funds = false) {
		try {
			return this.CustomOk(await currency.GetAll(funds));
		}
		catch (Exception e) {
			return this.HandleErrors(e);
		}
	}

	/// <summary>Adds a new currency.</summary>
	/// <param name="info">The details of the currency to be added.</param>
	/// <response code="200">Currency added successfully.</response>
	/// <response code="401">Unauthorized (missing or invalid authorization token).</response>
	[HttpPost, AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<CurrencyDto>>> Add(AddCurrencyDto info) {
		try {
			var res = await currency.Add(info);
			await dbContext.SaveChangesAsync();

			return res.Status switch {
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
				HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		}
		catch (Exception e) {
			return this.HandleErrors(e);
		}
	}

	/// <summary> Updates an existing currency. </summary>
	/// <param name="info">The details of the currency to be updated.</param>
	/// <param name="id">ID of the currency.</param>
	/// <response code="200">Currency updated successfully.</response>
	/// <response code="401">Unauthorized (missing or invalid authorization token).</response>
	/// <response code="404">Currency not found.</response>
	[HttpPut("{id:guid}"), AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<CurrencyDto>>> Update([FromBody] AddCurrencyDto info, [FromRoute] Guid id) {
		try {
			var (status, res, _) = await currency.Update(info, id);
			await dbContext.SaveChangesAsync();

			return status switch {
				HttpStatusCode.OK => this.CustomOk(res),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "Currency not found.")
			};
		}
		catch (Exception e) {
			return this.HandleErrors(e);
		}
	}

	/// <summary> Deletes a currency. </summary>
	/// <param name="id">The unique identifier of the currency to be deleted.</param>
	/// <response code="200">Currency deleted successfully.</response>
	/// <response code="401">Unauthorized (missing or invalid authorization token).</response>
	/// <response code="404">Currency not found.</response>
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<BaseDto<CurrencyDto>>> Delete([FromRoute] Guid id) {
		var validation = await auth.Authorize(HttpContext, [UserRole.Type.Administrator]);

		if (validation.Status is not HttpStatusCode.OK) return this.CustomUnauthorized(detail: validation.Message);

		var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var (status, (currencyData, fundData), message) = await currency.Delete(id);

			if (status is not HttpStatusCode.OK) {
				return status switch {
					HttpStatusCode.NotFound => this.CustomNotFound("Currency not found."),
					HttpStatusCode.InternalServerError => this.InternalError(message)
				};
			}

			var logTasks = fundData.Select(
				data => logService.Log(
					FundActivity.Type.DeleteCurrency,
					data.Item1, // Fund ID 
					validation.Value!.Id,
					FundTransaction.Type.Withdrawal,
					data.Item2 // Currency Amount
				)
			);

			await Task.WhenAll(logTasks);

			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();

			return this.CustomOk(currencyData);
		}
		catch (Exception e) {
			await trx.RollbackAsync();

			return this.HandleErrors(e);
		}
	}

	/// <summary>Gets the informal foreign exchange rates.</summary>
	/// <response code="200">Returns a list of informal foreign exchange rates.</response>
	[HttpGet("informal-foreign-exchange")]
	public async Task<ActionResult<BaseDto<Dictionary<string, float>>>> Test() {
		try {
			return this.CustomOk(await CurrencyService.InformalForeignExchange());
		}
		catch (Exception e) {
			return this.HandleErrors(e);
		}
	}
}