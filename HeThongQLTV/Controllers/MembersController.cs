using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize(Roles = "Admin,ThuThu")]
public class MembersController(LibraryDb db) : Controller
{
    public IActionResult Index(string? q, string? status) { var list = db.Members.Include(m => m.Loans).OrderBy(m => m.FullName).ToList(); if (!string.IsNullOrWhiteSpace(q)) list = list.Where(m => $"{m.FullName} {m.Email} {m.Id}".Contains(q.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList(); if (status == "active") list = list.Where(m => m.Active && m.ExpiresAt >= DateTime.Today).ToList(); if (status == "inactive") list = list.Where(m => !m.Active || m.ExpiresAt < DateTime.Today).ToList(); return View(list); }
    public IActionResult Edit(int? id) { var m = id == null ? new Member() : db.Members.Find(id); return m == null ? NotFound() : View(m); }
    [HttpPost]
    public IActionResult Edit(int id, [Bind("Id,FullName,Email,Phone,Type,ExpiresAt,Active")] Member input)
    {
        if (id != input.Id) return BadRequest();
        if (db.Members.Any(m => m.Email == input.Email && m.Id != id)) ModelState.AddModelError("Email", "Email này đã được sử dụng.");
        if (input.ExpiresAt == default) ModelState.AddModelError("ExpiresAt", "Vui lòng nhập ngày hết hạn.");
        if (!ModelState.IsValid) return View(input);
        if (id == 0) db.Members.Add(input); else { var m = db.Members.Find(id); if (m == null) return NotFound(); m.FullName = input.FullName; m.Email = input.Email; m.Phone = input.Phone; m.Type = input.Type; m.ExpiresAt = input.ExpiresAt; m.Active = input.Active; }
        db.SaveChanges(); TempData["Success"] = "Đã lưu thẻ độc giả."; return RedirectToAction("Index");
    }
    [HttpPost] public IActionResult Delete(int id) { var m = db.Members.Find(id); if (m == null) return NotFound(); if (db.Loans.Any(l => l.MemberId == id) || db.Users.Any(u => u.MemberId == id) || db.Reservations.Any(r => r.MemberId == id)) { TempData["Error"] = "Độc giả có dữ liệu liên kết. Hãy khóa thẻ thay vì xóa."; } else { db.Members.Remove(m); db.SaveChanges(); TempData["Success"] = "Đã xóa độc giả."; } return RedirectToAction("Index"); }
}
