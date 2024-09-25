namespace Common.DataTransferObjects.ApiResponses;

public record FundDto(
	Guid Id,
	string Name,
	DateTime CreateAt,
	string? LocationUrl = default,
	IEnumerable<FundDto.FundCurrency>? Currencies = default,
	UserDto? User = default)
{
	public record FundCurrency(string Name, double Amount);
}