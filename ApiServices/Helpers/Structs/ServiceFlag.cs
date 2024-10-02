using System.Net;

namespace ApiServices.Helpers.Structs;

public record ServiceFlag<T>(HttpStatusCode Status, T? Value = default, string? Message = default);