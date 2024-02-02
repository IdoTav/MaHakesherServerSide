using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MaHakesherServerSide.Models;

namespace MaHakesherServerSide.Data
{
    public class MaHakesherServerSideContext : DbContext
    {
        public MaHakesherServerSideContext (DbContextOptions<MaHakesherServerSideContext> options)
            : base(options)
        {
        }

        public DbSet<MaHakesherServerSide.Models.Relations> Relations { get; set; } = default!;

        public DbSet<MaHakesherServerSide.Models.User>? User { get; set; }
    }
}
