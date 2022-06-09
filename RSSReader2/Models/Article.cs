using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSSReader2.Models
{
    public class Article
    {
        public int Id { set; get; }
        [Display(Name = "Název")]
        public string? Title { set; get; }
        [Display(Name = "Obsah")]
        public string? Description { set; get; }
        [Display(Name = "Odkaz")]
        public string? Link { set; get; }
        [DataType(DataType.Date)]
        [Display(Name = "Datum publikování")]
        public DateTime? PubDate { set; get; }
        public int RSSFeedId { set; get; }
        public virtual RSSFeed? RSSFeed { set; get; }
    }
}
