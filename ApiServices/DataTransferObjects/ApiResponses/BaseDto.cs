using System.Net;

namespace ApiServices.DataTransferObjects.ApiResponses;

public record BaseDto<T>(HttpStatusCode Status, T? Response = default, string? Detail = default);