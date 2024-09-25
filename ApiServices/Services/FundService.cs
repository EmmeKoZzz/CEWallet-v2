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
	/// <summary> Retrieves a collection of all active funds. </summary>
	/// <returns>An array of FundDto objects representing all active funds.</returns>
	public async Task<FundDto[]> GetAll()
	{
		return await dbContext.Funds.Where(entity => entity.Active)
			.Select(entity => new FundDto
			{
				Name = entity.Name,
				Id = entity.Id,
				CreateAt = entity.CreatedAt,
				LocationUrl = entity.LocationUrl
			})
			.ToArrayAsync();
	}

	/// <summary> Retrieves a specific fund by its ID. </summary>
	/// <param name="id">The unique identifier of the fund.</param>
	/// <returns>A ServiceFlag object containing either a FundDto representing the retrieved fund on success (with HttpStatusCode.OK), 
	/// or null with HttpStatusCode.NotFound if the fund is not found.</returns>
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

	/// <summary> Retrieves all funds associated with a specific user. </summary>
	/// <param name="id">The unique identifier of the user.</param>
	/// <returns>An array of FundDto objects representing all funds owned by the user, 
	/// or an empty array if the user has no funds.</returns>
	public async Task<FundDto[]> GetByUser(Guid id)
	{
		return await dbContext.Funds
			.Include(entity => entity.FundCurrencies)
			.ThenInclude(entity => entity.Fund)
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

	/// <summary> Creates a new fund based on the provided information. </summary>
	/// <param name="info">An AddFundDto object containing details about the new fund.</param>
	/// <returns>A FundDto object representing the newly created fund.</returns>
	public async Task<FundDto> Add(AddFundDto info)
	{
		var fund = new Fund { Name = info.Name, LocationUrl = info.LocationUrl };
		await dbContext.Funds.AddAsync(fund);
		await dbContext.SaveChangesAsync();
		return new FundDto { Id = fund.Id, LocationUrl = fund.LocationUrl, Name = fund.Name, CreateAt = fund.CreatedAt };
	}

	/// <summary> Updates an existing fund with the provided information. </summary>
	/// <param name="info">An AddFundDto object containing updated details for the fund.</param>
	/// <param name="id">The unique identifier of the fund to be updated.</param>
	/// <returns>A ServiceFlag object containing either a FundDto representing the updated fund on success (with HttpStatusCode.OK), 
	/// or null with HttpStatusCode.NotFound if the fund is not found.</returns>
	public async Task<ServiceFlag<FundDto>> Update(AddFundDto info, Guid id)
	{
		var fund = await dbContext.Funds.FindAsync(id);
		if (fund == null) return new ServiceFlag<FundDto>(HttpStatusCode.NotFound, Message: "Fund not found.");

		fund.Name = info.Name;
		fund.LocationUrl = info.LocationUrl;
		await dbContext.SaveChangesAsync();

		return new ServiceFlag<FundDto>(HttpStatusCode.OK,
			new FundDto { Id = fund.Id, LocationUrl = fund.LocationUrl, Name = fund.Name, CreateAt = fund.CreatedAt });
	}

	/// <summary> Transfers a specified amount of currency from one fund to another. </summary>  
	/// <param name="info">Information about the transfer, including from/to fund IDs and amount to transfer.</param>  
	/// <returns>A ServiceFlag containing the result of the transfer operation, including success or error information.</returns>  
	public async Task<ServiceFlag<TransferDto.Response>> Transfer(TransferDto info)
	{
		// Prepare a query to fetch funds with associated currencies from the database.  
		var query = dbContext.Funds.Include(entity => entity.FundCurrencies).ThenInclude(entity => entity.Currency);

		// Initiate asynchronous tasks to fetch the source (from) and destination (to) funds.  
		var (fromFundTask, toFundTask) =
			(query.SingleOrDefaultAsync(entity => entity.Id == info.FromId),
				query.SingleOrDefaultAsync(entity => entity.Id == info.ToId));

		// Wait for both tasks to complete.  
		await Task.WhenAll(fromFundTask, toFundTask);

		// Retrieve both funds after the tasks have completed.  
		var (fromFund, toFund) = (await fromFundTask, await toFundTask);

		// Check if either fund was not found and return a not found response if so.  
		if (fromFund == null || toFund == null)
		{
			return new ServiceFlag<TransferDto.Response>(HttpStatusCode.NotFound,
				Message: $"Fund with ID: {(fromFund == null ? info.FromId : info.ToId)} not found.");
		}

		// Check if the source fund has the specified currency and sufficient amount for the transfer.  
		var fromFundCurrency =
			fromFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.CurrencyId);
		if (fromFundCurrency == null || fromFundCurrency.Amount < info.Amount)
		{
			return new ServiceFlag<TransferDto.Response>(HttpStatusCode.BadRequest,
				Message: "Fund has not enough of this currency to make this operation.");
		}

		// Begin a database transaction for the transfer operation.  
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try
		{
			// Reduce the amount of currency in the source fund.  
			fromFundCurrency.Amount -= info.Amount;

			// Retrieve the fund currency in the destination fund. If it doesn't exist, create a new one.  
			var toFundCurrency = toFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.CurrencyId);
			if (toFundCurrency == null)
			{
				toFundCurrency = new FundCurrency
				{
					FundId = info.ToId,
					CurrencyId = info.CurrencyId,
					Amount = info.Amount
				};
				toFund.FundCurrencies.Add(toFundCurrency); // Add new currency record if it didn't exist.  
			}
			else
			{
				// Increase the amount in the existing currency record of the destination fund.  
				toFundCurrency.Amount += info.Amount;
			}

			// Save the changes to the database and commit the transaction.  
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
		}
		catch (Exception e)
		{
			// Roll back the transaction in case of an error and return an internal server error.  
			await trx.RollbackAsync();
			return new ServiceFlag<TransferDto.Response>(HttpStatusCode.InternalServerError, Message: e.Message);
		}

		// Create DTOs for both funds to return in the response.  
		var fromFundDto = CreateFundDto(fromFund);
		var toFundDto = CreateFundDto(toFund);

		// Return a successful response with the DTOs of both funds.  
		return new ServiceFlag<TransferDto.Response>(HttpStatusCode.OK, new TransferDto.Response(fromFundDto, toFundDto));

		// Creates a FundDto object from a Fund entity.  
		FundDto CreateFundDto(Fund fund) => new()
		{
			Id = fund.Id,
			Currencies = fund.FundCurrencies.Select(c => new FundDto.FundCurrency(c.Currency.Name, c.Amount)),
			Name = fund.Name,
			CreateAt = fund.CreatedAt,
			LocationUrl = fund.LocationUrl
		};
	}
}