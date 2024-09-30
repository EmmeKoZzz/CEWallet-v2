namespace ApiServices.Models.Constants;

public struct FundTransaction {
	
	public enum Type { Deposit, Withdrawal }
	
	public static string? Value(Type? type) => type switch { Type.Deposit => "Depósito", Type.Withdrawal => "Egreso", _ => null };
	
}