using System;

namespace UPApp.Models
{
    public class BloodService
    {
        public int BloodId { get; set; }
        public int ServiceId { get; set; }
        public double? Result { get; set; }
        public DateTime? Finished { get; set; }
        public bool Accepted { get; set; }
        public string Status { get; set; }
        public string Analyzer { get; set; }
        public int UserId { get; set; }
    }
} 