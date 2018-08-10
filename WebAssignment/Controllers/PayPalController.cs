using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.Models;

namespace WebAssignment.Controllers
{
    public class PayPalController : Controller
    {
        private WebAssignmentDbContext _context;

        public PayPalController()
        {
            _context = new WebAssignmentDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult RedirectFromPaypal(string ids)
        {
            Temp_PayPal p = _context.Temp_PayPal.Single(m => m.Id == ids);
            Pay(p.Ticket_Game_Ids);
            _context.Temp_PayPal.Remove(p);
            return RedirectToAction("Index", "Order");
        }

        public void Pay(string tgs)
        {
            string[] pairs = tgs.Split(',');
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (string pair in pairs) {
                string[] broken = pair.Split('-');
                int tgId = Convert.ToInt32(broken[0]);
                string tgName = broken[1];
                dict.Add(tgId, tgName);
            }

            Order order = new Order();
            double? totalPrice = 0.0;
            foreach (int i in dict.Keys)
            {
                try
                {
                    int n = i;
                    Ticket_Game tg = _context.Ticket_Game.Single(m => m.Id == n);
                    totalPrice += tg.Price;
                    tg.Name = dict[i];
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
    }
    

        public ActionResult CancelFromPaypal()
        {
            return View();
        }

        public ActionResult NotifyFromPaypal()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ValidateCommand()
        {
            string tgs = "";
            double? totalPrice = 0;
            string product;
            string game = "";
            int ticketCount = 0;
            // build tgId-tgName pairs
            foreach (string name in Request.Form.AllKeys)
            {
                try
                {
                    int n = Convert.ToInt32(name);
                    Ticket_Game tg = _context.Ticket_Game.Single(m => m.Id == n);
                    if (tg.Sold == 1) {
                        //already sold
                        return View("Error");
                    }
                    totalPrice += tg.Price;
                    ticketCount++;
                    game = tg.Game.getFixture();
                    tgs += name + "-" + Request.Form[name] + ",";
                }
                catch (Exception e)
                {

                }


            }
            tgs = tgs.TrimEnd(',');
            product = ticketCount + " ticket(s) " + game;
            PayPalModel paypal = new PayPalModel();
            paypal.cmd = "_xclick";
            paypal.Tgs = tgs;
            paypal.business = "andrea.naudi-facilitator@gmail.com";
            paypal.currency_code = "EUR";
            bool useSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSandbox"]);
            if (useSandbox)
            {
                ViewBag.actionUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr";
            }
            else
            {
                ViewBag.actionUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr";
            }
            paypal.cancel_return = ConfigurationManager.AppSettings["CancelUrl"];
            paypal.@return = "https://localhost:44315/PayPal/RedirectFromPayPal?ids="+paypal.id;
            paypal.notify_url = "https://localhost:44315/PayPal/NotifyFromPaypal";
            

            
            paypal.item_name = product;
            paypal.amount = totalPrice.ToString();
            Temp_PayPal tp = new Temp_PayPal
            {
                Id = paypal.id,
                Ticket_Game_Ids = paypal.Tgs
            };
            _context.Temp_PayPal.Add(tp);
            _context.SaveChanges();
            return View(paypal);
        }
    }
}