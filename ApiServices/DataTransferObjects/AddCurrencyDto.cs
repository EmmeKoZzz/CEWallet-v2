using System.ComponentModel.DataAnnotations;

namespace ApiServices.DataTransferObjects;

public record AddCurrencyDto([Required] string Name);