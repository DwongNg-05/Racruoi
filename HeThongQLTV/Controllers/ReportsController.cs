using System.Text;
using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize(Roles = "Admin,ThuThu")]
public class ReportsController(LibraryDb db) : Controller
{
    public IActionResult Index(DateTime? from, DateTime? to) { if (from > to) ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc."); var q = db.Loans.Include(l => l.Book).Include(l => l.Member).AsQueryable(); if (from != null) q = q.Where(l => l.BorrowedAt >= from.Value.Date); if (to != null) q = q.Where(l => l.BorrowedAt < to.Value.Date.AddDays(1)); return View(new DashboardVm { Books = db.Books.Include(b => b.Loans).ToList(), Loans = q.ToList(), Members = db.Members.Count(), Reservations = db.Reservations.Count(r => r.Status == "Đang chờ") }); }
    public IActionResult Export(string kind = "loans", DateTime? from = null, DateTime? to = null)
    {
        if (from > to) { TempData["Error"] = "Khoảng ngày không hợp lệ."; return RedirectToAction("Index"); }
        var rows = new List<string>();
        if (kind == "books") { rows.Add("Mã sách,Tên sách,Tác giả,Thể loại,Tổng bản,Còn sẵn"); foreach (var b in db.Books.Include(b => b.Loans)) rows.Add(string.Join(",", b.Id, Csv(b.Title), Csv(b.Author), Csv(b.Category), b.Quantity, b.Available)); }
        else { rows.Add("Mã phiếu,Sách,Độc giả,Ngày mượn,Hạn trả,Trạng thái,Tiền phạt,Đã thanh toán"); var q = db.Loans.Include(l => l.Book).Include(l => l.Member).AsQueryable(); if (from != null) q = q.Where(l => l.BorrowedAt >= from.Value.Date); if (to != null) q = q.Where(l => l.BorrowedAt < to.Value.Date.AddDays(1)); foreach (var l in q) rows.Add(string.Join(",", l.Id, Csv(l.Book.Title), Csv(l.Member.FullName), l.BorrowedAt.ToString("yyyy-MM-dd"), l.DueAt.ToString("yyyy-MM-dd"), Csv(l.Status), l.Fine, l.FinePaid ? "Có" : "Không")); }
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(string.Join("\r\n", rows))).ToArray(); return File(bytes, "text/csv; charset=utf-8", $"thu-vien-{kind}-{DateTime.Today:yyyyMMdd}.csv");
    }
    private static string Csv(string value) { if (value.Length > 0 && "=+-@\t\r".Contains(value[0])) value = "'" + value; return "\"" + value.Replace("\"", "\"\"") + "\""; }
}
