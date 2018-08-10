using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAssignment.Models;
namespace WebAssignment.ViewModels
{
    public class GameDetailsViewModel
    {
        public int Id { get; set; }
        public string Fixture { get; set; }
        public string Competition { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<Ticket_Game> Tickets { get; set; }
        public IEnumerable<string> Categories { get; set; }
    }
}