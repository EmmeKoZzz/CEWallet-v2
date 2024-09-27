using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record AddFundDto([Required] string Name, string LocationUrl, string Address, string Details);