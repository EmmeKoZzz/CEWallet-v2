namespace ApiServices.DataTransferObjects.Filters;

public record FundFilter(
	string[]? FundNames = default,
	Guid[]? Currencies = default,
	string[]? Usernames = default,
	bool Descending = default,
	string? OrderBy = default
) {
	public string[]? Usernames { get; set; } = Usernames;

	public static class OrderByOptions {
		public const string Funds = "funds";
		public const string Usernames = "usernames";
		public const string CreateAt = "create_at";
	}
}