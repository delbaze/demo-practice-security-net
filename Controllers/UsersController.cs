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
            if (user == null)
            {
                return NotFound();
            }
            return user;
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

            var result = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                result.Add(row);
            }

            return Ok(result);
        }

        //// XSS
        //[HttpPost("comments")]
        //public ContentResult AddComment([FromBody] string comment)
        //{
        //    return new ContentResult
        //    {
        //        Content = $"<div class='comment'>{comment}</div>",
        //        ContentType = "text/html"
        //    };
        //}

        [HttpPost("comments")]
        [Consumes("application/x-www-form-urlencoded")]
        public ContentResult AddComment([FromForm] string comment)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Commentaire ajouté</title>
            </head>
            <body>
                <h1>Votre commentaire :</h1>
                <div class='comment'>{comment}</div>
                
                <h2>Ajouter un autre commentaire :</h2>
                <form method='post' action='/api/users/comments'>
                    <textarea name='comment'></textarea>
                    <button type='submit'>Envoyer</button>
                </form>
            </body>
            </html>";

            return new ContentResult
            {
                Content = html,
                ContentType = "text/html",
                StatusCode = 200
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