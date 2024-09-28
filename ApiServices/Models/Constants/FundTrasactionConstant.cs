namespace ApiServices.Models.Constants;

public struct FundTransaction
{
	public enum Type
	{
		Deposit,
		Withdrawal,
		Transfer
	}

	public static string Value(Type type) => type switch
	{
		Type.Deposit => "Depósito",
		Type.Withdrawal => "Egreso",
		Type.Transfer => "Transferencia",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
}