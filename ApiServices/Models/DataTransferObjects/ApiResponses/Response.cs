using System.Net;


namespace ApiServices.Models.DataTransferObjects.ApiResponses;

public record Response<T>(HttpStatusCode Status,
	T? Data = default,
	string? Detail = default);