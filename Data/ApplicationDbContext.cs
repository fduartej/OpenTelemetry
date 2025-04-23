using Microsoft.EntityFrameworkCore;
using ReFactoring.Models;

namespace ReFactoring.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    public DbSet<Customer> DbSetCustomer { get; set; }
}