using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Broker
{
    public class Subscriber
    {
        private readonly Socket _socket;

				public Subscriber( Socket socket ) => _socket = socket;

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

        public bool IsSocketStillConnected() => (!_socket.Poll(1000, SelectMode.SelectRead) || _socket.Available != 0) && _socket.Connected;

				private void SendCallback( IAsyncResult AR ) => _ = _socket.EndSend(AR);

				// Generates encoded frames for WebSocket protocol. Found on StackOverflow: https://stackoverflow.com/questions/10200910/creating-a-hello-world-websocket-example

				private static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
        {
            var bytesRaw = Encoding.Default.GetBytes(Message);
            var frame = new byte[10];

            int indexStartRawData;
            var length = bytesRaw.Length;

            frame[0] = (byte)(128 + (int)Opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = 126; // Bytes can hold numbers up to 127, so anything <= 127 doesn't need to be casted
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = 127;
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

            var response = new byte[indexStartRawData + length];

						frame.Take(indexStartRawData).ToArray().CopyTo(response, 0);
						bytesRaw.CopyTo(response, indexStartRawData);

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
