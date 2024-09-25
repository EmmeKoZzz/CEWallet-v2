using System.Net;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Services;
using Common.Constants;
using Common.DataTransferObjects;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController]
[Route("/fund")]
public class FundController(FundService funds) : ControllerBase
{
	/// <summary> Retrieves all funds. </summary>
	/// <response code="200">Successful retrieval of funds.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpGet]
	[AuthorizeRole(UserRole.Type.Administrator, UserRole.Type.Supervisor)]
	public async Task<ActionResult<Response<FundDto[]>>> GetAll()
	{
		try
		{
			return this.CustomOk(await funds.GetAll());
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Retrieves a specific fund by its ID. </summary>
	/// <param name="id">The ID of the fund to retrieve.</param>
	/// <response code="200">Successful retrieval of the fund.</response>
	/// <response code="401">Unauthorized access.</response>
	/// <response code="404">Fund not found.</response>
	[HttpGet("{id:guid}")]
	[AuthorizeRole]
	public async Task<ActionResult<Response<FundDto>>> Get([FromRoute] Guid id)
	{
		try
		{
			var res = await funds.Get(id);
			return res.Status switch
			{
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message),
				HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Retrieves funds associated with a specific user. </summary>
	/// <param name="id">The ID of the user whose funds to retrieve.</param>
	/// <response code="200">Successful retrieval of the user's funds.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpGet("user/{id:guid}")]
	[AuthorizeRole]
	public async Task<ActionResult<Response<FundDto[]>>> GetUserFunds([FromRoute] Guid id)
	{
		try
		{
			return this.CustomOk(await funds.GetByUser(id));
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Adds a new fund. </summary>
	/// <param name="info">The information for the new fund.</param>
	/// <response code="200">Successful addition of the fund.</response>
	/// <response code="400">Invalid fund information.</response>
	/// <response code="401">Unauthorized access.</response>
	[HttpPost]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<FundDto>>> Add([FromBody] AddFundDto info)
	{
		try
		{
			return this.CustomOk(await funds.Add(info));
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Updates an existing fund. </summary>
	/// <param name="info">The updated information for the fund.</param>
	/// <param name="id">The ID of the fund to update.</param>
	/// <response code="200">Successful update of the fund.</response>
	/// <response code="400">Invalid fund information.</response>
	/// <response code="401">Unauthorized access.</response>
	/// <response code="404">Fund not found.</response>
	[HttpPatch("{id:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<FundDto>>> Update([FromBody] AddFundDto info, [FromRoute] Guid id)
	{
		try
		{
			var res = await funds.Update(info, id);
			return res.Status switch
			{
				HttpStatusCode.OK => this.CustomOk(res.Value),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	// TODO add optional notes in the request data
	/// <summary> Transfers funds between accounts. </summary>
	/// <param name="info">The transfer request details, including source and destination accounts, amount, and optional notes.</param>
	/// <response code="200">The transfer was successful.</response>
	/// <response code="400">The transfer request is invalid or missing required fields.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">The source or destination account does not exist.</response>
	[HttpPost("transfer")]
	[AuthorizeRole(UserRole.Type.Administrator, UserRole.Type.Supervisor)]
	public async Task<ActionResult<Response<TransferDto.Response>>> Transfer([FromBody] TransferDto info)
	{
		try
		{
			var res = await funds.Transfer(info);
			return res.Status switch
			{
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message),
				HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Withdraws funds from a specified account. </summary>
	/// <param name="id">The ID of the account to withdraw from.</param>
	/// <param name="info">The withdrawal information.</param>
	/// <response code="200">Successful withdrawal.</response>
	/// <response code="400">Invalid withdrawal request.</response>
	/// <response code="404">Account not found.</response>
	[HttpPost("{id:guid}/withdrawal")]
	[AuthorizeRole]
	public async Task<ActionResult<Response<FundDto>>> Withdraw([FromRoute] Guid id, TransactionDto info)
	{
		try
		{
			var res = await funds.Withdraw(id, info);
			return res.Status switch
			{
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message),
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
				HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary>Deposits funds into a specified account.</summary>
	/// <param name="id">The ID of the account to deposit into.</param>
	/// <param name="info">The deposit information.</param>
	/// <response code="200">Successful deposit.</response>
	/// <response code="400">Invalid deposit request.</response>
	/// <response code="404">Account not found.</response>
	[HttpPost("{id:guid}/deposit")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<FundDto>>> Deposit([FromRoute] Guid id, TransferDto info)
	{
		try
		{
			var res = await funds.Deposit(id, info);
			return res.Status switch
			{
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message),
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: res.Message),
				HttpStatusCode.OK => this.CustomOk(res.Value)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	[HttpPatch("{fundId:guid}/attach-user/{userid:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> AddUser(Guid fundId, Guid userid)
	{
		try
		{
			var res = await funds.AttachUser(userid, fundId);
			return res.Status switch
			{
				HttpStatusCode.OK => this.CustomOk(res.Value),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: res.Message)
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	[HttpDelete("{id:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> Delete([FromRoute] Guid id)
	{
		throw new NotImplementedException();
	}
}