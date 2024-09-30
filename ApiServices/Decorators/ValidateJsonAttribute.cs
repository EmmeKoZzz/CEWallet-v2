using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ApiServices.Decorators;

[AttributeUsage(AttributeTargets.Property)]
public class ValidateJsonAttribute : ValidationAttribute {
	
	public override bool IsValid(object? value) {
		if (value == null) return false;
		
		var currencyData = value as string;
		
		if (string.IsNullOrEmpty(currencyData)) return false;
		
		try {
			JsonSerializer.Deserialize<object>(currencyData);
			
			return true;
		} catch (JsonException) { return false; }
	}
	
}