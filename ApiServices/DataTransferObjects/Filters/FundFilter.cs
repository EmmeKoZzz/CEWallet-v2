namespace ApiServices.DataTransferObjects.Filters;

public record FundFilter(
	string[]? FundNames = default,
	string[]? Usernames = default,
	Guid[]? Currencies = default,
	bool Descending = default,
	string? OrderBy = default
) {
	
	public static class OrderByOptions {
		
		public const string Funds = "funds";
		public const string Usernames = "usernames";
		public const string CreateAt = "create_at";
		
	}
	
}