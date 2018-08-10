using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class GameFormViewModel
    {
        public IEnumerable<Ref_Competition> Competitions { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(225)]
        [Display(Name = "Home Team")]
        public string HomeTeam { get; set; }

        [Required]
        [StringLength(225)]
        [Display(Name = "Away Team")]
        public string AwayTeam { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString =
        "{0:DD/MM/YYYY HH:mm}",
         ApplyFormatInEditMode = true)]
        public System.DateTime Date { get; set; }

        [Required]
        [Display(Name = "Competition")]
        public int CompetitionId { get; set; }
        public IEnumerable<string> SelectedTickets { get; set; }
        public IEnumerable<SelectListItem> Tickets { get; set; }
        public IEnumerable<SelectListGroup> Groups { get; set; }
    }
}