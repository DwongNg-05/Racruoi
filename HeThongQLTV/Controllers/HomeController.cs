using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace HeThongQLTV.Controllers;

[Authorize]
public class HomeController(LibraryDb db) : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("DocGia")) return RedirectToAction("Index", "Books");
        return View(new DashboardVm { Books = db.Books.Include(b => b.Loans).ToList(), Loans = db.Loans.Include(l => l.Book).Include(l => l.Member).ToList(), Members = db.Members.Count(), Reservations = db.Reservations.Count(r => r.Status == "Đang chờ") });
    }
    [AllowAnonymous, IgnoreAntiforgeryToken] public IActionResult Error() { Response.StatusCode = 500; return View("Message", ("Có lỗi khi xử lý", "Vui lòng thử lại. Nếu lỗi tiếp diễn, liên hệ quản trị viên.")); }
    [AllowAnonymous, IgnoreAntiforgeryToken] public IActionResult Status(int code) { Response.StatusCode = code; return View("Message", (code == 404 ? "Không tìm thấy trang" : "Không thể xử lý yêu cầu", "Kiểm tra đường dẫn hoặc dữ liệu biểu mẫu rồi thử lại.")); }
}
