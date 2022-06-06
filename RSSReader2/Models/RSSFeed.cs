using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSSReader2.Models
{
    public class RSSFeed
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Toto pole je povinné")]
        public string? Url { get; set; }
        [Required(ErrorMessage = "Toto pole je povinné")]
        [Display(Name = "Název")]
        public string? Name { get; set; }
        [Display(Name = "Články")]
        public virtual ICollection<Article>? Articles { set; get; } 
    }
}
