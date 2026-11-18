using ApiSecurityLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSecurityLab.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
}
