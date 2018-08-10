using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class TicketDetailsViewModel
    {
        public int Id { get; set; }
        public string Owner { get; set; }
        public string Position { get; set; }
        public string Category { get; set; }
        public IEnumerable<Ticket_Game> Games { get; set; }
        public IEnumerable<string> Competitions { get; set; }
    }
}