using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppBackend.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty; // Fixed CS8618
        public string ReceiverId { get; set; } = string.Empty; // Fixed CS8618
        public bool Accepted { get; set; } = false; // Sant om vänförfrågan är accepterad

        // Navigationsproperties
        public User? Sender { get; set; } // Fixed CS8618 (Navigation property can be null)
        public User? Receiver { get; set; } // Fixed CS8618 (Navigation property can be null)
    }
}