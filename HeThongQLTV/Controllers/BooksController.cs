using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize]
public class BooksController(LibraryDb db) : Controller
{
    public IActionResult Index(string? q, string? category, string? availability, string? sort, int page = 1)
    {
        var list = db.Books.Include(b => b.Loans).ToList(); ViewBag.Categories = list.Select(b => b.Category).Distinct().Order().ToList();
        if (!string.IsNullOrWhiteSpace(q)) list = list.Where(b => $"{b.Title} {b.Author} {b.Isbn}".Contains(q.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList();
        if (!string.IsNullOrEmpty(category)) list = list.Where(b => b.Category == category).ToList();
        if (availability == "available") list = list.Where(b => b.Available > 0).ToList();
        if (availability == "out") list = list.Where(b => b.Available == 0).ToList();
        list = sort switch { "new" => list.OrderByDescending(b => b.Id).ToList(), "quantity" => list.OrderByDescending(b => b.Available).ToList(), _ => list.OrderBy(b => b.Title).ToList() };
        ViewBag.Total = list.Count; ViewBag.Pages = Math.Max(1, (int)Math.Ceiling(list.Count / 8.0)); page = Math.Clamp(page, 1, (int)ViewBag.Pages); ViewBag.Page = page;
        return View(list.Skip((page - 1) * 8).Take(8).ToList());
    }
    public IActionResult Details(int id) { var b = db.Books.Include(b => b.Loans).SingleOrDefault(b => b.Id == id); return b == null ? NotFound() : View(b); }
    [Authorize(Roles = "Admin,ThuThu")] public IActionResult Edit(int? id) { var b = id == null ? new Book() : db.Books.Find(id); return b == null ? NotFound() : View(b); }
    [HttpPost, Authorize(Roles = "Admin,ThuThu")]
    public IActionResult Edit(int id, [Bind("Id,Title,Author,Category,Publisher,Isbn,Year,Quantity,Shelf,Description")] Book input)
    {
        if (id != input.Id) return BadRequest();
        using var tx = db.Database.BeginTransaction();
        input.Publisher ??= ""; input.Isbn ??= ""; input.Shelf ??= ""; input.Description ??= "";
        if (input.Quantity < db.Loans.Count(l => l.BookId == id && l.ReturnedAt == null)) ModelState.AddModelError("Quantity", "Tổng số bản không được nhỏ hơn số bản đang mượn.");
        if (!ModelState.IsValid) return View(input);
        if (id == 0) db.Books.Add(input); else { var b = db.Books.Find(id); if (b == null) return NotFound(); b.Title = input.Title; b.Author = input.Author; b.Category = input.Category; b.Publisher = input.Publisher; b.Isbn = input.Isbn; b.Year = input.Year; b.Quantity = input.Quantity; b.Shelf = input.Shelf; b.Description = input.Description; }
        db.SaveChanges(); tx.Commit(); TempData["Success"] = "Đã lưu thông tin sách."; return RedirectToAction("Index");
    }
    [HttpPost, Authorize(Roles = "Admin,ThuThu")]
    public IActionResult Delete(int id)
    {
        var b = db.Books.Find(id); if (b == null) return NotFound();
        if (db.Loans.Any(l => l.BookId == id) || db.Reservations.Any(r => r.BookId == id)) { TempData["Error"] = "Không thể xóa sách có lịch sử mượn hoặc đặt trước."; return RedirectToAction("Index"); }
        db.Books.Remove(b); db.SaveChanges(); TempData["Success"] = "Đã xóa sách."; return RedirectToAction("Index");
    }
}
