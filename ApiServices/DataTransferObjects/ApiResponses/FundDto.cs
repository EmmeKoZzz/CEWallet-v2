namespace ApiServices.DataTransferObjects.ApiResponses;

public record FundDto(
	Guid Id,
	string Name,
	DateTime CreateAt,
	string? LocationUrl = default,
	string? Address = default,
	string? Details = default,
	IEnumerable<FundCurrencyInfo>? Currencies = default,
	UserDto? User = default
);

public record FundCurrencyInfo(string Name, double Amount);