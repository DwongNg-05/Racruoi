using HeThongQLTV.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HeThongQLTV.Data;

public class LibraryDb(DbContextOptions<LibraryDb> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Book>().Ignore(x => x.Available); m.Entity<Loan>().Ignore(x => x.Status);
        m.Entity<Book>().ToTable(t => t.HasCheckConstraint("CK_Book_Quantity", "Quantity >= 0"));
        m.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        m.Entity<AppUser>().HasIndex(x => x.MemberId).IsUnique();
        m.Entity<Member>().HasIndex(x => x.Email).IsUnique();
        foreach (var fk in m.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())) fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
    public void Seed()
    {
        Database.EnsureCreated(); if (Users.Any()) return;
        var titles = new[] { ("Tôi thấy hoa vàng trên cỏ xanh", "Nguyễn Nhật Ánh", "Văn học", "#aa633b"), ("Nhà giả kim", "Paulo Coelho", "Văn học", "#b59b53"), ("Lập trình C# cơ bản", "Nhóm biên soạn CNTT", "Công nghệ", "#3b6876"), ("Tư duy nhanh và chậm", "Daniel Kahneman", "Tâm lý học", "#3f534b"), ("Đắc nhân tâm", "Dale Carnegie", "Kỹ năng sống", "#893f3b"), ("Sapiens: Lược sử loài người", "Yuval Noah Harari", "Lịch sử", "#9b826a"), ("Dế Mèn phiêu lưu ký", "Tô Hoài", "Văn học", "#51765e"), ("Nhập môn trí tuệ nhân tạo", "Nhóm biên soạn CNTT", "Công nghệ", "#454c72"), ("Vũ trụ trong vỏ hạt dẻ", "Stephen Hawking", "Khoa học", "#354761"), ("Tuổi trẻ đáng giá bao nhiêu", "Rosie Nguyễn", "Kỹ năng sống", "#b87e60"), ("Một thoáng ta rực rỡ ở nhân gian", "Ocean Vuong", "Văn học", "#5a6968"), ("Cơ sở dữ liệu quan hệ", "Nhóm biên soạn CNTT", "Công nghệ", "#61754b") };
        for (var i = 0; i < titles.Length; i++) Books.Add(new Book { Title = titles[i].Item1, Author = titles[i].Item2, Category = titles[i].Item3, Color = titles[i].Item4, Quantity = i == 7 ? 1 : 5 + i % 4, Publisher = "Dữ liệu demo", Year = 2020 + i % 5, Shelf = $"{(char)('A' + i % 3)}-{i + 1:00}", Description = $"Tài liệu thuộc chủ đề {titles[i].Item3.ToLower()}, phục vụ nhu cầu đọc và học tập. Mô tả, năm xuất bản và số lượng trong bản chạy thử là dữ liệu minh họa." });
        string[] names = ["Nguyễn Minh Anh", "Trần Hoàng Nam", "Lê Thu Hà", "Phạm Đức Huy", "Đỗ Ngọc Mai", "Vũ Quang Minh"];
        for (var i = 0; i < names.Length; i++) Members.Add(new Member { FullName = names[i], Email = $"docgia{i + 1}@example.com", Phone = $"090000000{i}", ExpiresAt = DateTime.Today.AddMonths(8 + i) });
        SaveChanges(); var hasher = new PasswordHasher<AppUser>();
        foreach (var item in new[] { ("admin", "Quản trị viên", "Admin", (int?)null), ("thuthu", "Nguyễn Tùng Dương", "ThuThu", (int?)null), ("docgia", "Nguyễn Minh Anh", "DocGia", (int?)1) })
        {
            var u = new AppUser { Username = item.Item1, FullName = item.Item2, Role = item.Item3, MemberId = item.Item4 }; u.PasswordHash = hasher.HashPassword(u, "ThuVien@123"); Users.Add(u);
        }
        for (var i = 0; i < 16; i++) Loans.Add(new Loan { BookId = i % 12 + 1, MemberId = i % 6 + 1, BorrowedAt = DateTime.Today.AddDays(-22 + i), DueAt = DateTime.Today.AddDays(-8 + i), ReturnedAt = i < 6 ? DateTime.Today.AddDays(-5 + i) : null });
        SaveChanges();
    }
}
