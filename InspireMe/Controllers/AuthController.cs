using InspireMe.Contracts;
using InspireMe.Data;
using InspireMe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InspireMe.Controllers
{
    public class AuthController : Controller
    {

        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _dbcontext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderer;


        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager
            ,AppDbContext dbContext,RoleManager<IdentityRole> roleManager,IEmailSender emailSender,IViewRenderService viewRenderer)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbcontext = dbContext;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _viewRenderer = viewRenderer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login() { 
            return View();
        }

        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }
            var user = new AppUser { Email = model.Email,UserName=model.Email,PhoneNumber=model.PhoneNumber,Name=model.Name};
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded) {

                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    ModelState.AddModelError(string.Empty, $"Role {model.Role} does not exist.");
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, model.Role);

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth",
                    new { userId = user.Id, token = token }, Request.Scheme);

                //await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                //    $"Click <a href='{confirmationLink}'>here</a> to confirm your email.");
                var emailModel = new EmailConfirmModel { Name = user.Name, ConfirmationLink = confirmationLink };
                var emailBody = await _viewRenderer.RenderViewToStringAsync("~/Views/Shared/_EmailConfirmationTemplate.cshtml", emailModel);
               

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);


                TempData["SuccessMessage"] = "Registration successful. Please confirm your email before logging in.";
                return RedirectToAction("Login", "Auth");


            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
                if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email first.");
                return View(model);
            }


            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Quotes");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("Error");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View("ConfirmEmailSuccess");

            return View("ConfirmEmailFailed");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

    }
}
