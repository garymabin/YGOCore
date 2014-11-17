using System;

namespace YGOCore
{
    public interface IGameWatcher
    {
        /**
         * Start watch
         */
        void Start();

        /**
         * Stop watch
         */
        void Stop();

        /**
         * when converted unicode string event triggered.
         */
        void onEvent(String unicodeStrEvent);
    }
}

