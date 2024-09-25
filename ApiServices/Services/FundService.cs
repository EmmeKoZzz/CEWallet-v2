using System.Net;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using Common.Constants;
using Common.DataTransferObjects;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class FundService(AppDbContext dbContext)
{
	public async Task<FundDto[]> GetAll()
	{
		return await dbContext.Funds.Where(entity => entity.Active).Select(entity => new FundDto
				{ Name = entity.Name, Id = entity.Id, CreateAt = entity.CreatedAt, LocationUrl = entity.LocationUrl })
			.ToArrayAsync();
	}

	public async Task<ServiceFlag<FundDto>> Get(Guid id)
	{
		var res = await dbContext.Funds
			.Include(entity => entity.User)
			.Include(entity => entity.FundCurrencies)
			.ThenInclude(entity => entity.Fund)
			.SingleOrDefaultAsync(entity => entity.Id == id);

		return res != null
			? new ServiceFlag<FundDto>(HttpStatusCode.OK,
				new FundDto
				{
					Name = res.Name,
					Id = res.Id,
					CreateAt = res.CreatedAt,
					LocationUrl = res.LocationUrl,
					User = res.User != null
						? new UserDto(
							res.User.Id,
							res.User.Username,
							UserRole.Value(UserRole.Type.Assessor),
							res.User.CreatedAt)
						: null,
					Currencies =
						res.FundCurrencies.Select(entity => new FundDto.FundCurrency(entity.Fund.Name, entity.Amount))
				})
			: new ServiceFlag<FundDto>(HttpStatusCode.NotFound);
	}

	public async Task<FundDto[]> GetByUser(Guid id)
	{
		return await dbContext.Funds.Include(entity => entity.FundCurrencies).ThenInclude(entity => entity.Fund)
			.Where(entity => entity.UserId == id)
			.Select(entity => new FundDto
			{
				Id = entity.Id,
				Name = entity.Name,
				LocationUrl = entity.LocationUrl,
				CreateAt = entity.CreatedAt,
				Currencies =
					entity.FundCurrencies.Select(currency => new FundDto.FundCurrency(currency.Fund.Name, currency.Amount))
			}).ToArrayAsync();
	}

	public async Task<FundDto> Add(AddFundDto info)
	{
		var fund = new Fund { Name = info.Name, LocationUrl = info.LocationUrl };
		await dbContext.Funds.AddAsync(fund);
		await dbContext.SaveChangesAsync();
		return new FundDto { Id = fund.Id, LocationUrl = fund.LocationUrl, Name = fund.Name, CreateAt = fund.CreatedAt };
	}

	public async Task<ServiceFlag<FundDto>> Update(AddFundDto info, Guid id)
	{
		var fund = await dbContext.Funds.FindAsync(id);
		if (fund == null) return new ServiceFlag<FundDto>(HttpStatusCode.NotFound);

		fund.Name = info.Name;
		fund.LocationUrl = info.LocationUrl;
		await dbContext.SaveChangesAsync();

		return new ServiceFlag<FundDto>(HttpStatusCode.OK,
			new FundDto { Id = fund.Id, LocationUrl = fund.LocationUrl, Name = fund.Name, CreateAt = fund.CreatedAt });
	}
}