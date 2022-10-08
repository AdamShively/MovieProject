using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Data;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;

namespace MovieProject.Services
{
    public class SeedService
    {

        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public SeedService(IOptions<AppSettings> appSettings, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        //wrapper method
        public async Task ManageDataAsync()
        {
            await UpdateDatabaseAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCollections();
        }

        private async Task UpdateDatabaseAsync()
        {
            await _dbContext.Database.MigrateAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (_dbContext.Roles.Any())
            {
                return;
            }

            var adminRole = _config["DefaultCredentials:Role"];

            await _roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        private async Task SeedUsersAsync()
        {
            if (_userManager.Users.Any())
            {
                return;
            }

            var newUser = new IdentityUser()
            {
                Email = _config["DefaultCredentials:Email"],
                UserName = _config["DefaultCredentials:DisplayName"],
                EmailConfirmed = true
            };

            //Create new user in db with password.
            await _userManager.CreateAsync(newUser, _config["DefaultCredentials:Password"]);

            //Add role of admin to existing user in db.
            await _userManager.AddToRoleAsync(newUser, _config["DefaultCredentials:Role"]);
        }

        private async Task SeedCollections()
        {
            if (_dbContext.Collection.Any())
            {
                return;
            }

            _dbContext.Add(new Collection()
            {
                Name = _appSettings.MovieProjectSettings.DefaultCollection.Name,
                Description = _appSettings.MovieProjectSettings.DefaultCollection.Description
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}
