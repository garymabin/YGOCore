using System;

namespace YGOCore.Game
{
    public interface IGameClient
    {
        bool IsConnected { get; private set; }
        Game Game { get; private set; }
        Player Player { get; private set; }

        void Close();

        bool InGame();

        void JoinGame(GameRoom room);

        void CloseDelayed();

        void Send(byte[] raw);

        void Tick();


    }
}

