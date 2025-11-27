using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using queryHelp.Models;

namespace queryHelp.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(ApplicationDbContext db, IConfiguration config)
        {
            if (!db.Users.Any())
            {
                var adminPassword = config["DefaultAdmin:Password"] ?? "Admin@123";
                var admin = new User
                {
                    Username = config["DefaultAdmin:Username"] ?? "admin",
                    Email = config["DefaultAdmin:Email"] ?? "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = "admin"
                };
                db.Users.Add(admin);
                db.SaveChanges();
            }
        }
    }
}