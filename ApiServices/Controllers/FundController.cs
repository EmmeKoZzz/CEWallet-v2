using System.Net;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.DataTransferObjects.Filters;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController, Route("/fund")]
public class FundController(AppDbContext dbContext, FundService funds, ActivityLogService logs, AuthService auth) : ControllerBase {
	
	/// <summary> Retrieves all funds. Only administrators and Supervisors are allowed to perform this action.</summary>
	/// <response code="200">Successful retrieval of funds.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpPost, AuthorizeRole(UserRole.Type.Administrator, UserRole.Type.Supervisor)]
	public async Task<ActionResult<BaseDto<FundDto[]>>> GetAll(
		[FromQuery] int page = 0,
		[FromQuery] int size = 10,
		[FromBody] FundFilter? filter = default
	) {
		try { return this.CustomOk(await funds.GetAllDev(page, size, filter)); } catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Retrieves a specific fund by its ID. </summary>
	/// <param name="id">The ID of the fund to retrieve.</param>
	/// <response code="200">Successful retrieval of the fund.</response>
	/// <response code="401">Unauthorized access.</response>
	/// <response code="404">Fund not found.</response>
	[HttpGet("{id:guid}"), AuthorizeRole]
	public async Task<ActionResult<BaseDto<FundDto>>> Get([FromRoute] Guid id) {
		try {
			var res = await funds.Get(id);
			
			return res.Status switch {
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message), HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Retrieves funds associated with a specific user. </summary>
	/// <param name="id">The ID of the user whose funds to retrieve.</param>
	/// <response code="200">Successful retrieval of the user's funds.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpGet("user/{id:guid}"), AuthorizeRole]
	public async Task<ActionResult<BaseDto<FundDto[]>>> GetUserFunds([FromRoute] Guid id) {
		try { return this.CustomOk(await funds.GetByUser(id)); } catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Adds a new fund. Only administrators are allowed to perform this action.</summary>
	/// <param name="info">The information for the new fund.</param>
	/// <response code="200">Successful addition of the fund.</response>
	/// <response code="400">Invalid fund information.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpPost("create")]
	public async Task<ActionResult<BaseDto<FundDto>>> Add([FromBody] AddFundDto info) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext, [UserRole.Type.Administrator]);
		
		if (validation is not HttpStatusCode.OK)
			return this.CustomUnauthorized("user attempting to register without administrator privileges).");
		
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var res = await funds.Add(info);
			await logs.Log(FundActivity.Type.CreateFund, res.Id, userSession!.Id, details: info.Details);
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			
			return this.CustomOk(res);
		} catch (Exception e) {
			await trx.RollbackAsync();
			
			return this.InternalError(e.Message);
		}
	}
	
	/// <summary> Updates an existing fund. Only administrators are allowed to perform this action.</summary>
	/// <param name="info">The updated information for the fund.</param>
	/// <param name="id">The ID of the fund to update.</param>
	/// <response code="200">Successful update of the fund.</response>
	/// <response code="400">Invalid fund information.</response>
	/// <response code="401">Unauthorized access.</response>
	/// <response code="404">Fund not found.</response>
	[HttpPatch("{id:guid}"), AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<FundDto>>> Update([FromBody] AddFundDto info, [FromRoute] Guid id) {
		try {
			var res = await funds.Update(info, id);
			await dbContext.SaveChangesAsync();
			
			return res.Status switch {
				HttpStatusCode.OK => this.CustomOk(res.Value), HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Transfers funds between accounts. Only administrators and Supervisors are allowed to perform this action.</summary>
	/// <param name="info">The transfer request details, including source and destination accounts, amount, and optional notes.</param>
	/// <response code="200">The transfer was successful.</response>
	/// <response code="400">The transfer request is invalid or missing required fields.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">The source or destination account does not exist.</response>
	[HttpPost("transfer")]
	public async Task<ActionResult<BaseDto<TransferDto.Response>>> Transfer([FromBody] TransferDto info) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext, [UserRole.Type.Administrator, UserRole.Type.Supervisor]);
		
		if (validation is not HttpStatusCode.OK)
			return this.CustomUnauthorized("user attempting to register without administrator privileges).");
		
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var res = await funds.Transfer(info);
			
			if (res.Status is not HttpStatusCode.OK)
				return res.Status switch {
					HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
					HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
				};
			
			await logs.Log(
				FundActivity.Type.Transfer,
				res.Value!.Source.Id,
				userSession!.Id,
				FundTransaction.Type.Withdrawal,
				currency: info.Currency,
				amount: info.Amount,
				details: info.Details
			);
			
			await logs.Log(
				FundActivity.Type.Transfer,
				res.Value!.Destination.Id,
				userSession.Id,
				FundTransaction.Type.Deposit,
				currency: info.Currency,
				amount: info.Amount,
				details: info.Details
			);
			
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			
			return this.CustomOk(res.Value);
		} catch (Exception e) {
			await trx.RollbackAsync();
			
			return this.InternalError(e.Message);
		}
	}
	
	/// <summary> Withdraws funds from a specified account. </summary>
	/// <param name="info">The withdrawal information.</param>
	/// <response code="200">Successful withdrawal.</response>
	/// <response code="400">Invalid withdrawal request.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">Account not found.</response>
	[HttpPost("withdrawal")]
	public async Task<ActionResult<BaseDto<FundDto>>> Withdraw([FromBody] TransactionDto info) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext);
		
		if (validation is not HttpStatusCode.OK)
			return this.CustomUnauthorized("user attempting to register without administrator privileges).");
		
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var res = await funds.Withdraw(info);
			
			if (res.Status is not HttpStatusCode.OK)
				return res.Status switch {
					HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
					HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
				};
			
			await logs.Log(
				FundActivity.Type.Withdrawal,
				res.Value!.Id,
				userSession!.Id,
				currency: info.Currency,
				transactionType: FundTransaction.Type.Withdrawal,
				amount: info.Amount,
				details: info.Details
			);
			
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			
			return this.CustomOk(res.Value);
		} catch (Exception e) {
			await trx.RollbackAsync();
			
			return this.InternalError(e.Message);
		}
	}
	
	/// <summary>Deposits funds into a specified account. Only administrators are allowed to perform this action.</summary>
	/// <param name="info">The deposit information.</param>
	/// <response code="200">Successful deposit.</response>
	/// <response code="400">Invalid deposit request.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">Account not found.</response>
	[HttpPost("deposit")]
	public async Task<ActionResult<BaseDto<FundDto>>> Deposit([FromBody] TransactionDto info) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext, [UserRole.Type.Administrator]);
		
		if (validation is not HttpStatusCode.OK)
			return this.CustomUnauthorized("user attempting to register without administrator privileges).");
		
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var res = await funds.Deposit(info);
			
			if (res.Status is not HttpStatusCode.OK)
				return res.Status switch {
					HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
					HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
				};
			
			await logs.Log(
				FundActivity.Type.Deposit,
				res.Value!.Id,
				userSession!.Id,
				currency: info.Currency,
				transactionType: FundTransaction.Type.Deposit,
				amount: info.Amount,
				details: info.Details
			);
			
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			
			return this.CustomOk(res.Value);
		} catch (Exception e) {
			await trx.RollbackAsync();
			
			return this.InternalError(e.Message);
		}
	}
	
	/// <summary>Attaches a user to a specified fund. Only administrators are allowed to perform this action.</summary>
	/// <param name="fundId">The ID of the fund.</param>
	/// <param name="userId">The ID of the user.</param>
	/// <response code="200">User attached successfully.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">Fund or user not found.</response>
	[HttpPatch("attach-user/{fundId:guid}/{userId:guid}"), AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<object>>> AddUser(Guid fundId, Guid userId) {
		try {
			var res = await funds.AttachUser(userId, fundId);
			await dbContext.SaveChangesAsync();
			
			return res.Status switch {
				HttpStatusCode.OK => this.CustomOk(res.Value), HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary>Deletes a fund by its unique identifier. Only administrators are allowed to perform this action.</summary>  
	/// <param name="id">The unique identifier of the fund to be deleted.</param>  
	/// <response code="200">The fund is successfully deleted.</response>  
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">No fund is found with the specified identifier.</response>  
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<BaseDto<object>>> Delete([FromRoute] Guid id) {
		var (validation, userSession, _) = await auth.Authorize(HttpContext, [UserRole.Type.Administrator]);
		
		if (validation is not HttpStatusCode.OK)
			return this.CustomUnauthorized("user attempting to register without administrator privileges).");
		
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var res = await funds.Delete(id);
			
			if (res.Status is HttpStatusCode.NotFound) return this.CustomNotFound(detail: res.Message);
			await logs.Log(FundActivity.Type.DeleteFund, res.Value!.Id, userSession!.Id);
			
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			
			return this.CustomOk(res.Value);
		} catch (Exception e) {
			await trx.RollbackAsync();
			
			return this.InternalError(e.Message);
		}
		
	}
	
}