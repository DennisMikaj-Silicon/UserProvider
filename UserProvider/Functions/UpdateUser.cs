using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserProvider.Functions
{
    public class UpdateUser
    {
        private readonly ILogger<UpdateUser> _logger;
        private readonly DataContext _context;

        public UpdateUser(ILogger<UpdateUser> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("UpdateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{email}")] HttpRequest req, string email)
        {
            _logger.LogInformation("UpdateUser function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UpdateUserModel updateUserModel;

            try
            {
                updateUserModel = JsonSerializer.Deserialize<UpdateUserModel>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                return new BadRequestObjectResult("Invalid JSON data.");
            }

            if (updateUserModel == null || updateUserModel.Email == null)
            {
                return new BadRequestObjectResult("Invalid user data or email mismatch.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new NotFoundResult();
            }

            user.FirstName = updateUserModel.FirstName;
            user.LastName = updateUserModel.LastName;
            user.Email = updateUserModel.Email;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public class UpdateUserModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }
    }
}
