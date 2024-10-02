namespace ApiServices.Constants;

public struct FundActivity {
	
	public enum Type { Deposit, Withdrawal, Transfer, CreateFund, DeleteFund }
	
	public static string Value(Type type) => type switch {
		Type.Deposit => "Depósito",
		Type.Withdrawal => "Egreso",
		Type.Transfer => "Transferencia",
		Type.CreateFund => "Creación de un Fondo",
		Type.DeleteFund => "Eliminación de un Fondo",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
	
}