using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Data.Contexts;
using System;
using System.Threading.Tasks;

namespace UserProvider.Functions
{
    public class DeleteUser
    {
        private readonly ILogger<DeleteUser> _logger;
        private readonly DataContext _context;

        public DeleteUser(ILogger<DeleteUser> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("DeleteUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/delete/{email}")] HttpRequest req, string email)
        {
            _logger.LogInformation("DeleteUser function processed a request.");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

                if (user == null)
                {
                    return new NotFoundResult();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error deleting user: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
