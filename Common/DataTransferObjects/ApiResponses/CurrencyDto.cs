namespace Common.DataTransferObjects.ApiResponses;

public class CurrencyDto
{
	public Guid Id { get; set; }
	public string Currency { get; set; }
	public double TotalBalance { get; set; }
	public IEnumerable<FundDto> Funds { get; set; } = [];
}