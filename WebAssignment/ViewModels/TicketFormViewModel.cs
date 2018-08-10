using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class TicketFormViewModel
    {
        public IEnumerable<Ref_Category> Categories { get; set; }

        public int Id { get; set; }

        [Required]
        [StringLength(225)]
        public string Owner { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required][Range(1, int.MaxValue)]
        public int Section { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Row { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Seat { get; set; }

        [StringLength(225)]
        [Display(Name = "View Upload")]
        public string ViewUpload { get; set; }

        public IEnumerable<string> SelectedGames { get; set; }
        public IEnumerable<SelectListItem> Games { get; set; }
        public IEnumerable<SelectListGroup> Groups { get; set; }
    }
}