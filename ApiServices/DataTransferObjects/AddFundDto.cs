using System.ComponentModel.DataAnnotations;

namespace ApiServices.DataTransferObjects;

public record AddFundDto([Required] string Name, string? LocationUrl, string? Address, string? Details);