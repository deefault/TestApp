using System.Collections.Generic;
using System.IO;
using WebApplication3.Models;

namespace WebApplication3.TParser
{
    public class TestData
    {
        public Test Test { get; set; } = new Test();
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<Option> Options { get; set; } = new List<Option>();
    }
}
