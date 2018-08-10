using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class TicketSelectionViewModel
    {
        public IEnumerable<Ticket_Game> AvailableTickets { get; set; }
        public string Fixture { get; set; }
        public IEnumerable<string> Categories { get; set; }
    }
}