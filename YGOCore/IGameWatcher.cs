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
         * when converted event string event triggered.
         */
        void onEvent(String eventstring, Object formatParams);
    }
}

