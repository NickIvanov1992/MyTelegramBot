using System.Security.Cryptography.X509Certificates;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Formats.Asn1.AsnWriter;
using File = System.IO.File;

using CancellationTokenSource cts = new();


var quiz = new Quiz("data.txt");
var token = "6625605730:AAG2mpJwAwXLCFeC_gyY8EUNnrKMesLbSRM";
var bot = new TelegramBotClient(token);
var States = new Dictionary<long,QuestionState>();
var UserScores = new Dictionary<long, int>();
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

    bot.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);
Console.ReadKey();
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    if (!States.TryGetValue(chatId, out var state))
    {
        state = new QuestionState();
        States[chatId] = state;
    }
    if(state.CurrentItem == null)
    {
        state.CurrentItem = quiz.NextQuestion();
    }
    

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

   


    var question = state.CurrentItem;
    var tryAnswer = messageText.ToLower().Replace('ё', 'е');
    if (tryAnswer == question.Answer)
    {
        //score++;
        await botClient.SendTextMessageAsync(chatId: chatId,
        text: "Правильно!",
        cancellationToken: cancellationToken);

        Console.WriteLine("Верно!");
        if(UserScores.ContainsKey(message.From.Id))
        {
            UserScores[message.From.Id]++;
        }
        else
        {
            UserScores[message.From.Id] = 1;
        }
        state.CurrentItem = quiz.NextQuestion();
        //Console.WriteLine($" У вас{score} очков");
        await botClient.SendTextMessageAsync(chatId: chatId,
        text: $"У вас {UserScores} очков",
        cancellationToken: cancellationToken);


    }
    else
    {
        state.Opened++;
        if (state.IsEnd)
        {
        await botClient.SendTextMessageAsync(chatId: chatId,
        text: $"Правильный ответ: {question.Answer}",
        cancellationToken: cancellationToken);

            Console.WriteLine($"Правильный ответ: {question.Answer}");
        }
        await botClient.SendTextMessageAsync(chatId: chatId,
        text: "Не правильно!",
        cancellationToken: cancellationToken);

        Console.WriteLine("Не верно!");
        //opened++;
    }
    await botClient.SendTextMessageAsync(chatId: chatId,
        text: state.DisplayQuestion,
        cancellationToken: cancellationToken);
    // Echo received message text
    void NewRound(long chatId)
    {

    }

}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
public class Quiz
{
    List<QuestionItem> Questions { get; set; }

    private Random random;
    private int count;
    public Quiz(string path = "data.txt")
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
public class QuestionItem
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

public class QuestionState
{
    public QuestionItem CurrentItem { get; set; }
    public int Opened { get; set; } 
    public string AnswerHint => CurrentItem.Answer.Substring(0, Opened).PadRight(CurrentItem.Answer.Length, '*');
    public string DisplayQuestion => $"{CurrentItem.Question} :  {CurrentItem.Answer.Length} букв \n" +
        $" {AnswerHint}";
    public bool IsEnd => Opened == CurrentItem.Answer.Length;
}
