using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.Models;
using WebAssignment.ViewModels;

namespace WebAssignment.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private WebAssignmentDbContext _context;

        public OrderController()
        {
            _context = new WebAssignmentDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: Order
        [HttpGet]
        public ActionResult Index()
        {
            var orders = new List<Order>();
            if (User.IsInRole(RoleName.Administrator))
            {
                orders = _context.Orders.ToList();
            }
            else {
                string currentId = User.Identity.GetUserId();
                orders = _context.Orders.Where(m => m.UserId == currentId).ToList();
            }
            
            var games = new List<Game>();
            foreach (Order order in orders) {
                var game = order.GetGame();
                if (!games.Contains(game)) {
                    games.Add(game);
                }
            }
            games = games.OrderBy(g => g.Date).ToList();

            var vm = new OrderIndexViewModel
            {
                Orders = orders,
                Games = games,
            };

            if (User.IsInRole(RoleName.Administrator))
            {
                return View(vm);
            }
            else
            {
                return View("MyOrders",vm);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleName.User)]
        public ActionResult New()
        {
            string query = "SELECT * FROM dbo.Game ORDER BY dbo.Game.CompetitionId, dbo.Game.Date";
            var games = _context.Database.SqlQuery<Game>(query).ToList();
            return View("GameSelection", games);
        }

        [HttpGet]
        [Authorize(Roles = RoleName.User)]
        public ActionResult TicketSelection(int gameId)
        {
            var game = _context.Games.SingleOrDefault(m => m.Id == gameId);
            if (game == null)
            {
                return HttpNotFound();
            }
            var unsoldTickets = game.GetAllUnsoldTickets();
            var categories = _context.Ref_Category.ToList();
            List<string> categoryNameList = new List<string>();
            foreach (Ref_Category ct in categories)
            {
                if (game.GetAllUnsoldTicketsCategorized(ct.Id).Count > 0)
                {
                    categoryNameList.Add(ct.Name);
                }
            }
            var vm = new TicketSelectionViewModel
            {
                AvailableTickets = unsoldTickets,
                Fixture = game.getFixture(),
                Categories = categoryNameList
            };
            return View("TicketSelection", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleName.User)]
        public ActionResult Form()
        {
            List<Ticket_Game> selectedTickets = new List<Ticket_Game>();
            foreach (string name in Request.Form.AllKeys)
            {
                try
                {
                    int n = Convert.ToInt32(name);
                    Ticket_Game tg = _context.Ticket_Game.Single(m => m.Id == n);
                    // Ref 1
                    if (tg.Sold == 1)
                    {
                        //already sold
                        return View("Error");
                    }
                    selectedTickets.Add(tg);
                }
                catch (Exception e) {

                }


            }
            return View(selectedTickets);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleName.User)]
        public ActionResult Pay()
        {
            Order order = new Order();
            double? totalPrice = 0.0;
            foreach (string reqName in Request.Form.AllKeys)
            {
                try
                {
                    int n = Convert.ToInt32(reqName);
                    Ticket_Game tg = _context.Ticket_Game.Single(m => m.Id == n);
                    var name = Request.Form[reqName];
                    totalPrice += tg.Price;
                    tg.Name = name;
                    tg.Sold = 1;
                    Order_Ticket_Game otg = new Order_Ticket_Game(tg.Id, order.Id);
                    _context.Order_Ticket_Game.Add(otg);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {

                }
            }
            order.Total = (double)totalPrice;
            order.Date = DateTime.Now;
            order.UserId = User.Identity.GetUserId();
            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction("Index", "Order");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var order = _context.Orders.Single(c => c.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (User.IsInRole(RoleName.User))
            {
                if (User.Identity.GetUserId() != order.UserId)
                {
                    return HttpNotFound();
                }
            }

            List<Ticket_Game> tickets = new List<Ticket_Game>();
            int configured = 0;
            var otgs = _context.Order_Ticket_Game.Where(m => m.OrderId == id).ToList();
            foreach (Order_Ticket_Game otg in otgs) {
                var tg = otg.GetTicketGame();
                if (tg.Configured == 1) {
                    configured = 1;
                }
                tickets.Add(tg);
            }

            var fixture = tickets.ElementAt(0).GetGame().getFixture();
            var email = order.GetUser().Email;
            var vm = new OrderDetailsViewModel
            {
                oid = order.Id,
                Fixture = fixture,
                Date = order.Date,
                UserEmail = email,
                Cost = order.Total,
                Tickets = tickets,
                Configured = configured
            };
            return View(vm);
            
        }

        [Authorize(Roles = RoleName.Administrator)]
        public ActionResult ConfigureAll(int id) {
            var order = _context.Orders.Single(c => c.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var otgs = _context.Order_Ticket_Game.Where(m => m.OrderId == id).ToList();
            foreach (Order_Ticket_Game otg in otgs)
            {
                var tg = otg.GetTicketGame();
                var tgInDb = _context.Ticket_Game.SingleOrDefault(m => m.Id == tg.Id); 
                tgInDb.Configured = 1;
            }
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = id});

        }
    }
}