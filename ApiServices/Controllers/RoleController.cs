using System.Net;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Services;
using Common.Constants;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace ApiServices.Controllers;

[ApiController]
[Route("/role")]
public class RoleController(RoleService roleService) : ControllerBase
{
	/// <summary> Retrieves a list of all available user roles. </summary>
	/// <response code="401"> Unauthorized (user attempting to access without administrator privileges). </response>
	[HttpGet]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<IEnumerable<RoleDto>>>> GetAll()
	{
		try
		{
			return this.CustomOk((await roleService.GetAll()).Select(role => new RoleDto(role.Id, role.Name)));
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}

	/// <summary> Get role by his ID </summary>
	/// <response code="401"> Unauthorized (user attempting to access without administrator privileges). </response>
	/// <response code="404"> Invalid role specified (role doesn't exist). </response>
	[HttpGet("{id:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<RoleDto>>> GetById([FromRoute] Guid id)
	{
		try
		{
			var (status, role,_) = await roleService.FindById(id);
			return status switch
			{
				HttpStatusCode.NotFound => this.CustomNotFound(detail: $"Role {id} Not Found"),
				HttpStatusCode.OK => this.CustomOk(new RoleDto(role!.Id, role.Name)),
			};
		}
		catch (Exception e)
		{
			return this.InternalError(e.Message);
		}
	}
}