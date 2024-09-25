using ApiServices.Decorators;
using Common.Constants;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController]
[Route("/fund")]
public class FundController : ControllerBase
{
	/// <summary>Retrieves all funds.</summary>
	/// <response code="200">A list of funds.</response>
	[HttpGet]
	[AuthorizeRole(UserRole.Type.Administrator, UserRole.Type.Supervisor)]
	public async Task<ActionResult<Response<object>>> GetAll()
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves a specific fund by its ID.</summary>
	/// <param name="id">The ID of the fund to retrieve.</param>
	/// <response code="200">The requested fund.</response>
	[HttpGet("{id:guid}")]
	[AuthorizeRole]
	public async Task<ActionResult<Response<object>>> Get([FromRoute] Guid id)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves a list of funds belonging to a specific user.</summary>
	/// <param name="id">The ID of the user.</param>
	/// <response code="200">A list of funds belonging to the user.</response>
	[HttpGet("user/{id:guid}")]
	[AuthorizeRole]
	public async Task<ActionResult<Response<object>>> GetUserFunds([FromRoute] Guid id)
	{
		throw new NotImplementedException();
	}

	/// <summary>Adds a new fund.</summary>
	/// <response code="200">The newly added fund.</response>
	[HttpPost]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> Add()
	{
		throw new NotImplementedException();
	}

	/// <summary>Updates an existing fund.</summary>
	/// <response code="200">The updated fund.</response>
	[HttpPatch]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> Update()
	{
		throw new NotImplementedException();
	}

	/// <summary> Transfers funds between accounts. </summary>
	/// <param name="transferRequest">The transfer request details, including source and destination accounts, amount, and optional notes.</param>
	/// <response code="200">The transfer was successful.</response>
	/// <response code="400">The transfer request is invalid or missing required fields.</response>
	/// <response code="401">The user is not authorized to perform this action.</response>
	/// <response code="404">The source or destination account does not exist.</response>
	[HttpPatch("transfer")]
	[AuthorizeRole(UserRole.Type.Administrator, UserRole.Type.Supervisor)]
	public async Task<ActionResult<Response<object>>> Transfer(object transferRequest)
	{
		throw new NotImplementedException();
	}

	[HttpPatch("{id:guid}/withdrawal")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> Withdraw([FromRoute] Guid id, object info)
	{
		throw new NotImplementedException();
	}

	[HttpPatch("{id:guid}/deposit")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> UpdateFunds([FromRoute] Guid id, object info)
	{
		throw new NotImplementedException();
	}

	/// <summary>Adds a user to an existing fund.</summary>
	[HttpPatch("add-user")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> AddUser([FromBody] object ids)
	{
		throw new NotImplementedException();
	}

	/// <summary>Deletes a fund.</summary>
	[HttpDelete("{id:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<object>>> Delete([FromRoute] Guid id)
	{
		throw new NotImplementedException();
	}
}