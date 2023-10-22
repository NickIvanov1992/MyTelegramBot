using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Interfaces
{
    public interface IGetGameState
    {
        void GetGameState(string textMessage);
        void StartNewGame();
        void FinishGame();
    }
}
