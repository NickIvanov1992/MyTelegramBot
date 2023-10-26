namespace TelegramBot
{
    public class QuestionState
    {
        public QuestionItem? CurrentItem { get; set; }
        public int Opened { get; set; }
        public string AnswerHelp => CurrentItem.Answer.Substring(0, Opened).PadRight(CurrentItem.Answer.Length, '*');
        public string DisplayQuestion => $"{CurrentItem.Question} :  {CurrentItem.Answer.Length} букв \n" +
            $" {AnswerHelp}";
        public bool IsEnd => Opened == CurrentItem.Answer.Length;
    }
}
