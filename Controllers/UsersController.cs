using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using DemoPracticeSecurityNet.Data;
using DemoPracticeSecurityNet.Models;

namespace DemoPracticeSecurityNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // IDOR vulnérable
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<User>> GetUserProfile(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user; // Renvoie toutes les données sans vérification
        }

        // Injection SQL
        [HttpGet("search")]
        public IActionResult SearchUsers(string query)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var command = new SqlCommand($"SELECT * FROM Users WHERE Username LIKE '%{query}%'", connection);
            var reader = command.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return Ok(dt);
        }

        // XSS
        [HttpPost("comments")]
        public ContentResult AddComment([FromBody] string comment)
        {
            return new ContentResult
            {
                Content = $"<div class='comment'>{comment}</div>",
                ContentType = "text/html"
            };
        }

        // Mass Assignment
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}