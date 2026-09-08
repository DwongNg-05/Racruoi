using HeThongQLTV.Data;
using HeThongQLTV.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace HeThongQLTV.Services;

public class CirculationService(LibraryDb db, IOptions<LibraryRules> options)
{
    private readonly LibraryRules rules = options.Value;
    public void Borrow(int memberId, int bookId)
    {
        using var tx = db.Database.BeginTransaction();
        var member = db.Members.Include(m => m.Loans).SingleOrDefault(m => m.Id == memberId) ?? throw new InvalidOperationException("Độc giả không tồn tại.");
        var book = db.Books.Include(b => b.Loans).SingleOrDefault(b => b.Id == bookId) ?? throw new InvalidOperationException("Sách không tồn tại.");
        ValidateMember(member);
        if (member.Loans.Count(l => l.ReturnedAt == null) >= rules.MaxLoans) throw new InvalidOperationException($"Độc giả chỉ được mượn tối đa {rules.MaxLoans} cuốn.");
        if (book.Available <= 0) throw new InvalidOperationException("Sách đã hết bản khả dụng.");
        if (member.Loans.Any(l => l.BookId == bookId && l.ReturnedAt == null)) throw new InvalidOperationException("Độc giả đang mượn đầu sách này.");
        var first = db.Reservations.Where(r => r.BookId == bookId && r.Status == "Đang chờ").OrderBy(r => r.CreatedAt).ThenBy(r => r.Id).FirstOrDefault();
        if (first != null && first.MemberId != memberId) throw new InvalidOperationException("Sách đang ưu tiên cho độc giả đứng đầu hàng đặt trước.");
        if (first != null) first.Status = "Đã nhận";
        db.Loans.Add(new Loan { BookId = bookId, MemberId = memberId, DueAt = DateTime.Today.AddDays(rules.LoanDays) }); db.SaveChanges(); tx.Commit();
    }
    public void Return(int id)
    {
        using var tx = db.Database.BeginTransaction(); var loan = db.Loans.Find(id) ?? throw new InvalidOperationException("Phiếu mượn không tồn tại.");
        if (loan.ReturnedAt != null) throw new InvalidOperationException("Phiếu này đã được trả trước đó.");
        loan.ReturnedAt = DateTime.Today; loan.Fine = Math.Max(0, (DateTime.Today - loan.DueAt.Date).Days) * (long)rules.FinePerDay; db.SaveChanges(); tx.Commit();
    }
    public void Renew(int id, int? memberId = null)
    {
        using var tx = db.Database.BeginTransaction(); var loan = db.Loans.Find(id) ?? throw new InvalidOperationException("Phiếu mượn không tồn tại.");
        if (memberId != null && loan.MemberId != memberId) throw new InvalidOperationException("Bạn không có quyền gia hạn phiếu này.");
        if (loan.ReturnedAt != null || loan.DueAt.Date < DateTime.Today || loan.Renewals >= rules.MaxRenewals) throw new InvalidOperationException("Không thể gia hạn: sách đã trả, quá hạn hoặc hết lượt gia hạn.");
        if (db.Reservations.Any(r => r.BookId == loan.BookId && r.Status == "Đang chờ")) throw new InvalidOperationException("Sách đang có độc giả đặt trước.");
        loan.DueAt = loan.DueAt.AddDays(rules.RenewalDays); loan.Renewals++; db.SaveChanges(); tx.Commit();
    }
    public void Reserve(int bookId, int memberId)
    {
        using var tx = db.Database.BeginTransaction();
        var member = db.Members.Include(m => m.Loans).SingleOrDefault(m => m.Id == memberId) ?? throw new InvalidOperationException("Tài khoản chưa liên kết thẻ độc giả."); ValidateMember(member);
        var book = db.Books.Include(b => b.Loans).SingleOrDefault(b => b.Id == bookId) ?? throw new InvalidOperationException("Không tìm thấy sách.");
        if (book.Available > 0) throw new InvalidOperationException("Sách còn sẵn. Vui lòng đến quầy để mượn trực tiếp.");
        if (db.Reservations.Any(r => r.BookId == bookId && r.MemberId == memberId && r.Status == "Đang chờ")) throw new InvalidOperationException("Bạn đã đặt trước sách này.");
        db.Reservations.Add(new Reservation { BookId = bookId, MemberId = memberId }); db.SaveChanges(); tx.Commit();
    }
    private static void ValidateMember(Member m)
    {
        if (!m.Active || m.ExpiresAt.Date < DateTime.Today) throw new InvalidOperationException("Thẻ độc giả đã khóa hoặc hết hạn.");
        if (m.Loans.Any(l => l.Fine > 0 && !l.FinePaid)) throw new InvalidOperationException("Độc giả còn khoản phạt chưa thanh toán.");
    }
}
