using System.ComponentModel.DataAnnotations;

namespace DemoPracticeSecurityNet.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;  // Stocké en clair intentionnellement
        public string Email { get; set; } = string.Empty;
        public string CreditCard { get; set; } = string.Empty; // Données sensibles exposées
        public bool IsAdmin { get; set; }
        public string PrivateNotes { get; set; } = string.Empty;
    }
}