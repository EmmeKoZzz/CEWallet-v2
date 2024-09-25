namespace Common.DataTransferObjects.ApiResponses;

public class FundDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public UserDto? User { get; set; }
	public string? LocationUrl { get; set; }
	public DateTime CreateAt { get; set; }
	public IEnumerable<FundCurrency> Currencies { get; set; }

	public record FundCurrency(string Name, double Amount);
}