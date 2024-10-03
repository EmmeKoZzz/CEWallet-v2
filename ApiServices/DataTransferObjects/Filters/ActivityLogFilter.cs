namespace ApiServices.DataTransferObjects.Filters;

public record ActivityLogFilter(
	DateTime? Since = default,
	DateTime? Until = default,
	string[]? FundTransaction = default,
	string[]? Activity = default,
	bool AmountOrCreateDate = default,
	bool Desc = true,
	double? AmountMin = default,
	double? AmountMax = default,
	string[]? Funds = default,
	string[]? Users = default,
	Guid[]? Currencies = default
);