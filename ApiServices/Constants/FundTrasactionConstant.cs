namespace ApiServices.Constants;

public struct FundTransaction {
	public enum Type { Deposit, Withdrawal }

	public static string? Value(Type? type) {
		return type switch {
			Type.Deposit => "Depósito",
			Type.Withdrawal => "Egreso",
			_ => null
		};
	}
}