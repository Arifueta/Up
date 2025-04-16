using System;

namespace UPApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public DateTime LastEnter { get; set; }
        public int Type { get; set; }
        public string RoleName { get; set; }
    }
} 