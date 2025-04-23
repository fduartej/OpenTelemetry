using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReFactoring.Models
{
    [Table("t_customer")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserName { get; set; }
        [Required(ErrorMessage = "DNI es obligatorios.")]
        public string? DNI { get; set; }
        [Required(ErrorMessage = "Fecha nacimiento es obligatorios.")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }
        [Required(ErrorMessage = "Direccion es obligatorios.")]
        public string? Address { get; set; }
    }
}