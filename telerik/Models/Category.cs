using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace telerik.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        // Navigation property to products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
