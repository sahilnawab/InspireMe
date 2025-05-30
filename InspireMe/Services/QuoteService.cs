using AutoMapper;
using Azure.Storage.Blobs;
using InspireMe.Contracts;
using InspireMe.Data;
using InspireMe.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InspireMe.Services
{
    public class QuoteService: IQuoteService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;


        public QuoteService(AppDbContext context, IMapper mapper, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
            _config = config;
        }

        public async Task<List<QuoteModel>> GetLoggedInUserQuote(string userId) {
           var quotes =await _context.Quotes.Where(s => s.UserId == userId).ToListAsync();
            return _mapper.Map<List<QuoteModel>>(quotes);

        }
        public async Task<List<QuoteModel>> GetAllQuotesAsync()
        {
            var quotes= await _context.Quotes.Include(q => q.User).ToListAsync();
            return _mapper.Map<List<QuoteModel>>(quotes);
        }

        public async Task<QuoteModel> GetQuoteByIdAsync(int id)
        {
            var quote= await _context.Quotes.FindAsync(id);
            return _mapper.Map<QuoteModel>(quote);

        }

        public async Task CreateQuoteAsync(QuoteModel quoteModel,string userId)
        {
            var imageUrl = await SaveImageAsync(quoteModel.Image);

            var quote = new Quote();
            quote.ImageUrl = imageUrl;
            quote.UserId = userId;
            quote.author = quoteModel.author;
            quote.content = quoteModel.content;


            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuoteAsync(QuoteModel quote, int id)
        {
            var imageUrl = await SaveImageAsync(quote.Image);

            var ExistingQuote = _context.Quotes.FirstOrDefault(s => s.id == id);
            if (ExistingQuote != null)
            {
                ExistingQuote.author = quote.author;
                ExistingQuote.content = quote.content;
                ExistingQuote.ImageUrl = imageUrl;

                try
                {
                    _context.Update(ExistingQuote);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public async Task DeleteQuoteAsync(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                if (!string.IsNullOrEmpty(quote.ImageUrl) && quote.ImageUrl.Contains(".blob.core.windows.net"))
                {
                    await DeleteBlobAsync(quote.ImageUrl);
                }
                else
                {
                    // delete local file
                    var path = Path.Combine(_env.WebRootPath, quote.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                _context.Quotes.Remove(quote);
            }

            await _context.SaveChangesAsync();
        }
        private async Task<string?> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);

            // In development: Save locally
            if (_env.IsDevelopment())
            {
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                return "/uploads/" + fileName; // Return relative URL
            }

            // In production: Upload to Azure Blob
            var connectionString = _config["AzureBlob:ConnectionString"];
            var containerName = _config["AzureBlob:Container"];

            var blobClient = new BlobContainerClient(connectionString, containerName);
            await blobClient.CreateIfNotExistsAsync();
            var blob = blobClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            return blob.Uri.ToString(); // Full URL to image
        }
        private async Task DeleteBlobAsync(string blobUrl)
        {
            try
            {
                // Extract blob name from URL
                var uri = new Uri(blobUrl);
                var blobName = Path.GetFileName(uri.LocalPath); // e.g. "abc123.jpg"

                var containerName = _config["AzureBlob:Container"];
                var connectionString = _config["AzureBlob:ConnectionString"];

                var blobContainerClient = new BlobContainerClient(connectionString, containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync(); // 🔥 Delete from Azure
            }
            catch (Exception ex)
            {
                // Optional: Log error or notify admin
                Console.WriteLine("Blob deletion failed: " + ex.Message);
            }
        }

        private bool QuoteExists(int id)
        {
            return _context.Quotes.Any(e => e.id == id);
        }


    }
}
