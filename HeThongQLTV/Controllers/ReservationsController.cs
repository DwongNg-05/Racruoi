using System.Security.Claims;
using HeThongQLTV.Data;
using HeThongQLTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize]
public class ReservationsController(LibraryDb db, CirculationService service) : Controller
{
    private int MemberId => db.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))?.MemberId ?? -1;
    public IActionResult Index() { var q = db.Reservations.Include(r => r.Book).ThenInclude(b => b.Loans).Include(r => r.Member).AsQueryable(); if (User.IsInRole("DocGia")) q = q.Where(r => r.MemberId == MemberId); return View(q.OrderBy(r => r.CreatedAt).ToList()); }
    [HttpPost, Authorize(Roles = "DocGia")] public IActionResult Create(int bookId) { try { service.Reserve(bookId, MemberId); TempData["Success"] = "Đã thêm vào hàng chờ đặt trước."; } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction("Index"); }
    [HttpPost] public IActionResult Cancel(int id) { var r = db.Reservations.Find(id); if (r == null) return NotFound(); if (User.IsInRole("DocGia") && r.MemberId != MemberId) return Forbid(); if (r.Status == "Đang chờ") { r.Status = "Đã hủy"; db.SaveChanges(); TempData["Success"] = "Đã hủy yêu cầu."; } return RedirectToAction("Index"); }
}
