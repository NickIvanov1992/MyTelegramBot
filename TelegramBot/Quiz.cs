using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class Quiz
    {
        List<QuestionItem> Questions { get; set; }

        private Random random;
        private int count;
        public Quiz(string path)
        {
            var lines = File.ReadAllLines(path);
            Questions = lines.Select(s => s.Split("|")).Select(s => new QuestionItem
            {
                Question = s[0],
                Answer = s[1]
            }).ToList();
            random = new Random();
            count = Questions.Count;
        }

        public QuestionItem NextQuestion()
        {
            var index = random.Next(count - 1);
            var question = Questions[index];
            return question;
        }

    }
}
