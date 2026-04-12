using Microsoft.EntityFrameworkCore;
using P1F_IQC.Models;

namespace P1F_IQC.Function.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /**
         *  DataSet Should name the same as table in database
         *
         */

    }
}