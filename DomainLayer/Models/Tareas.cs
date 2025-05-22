namespace DomainLayer.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Tareas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MinLength(5), MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pendiente";

        [MaxLength(1000)]
        public string? AdditionalData { get; set; } = string.Empty;

        [NotMapped]
        public int DiasRestantes => (DueDate - DateTime.Now).Days;

        [NotMapped]
        public bool EstaVencida => DueDate < DateTime.Now;
    }
}
