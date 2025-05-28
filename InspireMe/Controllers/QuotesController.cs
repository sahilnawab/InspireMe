using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InspireMe.Data;
using InspireMe.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Azure.Storage.Blobs;

namespace InspireMe.Controllers
{
    [Authorize]
    public class QuotesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public QuotesController(AppDbContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        // GET: Quotes
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(await _context.Quotes.Where(s=>s.UserId==userId) .ToListAsync());
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AdminList() {
            return View(await _context.Quotes.Include(s => s.User).ToListAsync());
        }

        // GET: Quotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .FirstOrDefaultAsync(m => m.id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // GET: Quotes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( QuoteModel quoteModel)
        {
            if (ModelState.IsValid)
            {
                // Get the logged-in user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized(); // Ensure the user is logged in
                }
                var imageUrl = await SaveImageAsync(quoteModel.Image);

                var quote = new Quote();
                quote.ImageUrl = imageUrl;
                quote.UserId = userId;
                quote.author = quoteModel.author;
                quote.content=quoteModel.content;


                _context.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quoteModel);
        }
       



        // GET: Quotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes.FindAsync(id);
            var quoteModel = new QuoteModel();
            quoteModel.author = quote.author;
            quoteModel.content = quote.content;
            quoteModel.ImagePath = quote.ImageUrl;


            if (quote == null)
            {
                return NotFound();
            }
            return View(quoteModel);
        }

        // POST: Quotes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuoteModel quote)
        {


            if (ModelState.IsValid)
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
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(quote);
        }

        // GET: Quotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .FirstOrDefaultAsync(m => m.id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // POST: Quotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                _context.Quotes.Remove(quote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        private bool QuoteExists(int id)
        {
            return _context.Quotes.Any(e => e.id == id);
        }
    }
}
