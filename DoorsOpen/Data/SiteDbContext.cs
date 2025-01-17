﻿using DoorsOpen.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoorsOpen.Data
{
    public class SiteDbContext : DbContext
    {
        public SiteDbContext(DbContextOptions<SiteDbContext> options) : base(options)
        {
        }

        public DbSet<SecurityUser> User { get; set; }
        public DbSet<SecurityGroup> Groups { get; set; }
        public DbSet<BuildingModel> Buildings { get; set; }
        public DbSet<EventModel> Events { get; set; }
        public DbSet<BuildingImageModel> BuildingImages { get; set; }
    }
}

