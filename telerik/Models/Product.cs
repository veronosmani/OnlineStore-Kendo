using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace telerik.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Range(0, int.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int UnitsInStock { get; set; }

        public bool Discontinued { get; set; }

        [Range(0, int.MaxValue)]
        public int UnitsOnOrder { get; set; }

        public int CategoryID { get; set; }
        [NotMapped]
        public string? CategoryName { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }  // used only in form

        public string? ImageUrl { get; set; }  // store image path    
    }
    }