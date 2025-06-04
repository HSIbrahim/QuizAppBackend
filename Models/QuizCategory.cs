using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppBackend.Models
{
    public class QuizCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fixed CS8618
        public string Description { get; set; } = string.Empty; // Fixed CS8618

        // Navigationsproperty
        public ICollection<Question> Questions { get; set; } = new List<Question>(); // Fixed CS8618 (Initialize collection)
    }
}