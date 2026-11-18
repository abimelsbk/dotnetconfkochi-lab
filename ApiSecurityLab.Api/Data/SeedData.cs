using ApiSecurityLab.Api.Models;

namespace ApiSecurityLab.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Invoices.Any()) return;

        db.Invoices.AddRange(
            new Invoice { Id = 101, OwnerId = "alice", CustomerName = "Alice Thomas",
                          Description = "Website design, October", Amount = 25000m },
            new Invoice { Id = 102, OwnerId = "alice", CustomerName = "Alice Thomas",
                          Description = "Hosting, 1 year", Amount = 6000m },
            new Invoice { Id = 202, OwnerId = "bob", CustomerName = "Bob Mathew",
                          Description = "Mobile app MVP", Amount = 180000m });

        db.SaveChanges();
    }
}
