namespace ApiServices.Models.DataTransferObjects;

public record LogFilterDto(
	DateTime? Since = default,
	DateTime? Until = default,
	string[]? FundTransaction = default,
	string[]? Activity = default,
	bool? DescAmount = default,
	bool? DescCreateDate = default,
	double? AmountMin = default,
	double? AmountMax = default,
	Guid? Fund = default,
	Guid? TargetFund = default,
	Guid? User = default,
	Guid? Currency = default
);