using System.ComponentModel.DataAnnotations;
namespace HeThongQLTV.Models;

public class Book
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Title { get; set; } = "";
    [Required, StringLength(120)] public string Author { get; set; } = "";
    [Required, StringLength(80)] public string Category { get; set; } = "";
    [StringLength(120)] public string Publisher { get; set; } = "";
    [StringLength(30)] public string Isbn { get; set; } = "";
    [Range(1, 2100)] public int Year { get; set; } = 2024;
    [Range(0, 10000)] public int Quantity { get; set; } = 1;
    [StringLength(30)] public string Shelf { get; set; } = "A-01";
    [StringLength(3000)] public string Description { get; set; } = "";
    public string Color { get; set; } = "#244f46";
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public int Available => Quantity - Loans.Count(l => l.ReturnedAt == null);
}
public class Member
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = "";
    [Phone, StringLength(20)] public string? Phone { get; set; }
    [Required, StringLength(40)] public string Type { get; set; } = "Sinh viên";
    public DateTime ExpiresAt { get; set; } = DateTime.Today.AddYears(1);
    public bool Active { get; set; } = true;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "DocGia";
    public bool Active { get; set; } = true;
    public int? MemberId { get; set; }
    public Member? Member { get; set; }
}
public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime BorrowedAt { get; set; } = DateTime.Today;
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int Renewals { get; set; }
    public long Fine { get; set; }
    public bool FinePaid { get; set; }
    public string Status => ReturnedAt != null ? "Đã trả" : DueAt.Date < DateTime.Today ? "Quá hạn" : "Đang mượn";
}
public class Reservation
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Đang chờ";
}
public class LibraryRules
{
    public int MaxLoans { get; set; } = 5;
    public int LoanDays { get; set; } = 14;
    public int RenewalDays { get; set; } = 7;
    public int MaxRenewals { get; set; } = 2;
    public int FinePerDay { get; set; } = 2000;
}
public class DashboardVm
{
    public List<Book> Books { get; set; } = [];
    public List<Loan> Loans { get; set; } = [];
    public int Members { get; set; }
    public int Reservations { get; set; }
}
public class UserInput
{
    public int Id { get; set; }
    [Required, RegularExpression("^[a-zA-Z0-9._-]{3,40}$", ErrorMessage = "Tên đăng nhập gồm 3–40 chữ không dấu, số, dấu chấm hoặc gạch.")] public string Username { get; set; } = "";
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [StringLength(100, MinimumLength = 8)] public string? Password { get; set; }
    [Required, RegularExpression("^(Admin|ThuThu|DocGia)$")] public string Role { get; set; } = "DocGia";
    public int? MemberId { get; set; }
    public bool Active { get; set; } = true;
}
