using System.Security.Claims;
using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace HeThongQLTV.Controllers;

public class AccountController(LibraryDb db) : Controller
{
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        var u = db.Users.SingleOrDefault(u => u.Username == (username ?? "").Trim().ToLower());
        if (u == null || !u.Active || string.IsNullOrEmpty(password) || new PasswordHasher<AppUser>().VerifyHashedPassword(u, u.PasswordHash, password) == PasswordVerificationResult.Failed) { ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã khóa."); return View(); }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()), new Claim(ClaimTypes.Name, u.FullName), new Claim(ClaimTypes.Role, u.Role) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return RedirectToAction("Login"); }
    public IActionResult Denied() { Response.StatusCode = 403; return View("~/Views/Home/Message.cshtml", ("Bạn không có quyền truy cập", "Chức năng này chỉ dành cho vai trò được cấp quyền.")); }
}
