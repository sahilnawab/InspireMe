using InspireMe.Models;

namespace InspireMe.Contracts
{
    public interface IQuoteService
    {
        Task<List<QuoteModel>> GetLoggedInUserQuote(string userId);
            Task<List<QuoteModel>> GetAllQuotesAsync();
        Task<QuoteModel> GetQuoteByIdAsync(int id);
        Task CreateQuoteAsync(QuoteModel quote,string userId);
        Task UpdateQuoteAsync(QuoteModel quote,int id);
        Task DeleteQuoteAsync(int id);
    }
}
