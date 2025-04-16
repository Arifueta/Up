using System;

namespace UPApp.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Guid { get; set; }
        public string Email { get; set; }
        public double? SocialSecNumber { get; set; }
        public string Ein { get; set; }
        public string SocialType { get; set; }
        public string Phone { get; set; }
        public double? PassportS { get; set; }
        public double? PassportN { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Country { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceAddress { get; set; }
        public double? InsuranceInn { get; set; }
        public string IpAddress { get; set; }
        public double? InsuranceP { get; set; }
        public double? InsuranceBik { get; set; }
        public string UserAgent { get; set; }
    }
} 