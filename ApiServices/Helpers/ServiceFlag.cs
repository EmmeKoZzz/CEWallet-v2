using System.Net;


namespace ApiServices.Helpers;

public record ServiceFlag<T>(HttpStatusCode Status,
	T? Value = default,
	string? Message = default);