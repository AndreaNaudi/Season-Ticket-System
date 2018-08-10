using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAssignment.Models
{
    // ALL HIDDEN FIELDS SHOULD NOT HAVE [REQUIRED]
    public class TicketMetadata
    {
        

        
    }

    public class GameMetadata
    {
                
    }

    public class Ticket_GameMetadata
    {
        public int TicketId { get; set; }
        public int GameId { get; set; }

    }

    public class Order_Ticket_GameMetadata
    {
        public int Id { get; set; }
        public int Ticket_Game_Id { get; set; }
        public int OrderId { get; set; }

        public virtual Order Order { get; set; }
        public virtual Ticket_Game Ticket_Game { get; set; }

    }

    public class OrderMetadata {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double Total { get; set; }
        public System.DateTime Date { get; set; }

        public virtual ICollection<Order_Ticket_Game> Order_Ticket_Game { get; set; }
    }

    public class ReviewMetadata
    {
        
    }
}