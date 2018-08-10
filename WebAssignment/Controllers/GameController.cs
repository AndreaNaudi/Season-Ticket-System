using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAssignment.Models;
using WebAssignment.ViewModels;

namespace WebAssignment.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    public class GameController : Controller
    {
        private WebAssignmentDbContext _context;

        public GameController()
        {
            _context = new WebAssignmentDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: Ticket
        [HttpGet]
        public ActionResult Index()
        {
            var games = _context.Games.ToList();

            return View(games);
        }

        [HttpGet]
        public ActionResult New()
        {
            var tickets = _context.Tickets.ToList().OrderBy(m => m.Section);
            List<SelectListItem> listSelectListItems = new List<SelectListItem>();

            var categories = _context.Ref_Category.ToList();
            List<SelectListGroup> groups = new List<SelectListGroup>();

            foreach (Ref_Category ct in categories)
            {
                var group = new SelectListGroup() { Name = ct.Name.Replace(" ", "&nbsp") };
                groups.Add(group);
            }

            foreach (Ticket t in tickets)
            {
                var grp = new SelectListGroup();
                switch (t.CategoryId)
                {
                    case 1:
                        grp = groups.ElementAt(0);
                        break;
                    case 2:
                        grp = groups.ElementAt(1);
                        break;
                    case 3:
                        grp = groups.ElementAt(2);
                        break;
                }
                SelectListItem selectList = new SelectListItem()
                {
                    Text = t.getPosition(),
                    Value = t.Id.ToString(),
                    Group = grp
                };
                listSelectListItems.Add(selectList);
            }

            var competitions = _context.Ref_Competition.ToList();

            var viewModel = new GameFormViewModel
            {
                Competitions = competitions,
                Tickets = listSelectListItems,
                Groups = groups
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(GameFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                //foreach (ModelState modelState in ViewData.ModelState.Values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {
                //        return Content(error.ErrorMessage);
                //    }
                //}
                var viewModel = new GameFormViewModel
                {
                    HomeTeam = vm.HomeTeam,
                    AwayTeam = vm.AwayTeam,
                    CompetitionId = vm.CompetitionId,
                    Date = vm.Date,
                    Competitions = _context.Ref_Competition.ToList()
                };
                return View("Form", viewModel);
            }

            Dictionary<int, double> Prices = new Dictionary<int, double>();
            bool editMode = false;
            Game game = null;
            if (vm.Id == 0)
            {
                game = new Game()
                {
                    Id = vm.Id,
                    HomeTeam = vm.HomeTeam,
                    AwayTeam = vm.AwayTeam,
                    CompetitionId = vm.CompetitionId,
                    Date = vm.Date,
                };
                _context.Games.Add(game);
            }
            else
            {
                editMode = true;
                var gameInDb = _context.Games.Single(c => c.Id == vm.Id);
                gameInDb.HomeTeam = vm.HomeTeam;
                gameInDb.AwayTeam = vm.AwayTeam;
                gameInDb.Date = vm.Date;
                gameInDb.CompetitionId = vm.CompetitionId;
                game = gameInDb;
                string query = "Select TicketId, Price From Ticket_Game Where Sold = 0 and GameId = " + gameInDb.Id + ";";
                var ticket_prices = _context.Database.SqlQuery<TicketPricePair>(query).ToList();
                foreach (TicketPricePair tp in ticket_prices)
                {
                    Prices.Add(tp.TicketId, tp.Price);
                }
                //remove existing data
                //_context.Database.ExecuteSqlCommand("Delete from Ticket_Game Where Sold=0 and GameId = " + game.Id);
                var toRemove = _context.Ticket_Game.Where(m => m.GameId == vm.Id && m.Sold == 0);
                _context.Ticket_Game.RemoveRange(toRemove);

            }

            _context.SaveChanges();
            if (vm.SelectedTickets != null)
            {
                foreach (string s in vm.SelectedTickets)
                {
                    double price = 0;
                    var ticket = _context.Tickets.SingleOrDefault(m => m.Id.ToString() == s);
                    if (editMode)
                    {
                        if (Prices.ContainsKey(ticket.Id))
                        {
                            price = Prices[ticket.Id];
                        }
                        else
                        {
                            switch (ticket.CategoryId)
                            {
                                case 1: price = 250; break;//Boniperti
                                case 2: price = 200; break;//Sivori
                                case 3: price = 150; break;//Regular

                            }
                        }

                    }
                    else
                    {
                        switch (ticket.CategoryId)
                        {
                            case 1: price = 250; break;//Boniperti
                            case 2: price = 200; break;//Sivori
                            case 3: price = 150; break;//Regular

                        }
                    }

                    //string query = "Insert into dbo.Ticket_Game(TicketId, GameId, Price) values(" + ticket.Id + ", " + game.Id + ", " + price + ")";
                    //_context.Database.ExecuteSqlCommand(query);
                    Ticket_Game tg = new Ticket_Game(ticket.Id, game.Id, price);
                    _context.Ticket_Game.Add(tg);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Game");
        }

        [HttpGet]
        public ActionResult Edit(int id, int? valid)
        {
            var game = _context.Games.SingleOrDefault(c => c.Id == id);

            if (game == null)
            {
                return HttpNotFound();
            }
            string q = "Select * From dbo.Ticket WHERE dbo.Ticket.Id NOT IN(Select dbo.Ticket.Id From dbo.Ticket JOIN dbo.Ticket_Game ON dbo.Ticket.Id = dbo.Ticket_Game.TicketId WHERE dbo.Ticket_Game.Sold = 1 and dbo.Ticket_Game.GameId =" + game.Id + ")";
            var alltickets = _context.Database.SqlQuery<Ticket>(q).ToList();

            List<SelectListItem> listSelectListItems = new List<SelectListItem>();
            string query = "SELECT * FROM dbo.Ticket JOIN dbo.Ticket_Game ON dbo.Ticket.Id = dbo.Ticket_Game.TicketId WHERE dbo.Ticket_Game.Sold = 0 and dbo.Ticket_Game.GameId = " + game.Id + "; ";
            var ticketsInDbUnsold = _context.Database.SqlQuery<Ticket>(query).ToList();

            List<SelectListGroup> groups = new List<SelectListGroup>();
            var categories = _context.Ref_Category.ToList();

            foreach (Ref_Category ct in categories)
            {
                var group = new SelectListGroup() { Name = ct.Name.Replace(" ", "&nbsp") };
                groups.Add(group);
            }

            foreach (Ticket t in alltickets)
            {
                var grp = new SelectListGroup();
                switch (t.CategoryId)
                {
                    case 1:
                        grp = groups.ElementAt(0);
                        break;
                    case 2:
                        grp = groups.ElementAt(1);
                        break;
                    case 3:
                        grp = groups.ElementAt(2);
                        break;
                }

                SelectListItem selectList = new SelectListItem()
                {
                    Text = t.getPosition(),
                    Value = t.Id.ToString(),
                    Group = grp
                };
                listSelectListItems.Add(selectList);
            }

            List<string> selected = new List<string>();
            foreach (Ticket t in ticketsInDbUnsold)
            {
                string s = t.getPosition();
                selected.Add(s);
            }

            var viewModel = new GameFormViewModel
            {
                Id = game.Id,
                HomeTeam = game.HomeTeam,
                AwayTeam = game.AwayTeam,
                CompetitionId = game.CompetitionId,
                Date = game.Date,
                Competitions = _context.Ref_Competition.ToList(),
                Tickets = listSelectListItems,
                SelectedTickets = selected,
                Groups = groups
            };
            return View("Form", viewModel);
        }

        [HttpGet]
        public ActionResult Remove(int id)
        {
            var game = _context.Games.Single(c => c.Id == id);
            var soldTickets = game.GetAllSoldTickets();
            if (game == null)
            {
                return HttpNotFound();
            }

            if (soldTickets.Count > 0)
            {
                return RedirectToAction("Edit", "Game", new { id = game.Id, valid = 0});
            }
            else
            {
                var tgs = _context.Ticket_Game.Where(m => m.GameId == game.Id).ToList();
                foreach (Ticket_Game tg in tgs) {
                    _context.Ticket_Game.Remove(tg);
                }
                _context.Games.Remove(game);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Game");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var game = _context.Games.Single(c => c.Id == id);

            if (game == null)
            {
                return HttpNotFound();
            }
            else
            {
                var categories = _context.Ref_Category.ToList();
                List<string> categoryNameList = new List<string>();
                foreach (Ref_Category ct in categories)
                {
                    if (game.GetAllAssignedTicketsCategorized(ct.Id).Count > 0)
                    {
                        categoryNameList.Add(ct.Name);
                    }
                }

                var vm = new GameDetailsViewModel
                {
                    Id = game.Id,
                    Fixture = game.getFixture(),
                    Competition = game.GetCompetition().Name,
                    Date = game.Date,
                    Tickets = game.GetAllAssignedTickets(),
                    Categories = categoryNameList
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrices(int id)
        {
            var game = _context.Games.Single(c => c.Id == id);
            if (game == null)
            {
                return HttpNotFound();
            }
            var unsoldTickets = game.GetAllUnsoldTickets();
            foreach (Ticket_Game tg in unsoldTickets) {
                var price = Request[tg.TicketId.ToString()];
                if (price == "") {
                    price = "0";
                }
                double? priceDouble = Convert.ToDouble(price);
                Ticket_Game toUpdate = _context.Ticket_Game.Single(m => m.GameId == game.Id && m.TicketId == tg.TicketId);
                toUpdate.Price = priceDouble;
                _context.SaveChanges();
                //string query = "UPDATE dbo.Ticket_Game SET Price = " + price + " WHERE TicketId = " + tg.TicketId + " and GameId = " + game.Id + "; ";
                //_context.Database.ExecuteSqlCommand(query);
            }
            return RedirectToAction("Index", "Game");
        }
    }

}