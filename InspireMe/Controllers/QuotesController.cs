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
using AutoMapper;
using InspireMe.Contracts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InspireMe.Controllers
{
    [Authorize]
    public class QuotesController : Controller
    {
        private readonly IQuoteService _quoteService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public QuotesController( IWebHostEnvironment env, IConfiguration config, IQuoteService quoteService)
        {
           _quoteService = quoteService;
            _env = env;
            _config = config;
           
        }

        // GET: Quotes
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(await _quoteService.GetLoggedInUserQuote(userId));
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AdminList() {
            var quote = await _quoteService.GetAllQuotesAsync();
            return View(quote);
        }

        // GET: Quotes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _quoteService.GetQuoteByIdAsync(id);

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
  
               await _quoteService.CreateQuoteAsync(quoteModel, userId);

             
                    return RedirectToAction(nameof(Index));
               
              
            }
            return View(quoteModel);
        }
       



        // GET: Quotes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

          var quote =await _quoteService.GetQuoteByIdAsync(id);

            if (quote == null)
            {
                return NotFound();
            }
            return View(quote);
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

                await _quoteService.UpdateQuoteAsync(quote, id);
                return RedirectToAction(nameof(Index));
                
            }
            return View(quote);
        }

        // GET: Quotes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote =await _quoteService.GetQuoteByIdAsync(id);
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
            await _quoteService.DeleteQuoteAsync(id);
            return RedirectToAction(nameof(Index));
        }

       
    }
}
