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
TelegramBot.User user = new();
GameData gameData = new(user, quiz);
gameData.LoadGame();

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

    gameData.CheckUserState(chatId, fromId);

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    var question = user.States[chatId].CurrentItem;
    var tryAnswer = messageText.ToLower().Replace('ё', 'е');

    string value = gameData.CheckState(question, tryAnswer, chatId, fromId);

    await botClient.SendTextMessageAsync(chatId,
        value, cancellationToken: cancellationToken);

    if (user.UserScores[fromId] > 0)
    {
        await botClient.SendTextMessageAsync(chatId,
        user.States[chatId].DisplayQuestion,
        cancellationToken: cancellationToken);
    }
    else
    {
        await botClient.SendTextMessageAsync(chatId,
        "Для начала игры отправьте: 'start' ",
        cancellationToken: cancellationToken);
    }
    gameData.SaveState(user.States);
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