using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAssignment.Models;
using Microsoft.AspNet.Identity;
using WebAssignment.ViewModels;

namespace WebAssignment.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private WebAssignmentDbContext _context;

        public ReviewController()
        {
            _context = new WebAssignmentDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: Review
        [HttpGet]
        public ActionResult Index()
        {
            var reviews = _context.Reviews.ToList();

            return View(reviews);
        }

        [Authorize(Roles = RoleName.User)]
        [HttpGet]
        public ActionResult New()
        {
            var viewModel = new ReviewFormViewModel()
            {
                Id = 0,
            };
            return View("Form", viewModel);
        }

        [HttpGet]
        public ActionResult Details(int id) {
            var review = _context.Reviews.SingleOrDefault(c => c.Id == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ReviewDetailsViewModel
            {
                WrittenBy = review.GetUser().UserName,
                Comment = review.Comment,
                Rating = review.Rating,
                DateWritten = review.DateWritten,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ReviewFormViewModel vm)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            if (!ModelState.IsValid)
            {
                //foreach (ModelState modelState in ViewData.ModelState.Values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {
                //        return Content(error.ErrorMessage);
                //    }
                //}
                return View("Form", vm);
            }

            if (vm.Id == 0)
            {
                var currentUserId = User.Identity.GetUserId();
                var review = new Review
                {
                    Id = vm.Id,
                    Comment = vm.Comment,
                    Rating = vm.Rating,
                    UserId = currentUserId,
                    DateWritten = DateTime.Now,
                };
                _context.Reviews.Add(review);
            }
            else
            {
                var reviewInDb = _context.Reviews.Single(c => c.Id == vm.Id);
                reviewInDb.Comment = vm.Comment;
                reviewInDb.Rating = vm.Rating;
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Review");
        }
        
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var review = _context.Reviews.SingleOrDefault(c => c.Id == id);

            if (review == null || (review.UserId != User.Identity.GetUserId() && User.IsInRole(RoleName.User)))
            {
                return HttpNotFound();
            }
            else {
                var viewModel = new ReviewFormViewModel
                {
                    Id = review.Id,
                    Comment = review.Comment,
                    Rating = review.Rating,
                };
                return View("Form", viewModel);
            }
            
        }

        [HttpGet]
        public ActionResult Remove(int id)
        {
            var review = _context.Reviews.Single(c => c.Id == id);

            if (review == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (review.UserId != User.Identity.GetUserId() && User.IsInRole(RoleName.Administrator) == false)
                {
                    return HttpNotFound();
                }
                else {
                    _context.Reviews.Remove(review);
                }
                
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Review");
        }
    }
}