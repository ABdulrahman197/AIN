using AIN.Core.Entites;
using AIN.Application.Interfaces.IRepos;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using static AIN.Core.Enums.enums;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IUserRepo userRepo, IConfiguration config)
    {
        // Read admin info using indexers (safe alternative)
        string adminEmail = config["AdminAccount:Email"] ?? "mustafamahmooud2004@gmail.com";
        string adminPassword = config["AdminAccount:Password"] ?? "Admin@123";
        string adminDisplayName = config["AdminAccount:DisplayName"] ?? "Admin";

        // Check if admin already exists
        var existingAdmin = await userRepo.GetByEmailAsync(adminEmail, CancellationToken.None);
        if (existingAdmin != null) return;

        // Create admin user
        var adminUser = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            DisplayName = adminDisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin,
            IsEmailConfirmed = true,
            TrustPoints = 100
        };

        await userRepo.AddAsync(adminUser, CancellationToken.None);
        await userRepo.SaveChangesAsync(CancellationToken.None);
    }
}
