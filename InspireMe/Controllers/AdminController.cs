using InspireMe.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireMe.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IQuoteService _quoteService;
        private readonly IAdminService _adminService;
        public AdminController(IQuoteService quoteService, IAdminService adminService)
        {
            _quoteService = quoteService;
            _adminService = adminService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Quotes() 
        {
            var quote = await _quoteService.GetAllQuotesAsync();
            return View(quote);
        }

        //public async Task<IActionResult> Users()
        //{
        //    var users = await _adminService.GetAllUsersAsync();
        //    return View(users);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string selectedRole)
        {
            var success = await _adminService.UpdateUserRoleAsync(userId, selectedRole);
            if (success)
            {
                TempData["SuccessMessage"] = "User role updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error: user not found.";
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Users(string searchTerm = null)
        {
            var model = await _adminService.GetAllUsersWithRolesAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View(model);
        }

    }
}
