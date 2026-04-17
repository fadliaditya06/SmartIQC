using Microsoft.EntityFrameworkCore;
using Smart_IQC.Models;

namespace Smart_IQC.Function.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}