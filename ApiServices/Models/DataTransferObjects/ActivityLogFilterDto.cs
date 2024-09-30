namespace ApiServices.Models.DataTransferObjects;

public record ActivityLogFilterDto(
	DateTime? Since = default,
	DateTime? Until = default,
	string[]? FundTransaction = default,
	string[]? Activity = default,
	bool AmountOrCreateDate = default,
	bool Desc = true,
	double? AmountMin = default,
	double? AmountMax = default,
	Guid[]? Funds = default,
	Guid[]? Users = default,
	Guid? Currency = default
);