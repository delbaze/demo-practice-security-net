using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;
using DemoPracticeSecurityNet.Data;
using DemoPracticeSecurityNet.Models;

using DemoPracticeSecurityNet.DTOs;

namespace DemoPracticeSecurityNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ajout de l'authentification
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HtmlEncoder _htmlEncoder;

        public UsersController(
            ApplicationDbContext context,
            HtmlEncoder htmlEncoder)
        {
            _context = context;
            _htmlEncoder = htmlEncoder;
        }

        // Correction IDOR - V�rification de l'autorisation
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserDto>> GetUserProfile(int id)
        {
            // R�cup�ration de l'ID de l'utilisateur connect�
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            // V�rification que l'utilisateur acc�de � son propre profil ou est admin
            if (id != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Utilisation d'un DTO pour limiter les donn�es expos�es
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        // Correction Injection SQL - Utilisation de param�tres et EF Core
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required");
            }

            var users = await _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        // Correction XSS - Encodage HTML et validation des entr�es
        [HttpPost("comments")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult AddComment([FromForm] string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return BadRequest("Comment cannot be empty");
            }

            // Encodage HTML pour pr�venir le XSS
            var encodedComment = _htmlEncoder.Encode(comment);

            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Commentaire ajout�</title>
                <meta http-equiv='Content-Security-Policy' 
                      content=""default-src 'self'; 
                               script-src 'self';
                               style-src 'self';"">
            </head>
            <body>
                <h1>Votre commentaire :</h1>
                <div class='comment'>{encodedComment}</div>
                
                <h2>Ajouter un autre commentaire :</h2>
                <form method='post' action='/api/users/comments'>
                    <textarea name='comment' maxlength='1000' required></textarea>
                    <button type='submit'>Envouter</button>
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

        // Correction Mass Assignment - Utilisation d'un DTO et validation explicite
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                // Hashage du mot de passe - � impl�menter avec un service d�di�
                Password = HashPassword(userDto.Password),
                IsAdmin = false // Valeur par d�faut forc�e
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
        }

        private string HashPassword(string password)
        {
            // Impl�menter un hashage s�curis� ici
            // Exemple avec BCrypt.Net-Next :
            // return BCrypt.HashPassword(password);
            throw new NotImplementedException("Implement secure password hashing");
        }

    }
}
    

