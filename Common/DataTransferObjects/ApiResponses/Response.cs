using System.Net;

namespace Common.DataTransferObjects.ApiResponses;

public record Response<T>(
	HttpStatusCode Status,
	T? Data = default,
	string? Detail = default
);