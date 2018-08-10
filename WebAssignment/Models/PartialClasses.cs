using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WebAssignment.Models;

namespace WebAssignment.Models
{
    [MetadataType(typeof(TicketMetadata))]
    public partial class Ticket
    {
        public string getPosition()
        {
            string s = "Section " + Section + " - Row " + Row + " - Seat " + Seat;
            return s;
        }

        public List<Ticket_Game> GetAllUnsoldGames()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket_Game WHERE sold = 0 and dbo.Ticket_Game.TicketId = " + Id + "; ";
            var gamesInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return gamesInDb;
        }

        public List<Ticket_Game> GetAllAssignedGamesByCompetition(int competitionId)
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Game JOIN dbo.Ticket_Game ON dbo.Game.Id = dbo.Ticket_Game.GameId WHERE dbo.Ticket_Game.TicketId = " + Id + " and " + "dbo.Game.CompetitionId = " + competitionId + ";";
            var gamesInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return gamesInDb;
        }

        public List<Ticket_Game> GetAllAssignedGames()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Game JOIN dbo.Ticket_Game ON dbo.Game.Id = dbo.Ticket_Game.GameId WHERE dbo.Ticket_Game.TicketId = " + Id + "; ";
            var gamesInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return gamesInDb;
        }

        public List<Ticket_Game> GetAllSoldGames()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket_Game WHERE sold = 1 and dbo.Ticket_Game.TicketId = " + Id + "; ";
            var gamesInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return gamesInDb;
        }
    }

    [MetadataType(typeof(GameMetadata))]
    public partial class Game
    {
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Game temp = (Game)obj;
            return Id == temp.Id;
        }

        public string getFixture()
        {
            string s = HomeTeam + " vs " + AwayTeam;
            return s;
        }

        public Ref_Competition GetCompetition()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            Ref_Competition competition = db.Ref_Competition.SingleOrDefault(m => m.Id == CompetitionId);
            return competition;
        }

        public List<Ticket_Game> GetAllAssignedTickets()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket JOIN dbo.Ticket_Game ON dbo.Ticket.Id = dbo.Ticket_Game.TicketId WHERE dbo.Ticket_Game.GameId = " + Id + "; ";
            var ticketsInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();
            
            return ticketsInDb;
        }

        public List<Ticket_Game> GetAllUnsoldTickets()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket_Game WHERE sold = 0 and dbo.Ticket_Game.GameId = " + Id + "; ";
            var ticketsInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return ticketsInDb;
        }

        public List<Ticket_Game> GetAllSoldTickets()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket_Game WHERE sold = 1 and dbo.Ticket_Game.GameId = " + Id + "; ";
            var ticketsInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return ticketsInDb;
        }

        public List<Ticket_Game> GetAllAssignedTicketsCategorized(int categoryId)
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket JOIN dbo.Ticket_Game ON dbo.Ticket.Id = dbo.Ticket_Game.TicketId WHERE dbo.Ticket_Game.GameId = " + Id + " and " + "dbo.Ticket.CategoryId = " + categoryId + ";";
            var ticketsInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return ticketsInDb;
        }

        public List<Ticket_Game> GetAllUnsoldTicketsCategorized(int categoryId)
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT * FROM dbo.Ticket JOIN dbo.Ticket_Game ON dbo.Ticket.Id = dbo.Ticket_Game.TicketId WHERE dbo.Ticket_Game.Sold=0 and dbo.Ticket_Game.GameId = " + Id + " and " + "dbo.Ticket.CategoryId = " + categoryId + ";";
            var ticketsInDb = db.Database.SqlQuery<Ticket_Game>(query).ToList();

            return ticketsInDb;
        }

    }

    [MetadataType(typeof(ReviewMetadata))]
    public partial class Review
    {
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public ApplicationUser GetUser() {
            ApplicationDbContext db = new ApplicationDbContext();
            //string query = "Select * From dbo.AspNetUsers Where Id = " + UserId;
            var user = db.Users.SingleOrDefault(m => m.Id == UserId);
            //var user = db.Database.SqlQuery<ApplicationUser>(query).Single(); 
            return user;
        }
    }

    public partial class TicketPricePair
    {
        public int TicketId { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return TicketId + " => " + Price;
        }
    }

    public partial class GamePricePair
    {
        public int GameId { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return GameId + " => " + Price;
        }
    }

    [MetadataType(typeof(Ticket_GameMetadata))]
    public partial class Ticket_Game
    {
        public Ticket_Game(int TicketId, int GameId, double Price)
        {
            this.TicketId = TicketId;
            this.GameId = GameId;
            this.Price = Price;
        }

        public Ticket GetTicket()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            var ticket = db.Tickets.SingleOrDefault(t => t.Id == TicketId);
            return ticket;
        }

        public Game GetGame()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            var game = db.Games.SingleOrDefault(t => t.Id == GameId);
            return game;
        }
    }

    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
        public List<Ticket_Game> GetTicketGames()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            string query = "SELECT Ticket_Game_Id FROM dbo.Order_Ticket_Game Where OrderId = " + Id + ";";
            var tgids = db.Database.SqlQuery<int>(query).ToList();

            List<Ticket_Game> tglist = new List<Ticket_Game>();
            foreach (int id in tgids) {
                Ticket_Game tg = db.Ticket_Game.Single(m => m.Id == id);
                tglist.Add(tg);
            }

            return tglist;
        }

        public Game GetGame() {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            var game = GetTicketGames().ElementAt(0).GetGame();
            return game;
        }

        public ApplicationUser GetUser()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            //string query = "Select * From dbo.AspNetUsers Where Id = " + UserId;
            var user = db.Users.SingleOrDefault(m => m.Id == UserId);
            //var user = db.Database.SqlQuery<ApplicationUser>(query).Single(); 
            return user;
        }
    }

    [MetadataType(typeof(Order_Ticket_GameMetadata))]
    public partial class Order_Ticket_Game
    {
        public Order_Ticket_Game(int Ticket_Game_Id, int OrderId)
        {
            this.Ticket_Game_Id = Ticket_Game_Id;
            this.OrderId = OrderId;
        }

        public Order_Ticket_Game()
        {
        }

        public Order GetOrder() {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            var order = db.Orders.SingleOrDefault(t => t.Id == OrderId);
            return order;
        }

        public Ticket_Game GetTicketGame()
        {
            WebAssignmentDbContext db = new WebAssignmentDbContext();
            var tg = db.Ticket_Game.SingleOrDefault(t => t.Id == Ticket_Game_Id);
            return tg;
        }
    }
}