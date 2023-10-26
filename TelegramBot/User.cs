using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class User
    {
        public Dictionary<long, QuestionState> States = new Dictionary<long, QuestionState>();
        public Dictionary<long, int> UserScores = new Dictionary<long, int>();
    }
}
