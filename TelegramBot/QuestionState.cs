using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class QuestionState
    {
        public QuestionItem CurrentItem { get; set; }
        public int Opened { get; set; }
        public string AnswerHint => CurrentItem.Answer.Substring(0, Opened).PadRight(CurrentItem.Answer.Length, '*');
        public string DisplayQuestion => $"{CurrentItem.Question} :  {CurrentItem.Answer.Length} букв \n" +
            $" {AnswerHint}";
        public bool IsEnd => Opened == CurrentItem.Answer.Length;
    }
}
