using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int oid { get; set; }
        public string Fixture { get; set; }
        public string UserEmail { get; set; }
        public double Cost { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<Ticket_Game> Tickets { get; set; }
        public int Configured { get; set; }
    }
}