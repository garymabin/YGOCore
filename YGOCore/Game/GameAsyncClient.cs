using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;

namespace YGOCore.Game
{
    public class GameAsyncClient: IGameClient
    {
        public bool IsConnected { get; private set; }

        public Game Game { get;private set; }

        public Player Player{ get;private set; }

        private GameRoom m_room;
        private Socket m_socket;
        private Queue<GameClientPacket> m_recvQueue;
        private Queue<byte[]> m_sendQueue;
        private bool m_closePending;
        private bool m_disconnected;
        private int m_receivedLen;

        private Byte[] m_read_buffer;

        private const  int BufferSize = 256;

        public GameAsyncClient(Socket socket)
        {
            IsConnected = true;
            m_socket = socket;
            m_socket.Blocking = false;
            Player = new Player(this);
            m_recvQueue = new Queue<GameClientPacket>();
            m_sendQueue = new Queue<byte[]>();

            m_read_buffer = new byte[BufferSize]{ 0 };
        }

        #region IGameClient implementation

        public void Close()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            m_socket.Close();
            if(InGame())
                m_room.RemoveClient(this);
        }

        public bool InGame()
        {
            return Game != null;
        }

        public void JoinGame(GameRoom room)
        {
            if (m_room == null)
            {
                m_room = room;
                Game = m_room.Game;
            }
        }

        public void CloseDelayed()
        {
            m_closePending = true;
        }

        public void Send(byte[] raw)
        {
            m_sendQueue.Enqueue(raw);
        }

        private void NetworkReceive()
        {
            byte[] buffer = new byte[256]{0};
            if (m_socket.Available >= 2 && m_receivedLen == -1)
                m_receivedLen = m_socket.Receive(m_read_buffer, 2);

            if (m_receivedLen != -1 && m_socket.Available >= m_receivedLen)
            {
                GameClientPacket packet = new GameClientPacket(m_reader.ReadBytes(m_receivedLen));
                m_receivedLen = -1;
                lock (m_recvQueue)
                    m_recvQueue.Enqueue(packet);
            }
        }

        private void NetworkSend()
        {
            while (m_sendQueue.Count > 0)
            {
                byte[] raw = m_sendQueue.Dequeue();
                MemoryStream stream = new MemoryStream(raw.Length + 2);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((ushort)raw.Length);
                writer.Write(raw);
                m_socket.Send(stream.ToArray());
            }
        }

        public void Tick()
        {
            if (IsConnected)
            {
                try
                {
                    CheckDisconnected();
                    NetworkSend();
                    NetworkReceive();
                }
                catch (Exception)
                {
                    m_disconnected = true;
                }
            }
            if (m_closePending)
            {
                m_disconnected = true;
                Close();
                return;
            }
            if (!m_disconnected)
            {
                try
                {
                    NetworkParse();
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                    m_disconnected = true;
                }
            }
            if (m_disconnected)
            {
                Close();
                Player.OnDisconnected();
            }
        }

        #endregion

        private void CheckDisconnected()
        {
            m_disconnected = (m_socket.Poll(1, SelectMode.SelectRead) && m_socket.Available == 0);
        }

        private void NetworkParse()
        {
            int count;
            lock (m_recvQueue)
                count = m_recvQueue.Count;
            while (count > 0)
            {
                GameClientPacket packet = null;
                lock (m_recvQueue)
                {
                    if (m_recvQueue.Count > 0)
                        packet = m_recvQueue.Dequeue();
                    count = m_recvQueue.Count;
                }
                if (packet != null)
                    Player.Parse(packet);
            }
        }

    }
}

