using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAssignment.ViewModels
{
    public class ReviewDetailsViewModel
    {
        public string Comment { get; set; }
        public string WrittenBy { get; set; }
        public DateTime DateWritten { get; set; }
        public int Rating { get; set; }

    }
}