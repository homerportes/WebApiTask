using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Tareas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MinLength(5, ErrorMessage = "La descripción debe tener al menos 5 caracteres")]
        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [MaxLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Los datos adicionales no pueden exceder los 1000 caracteres")]
        public string? AdditionalData { get; set; } = string.Empty;

        [NotMapped]
        public int DiasRestantes => (DueDate - DateTime.Now).Days;

        [NotMapped]
        public bool EstaVencida => DueDate < DateTime.Now;

        public bool EsValida()
        {
            return !string.IsNullOrEmpty(Description)
                && Description.Length >= 5
                && !string.IsNullOrEmpty(Status)
                && DueDate > DateTime.MinValue;
        }

        public bool FechaEsValida()
        {
            return DueDate > DateTime.Now;
        }

        public Tareas()
        {
            Status = "Pendiente";
            DueDate = DateTime.Now.AddDays(1);
        }

        public Tareas(string descripcion, DateTime fechaVencimiento, string estado)
        {
            Description = descripcion;
            DueDate = fechaVencimiento;
            Status = estado;
        }
    }
}