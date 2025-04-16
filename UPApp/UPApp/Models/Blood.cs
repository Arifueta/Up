using System;

namespace UPApp.Models
{
    public class Blood
    {
        public int Id { get; set; }
        public int? PatientId { get; set; }
        public double? Barcode { get; set; }
        public DateTime? Date { get; set; }
    }
} 