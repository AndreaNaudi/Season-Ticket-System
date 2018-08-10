using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAssignment.Models;

namespace WebAssignment.ViewModels
{
    public class OrderIndexViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Game> Games { get; set; }
    }
}