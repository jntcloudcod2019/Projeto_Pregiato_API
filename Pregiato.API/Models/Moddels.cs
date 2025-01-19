using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Pregiato.API.Models
{
    public class Moddels
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IdModel { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres.")]
        public string CPF { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "RG deve ter no máximo 20 caracteres.")]
        public string RG { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres.")]
        public string PostalCode { get; set; }

        [StringLength(255, ErrorMessage = "Endereço deve ter no máximo 255 caracteres.")]
        public string Address { get; set; }

        [StringLength(30, ErrorMessage = "Conta bancária deve ter no máximo 30 caracteres.")]
        public string BankAccount { get; set; }

        [Required]
        public bool Status { get; set; } = true;

        [Required]
        [StringLength(255, ErrorMessage = "Senha criptografada deve ter no máximo 255 caracteres.")]
        public string PasswordHash { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
