using Microsoft.AspNetCore.Identity;

namespace GymManagementSystem.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roleNames = { "Admin", "Member" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user (as specified in project requirements)
            var adminEmail = "ogrencinumarasi@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "sau");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Also create the original admin for backup
            var backupAdminEmail = "admin@fitlifegym.com";
            var backupAdminUser = await userManager.FindByEmailAsync(backupAdminEmail);

            if (backupAdminUser == null)
            {
                backupAdminUser = new ApplicationUser
                {
                    UserName = backupAdminEmail,
                    Email = backupAdminEmail,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(1985, 5, 15),
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(backupAdminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(backupAdminUser, "Admin");
                }
            }

            // Seed sample data
            await SeedSampleData(context);
        }

        private static async Task SeedSampleData(ApplicationDbContext context)
        {
            // Add sample services
            if (!context.Services.Any())
            {
                var services = new List<Service>
                {
                    new Service { Name = "Personal Training", Description = "One-on-one training session", Duration = 60, Price = 150, Category = "Fitness", GymId = 1, IsActive = true },
                    new Service { Name = "Group Fitness", Description = "Group workout session", Duration = 45, Price = 50, Category = "Fitness", GymId = 1, IsActive = true },
                    new Service { Name = "Yoga Class", Description = "Relaxing yoga session", Duration = 60, Price = 75, Category = "Yoga", GymId = 1, IsActive = true },
                    new Service { Name = "Cardio Training", Description = "Cardiovascular workout", Duration = 30, Price = 40, Category = "Cardio", GymId = 1, IsActive = true },
                    new Service { Name = "Weight Training", Description = "Strength and muscle building", Duration = 45, Price = 60, Category = "Strength", GymId = 1, IsActive = true }
                };

                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }

            // Add sample trainers
            if (!context.Trainers.Any())
            {
                var trainers = new List<Trainer>
                {
                    new Trainer 
                    { 
                        FirstName = "Ahmet", 
                        LastName = "Yılmaz", 
                        Email = "ahmet@fitlifegym.com", 
                        Phone = "+90 555 123 4567",
                        Specialization = "Personal Training, Weight Training", 
                        Bio = "5 years of experience in fitness training",
                        Experience = 5,
                        GymId = 1 
                    },
                    new Trainer 
                    { 
                        FirstName = "Ayşe", 
                        LastName = "Demir", 
                        Email = "ayse@fitlifegym.com", 
                        Phone = "+90 555 234 5678",
                        Specialization = "Yoga, Group Fitness", 
                        Bio = "Certified yoga instructor with 3 years experience",
                        Experience = 3,
                        GymId = 1 
                    },
                    new Trainer 
                    { 
                        FirstName = "Mehmet", 
                        LastName = "Kaya", 
                        Email = "mehmet@fitlifegym.com", 
                        Phone = "+90 555 345 6789",
                        Specialization = "Cardio Training, Personal Training", 
                        Bio = "Former athlete, specializes in cardio workouts",
                        Experience = 7,
                        GymId = 1 
                    }
                };

                context.Trainers.AddRange(trainers);
                await context.SaveChangesAsync();

                // Add trainer services
                var trainerServices = new List<TrainerService>
                {
                    new TrainerService { TrainerId = 1, ServiceId = 1 }, // Ahmet - Personal Training
                    new TrainerService { TrainerId = 1, ServiceId = 5 }, // Ahmet - Weight Training
                    new TrainerService { TrainerId = 2, ServiceId = 2 }, // Ayşe - Group Fitness
                    new TrainerService { TrainerId = 2, ServiceId = 3 }, // Ayşe - Yoga Class
                    new TrainerService { TrainerId = 3, ServiceId = 1 }, // Mehmet - Personal Training
                    new TrainerService { TrainerId = 3, ServiceId = 4 }  // Mehmet - Cardio Training
                };

                context.TrainerServices.AddRange(trainerServices);
                await context.SaveChangesAsync();

                // Add trainer availabilities
                var availabilities = new List<TrainerAvailability>();
                
                // Ahmet's availability (Monday to Friday, 9-17)
                for (int day = 1; day <= 5; day++)
                {
                    availabilities.Add(new TrainerAvailability
                    {
                        TrainerId = 1,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsAvailable = true,
                        IsActive = true
                    });
                }

                // Ayşe's availability (Tuesday to Saturday, 10-18)
                for (int day = 2; day <= 6; day++)
                {
                    availabilities.Add(new TrainerAvailability
                    {
                        TrainerId = 2,
                        DayOfWeek = (DayOfWeek)(day % 7),
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        IsAvailable = true,
                        IsActive = true
                    });
                }

                // Mehmet's availability (Monday to Saturday, 8-16)
                for (int day = 1; day <= 6; day++)
                {
                    availabilities.Add(new TrainerAvailability
                    {
                        TrainerId = 3,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(16, 0, 0),
                        IsAvailable = true,
                        IsActive = true
                    });
                }

                context.TrainerAvailabilities.AddRange(availabilities);
                await context.SaveChangesAsync();
            }
        }
    }
}