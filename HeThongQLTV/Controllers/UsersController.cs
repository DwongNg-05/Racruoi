using System.Security.Claims;
using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController(LibraryDb db) : Controller
{
    public IActionResult Index() => View(db.Users.Include(u => u.Member).OrderBy(u => u.Id).ToList());
    public IActionResult Edit(int? id) { Load(); if (id == null) return View(new UserInput()); var u = db.Users.Find(id); return u == null ? NotFound() : View(new UserInput { Id = u.Id, Username = u.Username, FullName = u.FullName, Role = u.Role, MemberId = u.MemberId, Active = u.Active }); }
    [HttpPost]
    public IActionResult Edit(int id, UserInput input)
    {
        if (id != input.Id) return BadRequest(); input.Username = (input.Username ?? "").Trim().ToLower();
        if (db.Users.Any(u => u.Username == input.Username && u.Id != id)) ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
        if (id == 0 && string.IsNullOrEmpty(input.Password)) ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự.");
        if (input.Role == "DocGia" && (input.MemberId == null || !db.Members.Any(m => m.Id == input.MemberId))) ModelState.AddModelError("MemberId", "Chọn thẻ độc giả hợp lệ.");
        if (input.MemberId != null && db.Users.Any(u => u.MemberId == input.MemberId && u.Id != id)) ModelState.AddModelError("MemberId", "Thẻ này đã liên kết một tài khoản.");
        if (id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier) && (!input.Active || input.Role != "Admin")) ModelState.AddModelError("", "Không thể tự khóa hoặc hạ quyền tài khoản đang sử dụng.");
        if (!ModelState.IsValid) { Load(); return View(input); }
        var user = id == 0 ? new AppUser() : db.Users.Find(id); if (user == null) return NotFound(); user.Username = input.Username; user.FullName = input.FullName; user.Role = input.Role; user.MemberId = input.Role == "DocGia" ? input.MemberId : null; user.Active = input.Active;
        if (!string.IsNullOrEmpty(input.Password)) user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, input.Password);
        if (id == 0) db.Users.Add(user); db.SaveChanges(); TempData["Success"] = "Đã lưu tài khoản và phân quyền."; return RedirectToAction("Index");
    }
    private void Load() => ViewBag.Members = db.Members.OrderBy(m => m.FullName).ToList();
}
