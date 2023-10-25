﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace TelegramBot
{
    public class GameData
    {
        private readonly string StateFileName = "state.json";
        private readonly string ScoreFileName = "score.json";
        User user = new();
        Quiz quiz = new("data.txt");
        public GameData(User user, Quiz quiz)
        {
            this.user = user;
            this.quiz = quiz;
        }
        public void LoadGame()
        {
            if (File.Exists(StateFileName))
            {
                var json = File.ReadAllText(StateFileName);
                user.States = JsonConvert.DeserializeObject<Dictionary<long, QuestionState>>(json);
            }
            if (File.Exists(ScoreFileName))
            {
                var json = File.ReadAllText(ScoreFileName);
                user.UserScores = JsonConvert.DeserializeObject<Dictionary<long, int>>(json);
            }
        }

        public void SaveGame(Dictionary<long, QuestionState> States, Dictionary<long, int> UserScores)
        {
            var stateJson = JsonConvert.SerializeObject(States);
            File.WriteAllText(StateFileName, stateJson);

            var scoreJson = JsonConvert.SerializeObject(UserScores);
            File.WriteAllText(ScoreFileName, scoreJson);
        }
        public void SaveState(Dictionary<long, QuestionState> States)
        {
            var stateJson = JsonConvert.SerializeObject(States);
            File.WriteAllText(StateFileName, stateJson);
        }
        public void SaveScores(Dictionary<long, int> UserScores, long fromId)
        {
            var scoreJson = JsonConvert.SerializeObject(UserScores);
            File.WriteAllText(ScoreFileName, scoreJson);

        }
        public void CheckUserState(long chatId, long fromId)
        {
            if (!user.States.TryGetValue(chatId, out var state))
            {
                state = new QuestionState();
                user.States[chatId] = state;
                user.UserScores[fromId] = 5;
            }
            if (state.CurrentItem == null)
            {
                state.CurrentItem = quiz.NextQuestion();
            }
        }
        public string CheckState(QuestionItem item, string answer, long chatId, long fromId)
        {
            if (answer == item.Answer && user.UserScores[fromId] > 0)
            {
                user.States[chatId].Opened = 0;
                Console.WriteLine("Верно!");

                user.UserScores[fromId]++;

                user.States[chatId].CurrentItem = quiz.NextQuestion();


                return $"Правильно! У вас {user.UserScores[fromId]} очков";
            }
            else if (answer != item.Answer && user.UserScores[fromId] > 1)
            {
                user.States[chatId].Opened++;
                user.UserScores[fromId]--;
                if (user.States[chatId].IsEnd)
                {
                    user.States[chatId].Opened = 0;
                    user.States[chatId].CurrentItem = quiz.NextQuestion();
                    Console.WriteLine($"Не верно!  Правильный ответ: {item.Answer}");
                    return $"Не верно! Правильный ответ: {item.Answer} ";
                }
                Console.WriteLine("Не верно!");
                return $"Не верно! \n" +
                    $"Осталось {user.UserScores[fromId]} очков";
            }
            else if (answer == "start")
            {
                user.UserScores[fromId] = 5;
                user.States[chatId].CurrentItem = quiz.NextQuestion();

                return $"Игра началась! \n" +
                    $" У вас {user.UserScores[fromId]} очков";
            }
            else /*if (user.UserScores[fromId] == 0)*/
            {
                user.States[chatId].Opened = 0;
                user.UserScores[fromId] = 0;

                Console.WriteLine($"Игра окончена. У вас 0 очков. \n " +
                                    $"Правильный ответ: {item.Answer}");

                return $"Игра окончена. У вас 0 очков. \n " +
                            $"Правильный ответ: {item.Answer} \n" +
                            $"РЕКОРДЫ:\n" +
                            $" {GetGameRecords(user)}";
            }
        }
        public string GetGameRecords(User users)
        {

            List<string> scores = new List<string>();
            foreach (var user in users.UserScores)
            {
                scores.Add($"Имя: {user.Key} Очков: {user.Value}");
            }
            var result = String.Join(",", scores.ToArray());
            return result;
        }
    }

}
