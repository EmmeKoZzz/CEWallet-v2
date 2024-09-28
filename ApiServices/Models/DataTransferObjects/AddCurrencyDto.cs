using System.ComponentModel.DataAnnotations;


namespace ApiServices.Models.DataTransferObjects;

public record AddCurrencyDto([Required] string Name);