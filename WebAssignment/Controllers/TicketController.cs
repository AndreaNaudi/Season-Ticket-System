using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.ViewModels;
using WebAssignment.Models;
using System.IO;

namespace WebAssignment.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    public class TicketController : Controller
    {
        private WebAssignmentDbContext _context;

        public TicketController()
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
            var tickets = _context.Tickets.ToList();

            return View(tickets);
        }

        [HttpGet]
        public ActionResult New()
        {
            var categories = _context.Ref_Category.ToList();

            var games = _context.Games.ToList().OrderBy(m => m.Date);
            List<SelectListItem> listSelectListItems = new List<SelectListItem>();

            var competitions = _context.Ref_Competition.ToList();
            List<SelectListGroup> groups = new List<SelectListGroup>();

            foreach (Ref_Competition cp in competitions)
            {
                var group = new SelectListGroup() { Name = cp.Name.Replace(" ", "&nbsp") };
                groups.Add(group);
            }

            foreach (Game g in games)
            {
                var grp = new SelectListGroup();
                switch (g.CompetitionId)
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
                    Text = g.getFixture(),
                    Value = g.Id.ToString(),
                    Group = grp
                };
                listSelectListItems.Add(selectList);
            }

            var viewModel = new TicketFormViewModel
            {
                Categories = categories,
                Games = listSelectListItems,
                Groups = groups
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(TicketFormViewModel vm, HttpPostedFileBase postedFile)
        {
            //Ticket ticket = vm.Ticket;
            if (!ModelState.IsValid)
            {
                //foreach (ModelState modelState in ViewData.ModelState.Values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {
                //        return Content(error.ErrorMessage);
                //    }
                //}
                var viewModel = new TicketFormViewModel
                {
                    Owner = vm.Owner,
                    CategoryId = vm.CategoryId,
                    Section = vm.Section,
                    Seat = vm.Seat,
                    Row = vm.Row,
                    ViewUpload = vm.ViewUpload,
                    Categories = _context.Ref_Category.ToList()
                };
                return View("Form", viewModel);
            }

            if (postedFile != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                postedFile.SaveAs(path + Path.GetFileName(postedFile.FileName.Replace(' ', '_')));
            }

            Dictionary<int, double> Prices = new Dictionary<int, double>();
            bool editMode = false;
            Ticket ticket = null;
            if (vm.Id == 0)
            {
                ticket = new Ticket()
                {
                    Id = vm.Id,
                    CategoryId = vm.CategoryId,
                    Section = vm.Section,
                    Row = vm.Row,
                    Seat = vm.Seat,
                    Owner = vm.Owner
                };
                if (postedFile != null) {
                    ticket.ViewUpload = postedFile.FileName.Replace(' ', '_');
                }
                
                _context.Tickets.Add(ticket);                
            }
            else
            {
                editMode = true;
                

                var ticketInDb = _context.Tickets.Single(c => c.Id == vm.Id);
                ticketInDb.Owner = vm.Owner;
                ticketInDb.CategoryId = vm.CategoryId;
                ticketInDb.Section = vm.Section;
                ticketInDb.Row = vm.Row;
                ticketInDb.Seat = vm.Seat;
                if (postedFile != null)
                {
                    ticketInDb.ViewUpload = postedFile.FileName.Replace(' ', '_');
                }
                string query = "Select GameId, Price From Ticket_Game Where Sold = 0 and TicketId = " + ticketInDb.Id + ";";
                var game_prices = _context.Database.SqlQuery<GamePricePair>(query).ToList();
                foreach (GamePricePair tp in game_prices)
                {
                    Prices.Add(tp.GameId, tp.Price);
                }
                //remove existing data
                //_context.Database.ExecuteSqlCommand("Delete from Ticket_Game Where Sold=0 and TicketId = " + ticket.Id);
                var toRemove = _context.Ticket_Game.Where(m => m.TicketId == vm.Id && m.Sold == 0);
                _context.Ticket_Game.RemoveRange(toRemove);

                if (vm.ViewUpload != null)
                {
                    ticketInDb.ViewUpload = vm.ViewUpload.Replace(' ', '_');
                }
                //else keep same
                ticket = ticketInDb;

            }

            _context.SaveChanges();
            if (vm.SelectedGames != null)
            {
                foreach (string s in vm.SelectedGames)
                {
                    double price = 0;
                    var game = _context.Games.SingleOrDefault(m => m.Id.ToString() == s);
                    if (editMode)
                    {
                        if (Prices.ContainsKey(game.Id))
                        {
                            price = Prices[game.Id];
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
            return RedirectToAction("Index", "Ticket");
        }

        [HttpGet]
        public ActionResult Edit(int id, int? valid)
        {
            var ticket = _context.Tickets.SingleOrDefault(c => c.Id == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            string q = "Select * From dbo.Game WHERE dbo.Game.Id NOT IN(Select dbo.Game.Id From dbo.Game JOIN dbo.Ticket_Game ON dbo.Game.Id = dbo.Ticket_Game.GameId WHERE dbo.Ticket_Game.Sold = 1 and dbo.Ticket_Game.TicketId =" + ticket.Id + ")";
            var allgames = _context.Database.SqlQuery<Game>(q).ToList();

            List<SelectListItem> listSelectListItems = new List<SelectListItem>();
            string query = "SELECT * FROM dbo.Game JOIN dbo.Ticket_Game ON dbo.Game.Id = dbo.Ticket_Game.GameId WHERE dbo.Ticket_Game.Sold = 0 and dbo.Ticket_Game.TicketId = " + ticket.Id + "; ";
            var gamesInDbUnsold = _context.Database.SqlQuery<Game>(query).ToList();

            List<SelectListGroup> groups = new List<SelectListGroup>();
            var competitions = _context.Ref_Competition.ToList();

            foreach (Ref_Competition cp in competitions)
            {
                var group = new SelectListGroup() { Name = cp.Name.Replace(" ", "&nbsp") };
                groups.Add(group);
            }

            foreach (Game g in allgames)
            {
                var grp = new SelectListGroup();
                switch (g.CompetitionId)
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
                    Text = g.getFixture(),
                    Value = g.Id.ToString(),
                    Group = grp
                };
                listSelectListItems.Add(selectList);
            }

            List<string> selected = new List<string>();
            foreach (Game g in gamesInDbUnsold)
            {
                string s = g.getFixture();
                selected.Add(s);
            }

            var viewModel = new TicketFormViewModel
            {
                Id = ticket.Id,
                Owner = ticket.Owner,
                CategoryId = ticket.CategoryId,
                ViewUpload = ticket.ViewUpload,
                Section = ticket.Section,
                Row = ticket.Row,
                Seat = ticket.Seat,
                Categories = _context.Ref_Category.ToList(),
                Games = listSelectListItems,
                SelectedGames = selected,
                Groups = groups
            };
            return View("Form", viewModel);
        }

        [HttpGet]
        public ActionResult Remove(int id)
        {
            var ticket = _context.Tickets.Single(c => c.Id == id);
            var soldGames = ticket.GetAllSoldGames();
            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (soldGames.Count > 0)
            {
                return RedirectToAction("Edit", "Ticket", new { id = ticket.Id, valid = 0 });
            }
            else
            {
                var tgs = _context.Ticket_Game.Where(m => m.TicketId == ticket.Id).ToList();
                foreach (Ticket_Game tg in tgs)
                {
                    _context.Ticket_Game.Remove(tg);
                }
                _context.Tickets.Remove(ticket);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Ticket");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var ticket = _context.Tickets.Single(c => c.Id == id);

            if (ticket == null)
            {
                return HttpNotFound();
            }
            else
            {
                var competitions = _context.Ref_Competition.ToList();
                List<string> competitionNameList = new List<string>();
                foreach (Ref_Competition cp in competitions)
                {
                    if (ticket.GetAllAssignedGamesByCompetition(cp.Id).Count > 0)
                    {
                        competitionNameList.Add(cp.Name);
                    }
                }

                var vm = new TicketDetailsViewModel
                {
                    Id = ticket.Id,
                    Owner = ticket.Owner,
                    Category = ticket.Ref_Category.Name,
                    Position = ticket.getPosition(),
                    Games = ticket.GetAllAssignedGames(),
                    Competitions = competitionNameList
                };
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrices(int id)
        {
            var ticket = _context.Tickets.Single(c => c.Id == id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var unsoldGames = ticket.GetAllUnsoldGames();
            foreach (Ticket_Game tg in unsoldGames)
            {
                var price = Request[tg.GameId.ToString()];
                if (price == "")
                {
                    price = "0";
                }
                double? priceDouble = Convert.ToDouble(price);
                Ticket_Game toUpdate = _context.Ticket_Game.Single(m => m.GameId == tg.GameId && m.TicketId == ticket.Id);
                toUpdate.Price = priceDouble;
                _context.SaveChanges();

                //string query = "UPDATE dbo.Ticket_Game SET Price = " + price + " WHERE TicketId = " + ticket.Id + " and GameId = " + tg.GameId + "; ";
                //_context.Database.ExecuteSqlCommand(query);
            }
            return RedirectToAction("Index", "Ticket");
        }

    }

    
}