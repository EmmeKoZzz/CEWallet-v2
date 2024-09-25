namespace Common.DataTransferObjects.ApiResponses;

public class FundDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public UserDto? User { get; set; }
	public string? LocationUrl { get; set; }
	public DateTime CreateAt { get; set; }
	public IEnumerable<FundCurrency> Currencies { get; set; }

	public class FundCurrency(string name, double amount)
	{
		public string Currency { get; } = name;
		public double Amount { get; } = amount;
	}
}