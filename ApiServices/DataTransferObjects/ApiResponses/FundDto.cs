namespace ApiServices.DataTransferObjects.ApiResponses;

public record FundDto(
	Guid Id,
	string Name,
	DateTime CreateAt,
	string? LocationUrl = default,
	string? Address = default,
	string? Details = default,
	IEnumerable<FundDto.CurrencyAmount>? Currencies = default,
	UserDto? User = default
) {
	
	public record CurrencyAmount(string Currency, double Amount);
	
}