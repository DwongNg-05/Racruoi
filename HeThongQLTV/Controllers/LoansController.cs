using System.Security.Claims;
using HeThongQLTV.Data;
using HeThongQLTV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize]
public class LoansController(LibraryDb db, CirculationService service) : Controller
{
    private int MemberId => db.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))?.MemberId ?? -1;
    public IActionResult Index(string? q, string? status) { var query = db.Loans.Include(l => l.Book).Include(l => l.Member).AsQueryable(); if (User.IsInRole("DocGia")) query = query.Where(l => l.MemberId == MemberId); var list = query.OrderByDescending(l => l.Id).ToList(); if (!string.IsNullOrWhiteSpace(q)) list = list.Where(l => $"{l.Book.Title} {l.Member.FullName} PM{l.Id:0000}".Contains(q.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList(); if (!string.IsNullOrEmpty(status)) list = list.Where(l => l.Status == status).ToList(); return View(list); }
    [Authorize(Roles = "Admin,ThuThu")] public IActionResult Create() { Load(); return View(); }
    [HttpPost, Authorize(Roles = "Admin,ThuThu")] public IActionResult Create(int memberId, int bookId) { try { service.Borrow(memberId, bookId); TempData["Success"] = "Lập phiếu mượn thành công."; return RedirectToAction("Index"); } catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); Load(); return View(); } }
    private void Load() { ViewBag.Members = db.Members.OrderBy(m => m.FullName).ToList(); ViewBag.Books = db.Books.Include(b => b.Loans).OrderBy(b => b.Title).ToList(); }
    [HttpPost, Authorize(Roles = "Admin,ThuThu")] public IActionResult Return(int id) => Run(() => service.Return(id), "Đã ghi nhận trả sách và tính tiền phạt nếu quá hạn.");
    [HttpPost] public IActionResult Renew(int id) => Run(() => service.Renew(id, User.IsInRole("DocGia") ? MemberId : null), "Gia hạn thành công.");
    [HttpPost, Authorize(Roles = "Admin,ThuThu")] public IActionResult Pay(int id) => Run(() => { var l = db.Loans.Find(id) ?? throw new InvalidOperationException("Không tìm thấy phiếu."); if (l.Fine <= 0 || l.FinePaid) throw new InvalidOperationException("Không có khoản phạt cần thanh toán."); l.FinePaid = true; db.SaveChanges(); }, "Đã ghi nhận thanh toán tiền phạt.");
    private IActionResult Run(Action action, string message) { try { action(); TempData["Success"] = message; } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction("Index"); }
}
