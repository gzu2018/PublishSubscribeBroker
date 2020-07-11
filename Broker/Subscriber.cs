using System;
using System.Net.Sockets;
using System.Text;

namespace Broker
{
    public class Subscriber
    {
        private readonly Socket _socket;

        public Subscriber(Socket socket)
        {
            _socket = socket;
        }

        public void SendMessage(string messageContent)
        {
            try
            {
                var frameContent = GetFrameFromString(messageContent);
                _socket.BeginSend(frameContent, 0, frameContent.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (SocketException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
            }
        }

        public bool IsSocketStillConnected()
        {
            var part1 = _socket.Poll(1000, SelectMode.SelectRead);
            var part2 = (_socket.Available == 0);
            return !((part1 && part2) || !_socket.Connected);
        }

        private void SendCallback(IAsyncResult AR)
        {
            _socket.EndSend(AR);
        }

        // Generates encoded frames for WebSocket protocol. Found on StackOverflow: https://stackoverflow.com/questions/10200910/creating-a-hello-world-websocket-example

        private static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(Message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)(128 + (int)Opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        private enum EOpcodeType
        {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }
    }
}