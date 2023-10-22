using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot;
using static System.Formats.Asn1.AsnWriter;
using File = System.IO.File;

using CancellationTokenSource cts = new();


var quiz = new Quiz("data.txt");
var token = "6625605730:AAG2mpJwAwXLCFeC_gyY8EUNnrKMesLbSRM";
var bot = new TelegramBotClient(token);
var States = new Dictionary<long, QuestionState>();
var UserScores = new Dictionary<long, int>();

GameData gameData = new GameData();
gameData.LoadGame(ref States, ref UserScores);

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
    var fromId = message.From.Id;
    if (!States.TryGetValue(chatId, out var state))
    {
        state = new QuestionState();
        States[chatId] = state;
    }
    if (state.CurrentItem == null)
    {
        state.CurrentItem = quiz.NextQuestion();
    }

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    var question = state.CurrentItem;
    var tryAnswer = messageText.ToLower().Replace('ё', 'е');
    if (tryAnswer == question.Answer)
    {
        //score++;
        await botClient.SendTextMessageAsync(chatId, "Правильно!",
            cancellationToken: cancellationToken);
        state.Opened = 0;
        Console.WriteLine("Верно!");
        if (UserScores.ContainsKey(fromId))
            UserScores[fromId]++;

        else
            UserScores[fromId] = 1;

        state.CurrentItem = quiz.NextQuestion();

        await botClient.SendTextMessageAsync(chatId,
        $"У вас {UserScores[fromId]} очков",
        cancellationToken: cancellationToken);
    }
    else
    {
        await botClient.SendTextMessageAsync(chatId,
        "Не правильно!",
        cancellationToken: cancellationToken);

        state.Opened++;
        if (state.IsEnd)
        {
            await botClient.SendTextMessageAsync(chatId,
            $"Правильный ответ: {question.Answer}",
            cancellationToken: cancellationToken);
            state.Opened = 0;
            state.CurrentItem = quiz.NextQuestion();
            Console.WriteLine($"Правильный ответ: {question.Answer}");
        }

        Console.WriteLine("Не верно!");
    }

    await botClient.SendTextMessageAsync(chatId,
        state.DisplayQuestion,
        cancellationToken: cancellationToken);

    gameData.SaveGame(States, UserScores);
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

