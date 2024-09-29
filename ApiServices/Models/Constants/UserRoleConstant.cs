namespace ApiServices.Models.Constants;

public struct UserRole {
	
	public enum Type { Assessor, Supervisor, Administrator }
	
	public static string Value(Type type) => type switch {
		Type.Assessor => "Asesor",
		Type.Supervisor => "Supervisor",
		Type.Administrator => "Administrador",
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid Role Type")
	};
	
	public static Type Value(string role) => role switch {
		"Asesor" => Type.Assessor,
		"Supervisor" => Type.Supervisor,
		"Administrador" => Type.Administrator,
		_ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid Role String")
	};
	
}