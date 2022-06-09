using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RSSReader2.Models;

namespace RSSReader2.Data
{
    public class RSSReader2Context : DbContext
    {
        public RSSReader2Context (DbContextOptions<RSSReader2Context> options)
            : base(options)
        {
        }

        public DbSet<RSSReader2.Models.RSSFeed>? RSSFeed { get; set; }
        public DbSet<RSSReader2.Models.Article>? Article { get; set; }
        
    }
}
