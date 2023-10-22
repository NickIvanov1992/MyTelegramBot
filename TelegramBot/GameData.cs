using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TelegramBot
{
    public class GameData
    {
        private readonly string  StateFileName = "state.json";
        private readonly string  ScoreFileName = "score.json";

        public void LoadGame(ref Dictionary<long, QuestionState>?  States, ref Dictionary<long, int>? UserScores)
        {
            if (File.Exists(StateFileName))
            {
                var json = File.ReadAllText(StateFileName);
                States = JsonConvert.DeserializeObject<Dictionary<long, QuestionState>>(json);
            }
            if (File.Exists(ScoreFileName))
            {
                var json = File.ReadAllText(ScoreFileName);
                UserScores = JsonConvert.DeserializeObject<Dictionary<long, int>>(json);
            }
        }
        public void SaveGame( Dictionary<long, QuestionState> States, Dictionary<long, int> UserScores)
        {
            var stateJson = JsonConvert.SerializeObject(States);
            File.WriteAllText(StateFileName, stateJson);

            var scoreJson = JsonConvert.SerializeObject(UserScores);
            File.WriteAllText(ScoreFileName, scoreJson);
        }
    }
}
