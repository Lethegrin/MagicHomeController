using System;
using System.Net;
using System.Net.Sockets;

namespace MagicHomeConsoleApp
{

    // General Stuff based on: 
    // https://github.com/beville/flux_led
    // https://github.com/vikstrous/zengge-lightcontrol
    // https://github.com/zoot1612/plugin_mh/blob/master/MH_API.txt
    // some own reverse engineering via tcpdump

    // Legacy Bulb Stuff based on: 
    // https://docs.google.com/document/d/1IJ2l5GvphzQFpe22I6L5qjZG0DXvY12nHl6hHSQ7I2s/pub

    public class Transmit
    {



        protected byte CalculateChecksum(byte[] bytes)
        {
            byte checksum = 0;

            foreach (var b in bytes)
            {
                unchecked
                {
                    checksum += b;
                }
            }

            return checksum;
        }


        public virtual byte[] SendMessage(string ipAddress, byte[] bytes, bool sendChecksum, bool waitForResponse)
        {
            Socket _socket;
            int DefaultPort = 5577;

            IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), DefaultPort);
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.ReceiveTimeout = 1;
            _socket.SendTimeout = 1;
            _socket.Connect(_endPoint);

            if (sendChecksum)
            {
                var checksum = CalculateChecksum(bytes);
                Array.Resize(ref bytes, bytes.Length + 1);
                bytes[bytes.Length - 1] = checksum;
            }

            var buffer = new byte[256];

            if (waitForResponse)
            {
                _socket.Blocking = false;

                try
                {
                    while (_socket.Receive(buffer) > 0)
                    {
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.WouldBlock)
                    {
                        throw;
                    }
                }

                _socket.Blocking = true;
            }

            const int maxSendRetries = 1;
            var retries = 0;

            while (true)
            {
                _socket.Send(bytes);

                if (!waitForResponse)
                {
                    _socket.Close();
                    return null;
                }

                try
                {
                    int readBytes = _socket.Receive(buffer);

                    Array.Resize(ref buffer, readBytes);

                    return buffer;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut || retries >= maxSendRetries)
                    {
                        throw;
                    }

                    retries++;
                    System.Threading.Thread.Sleep(10);
                }
            }

        }



        /*
     
       public void SetPreset(PresetMode presetMode, byte delay)
       {
           Math.Clamp(delay, (byte)0x01, (byte)0x24);


            SendMessage(IpAddress, new byte[] { 0x61, (byte)presetMode, delay, 0x0F }, true, true);
           }
       }

       public DateTime GetTime()
       {
           var msg = new byte[] { 0x11, 0x1a, 0x1b, 0x0f };

           var response = SendMessage(msg, true, true);

           if (response.Length != 12)
               throw new Exception("Controller sent wrong number of bytes while getting time");

           return new DateTime(response[3] + 2000, response[4], response[5], response[6], response[7], response[8]);
       }

       public void SetTime(DateTime time)
       {
           byte[] msg;

           checked
           {
               msg = new byte[]
               {
                   0x10,
                   0x14,
                   (byte)(time.Year - 2000),
                   (byte)time.Month,
                   (byte)time.Day,
                   (byte)time.Hour,
                   (byte)time.Minute,
                   (byte)time.Second,
                   (byte)(time.DayOfWeek == 0 ? 7 : (int)time.DayOfWeek),
                   0x00,
                   0x0F
               };
           }

           SendMessage(msg, true, false);
       }

       public IEnumerable<Timer> GetTimers()
       {
           var msg = new byte[] { 0x22, 0x2a, 0x2b, 0x0f };

           var response = SendMessage(msg, true, true);

           return TimerSerializer.Deserialize(response, _deviceType);
       }

       public void SetTimers(IEnumerable<Timer> timers)
       {
           var msg = TimerSerializer.Serialize(timers, _deviceType);

           SendMessage(msg, true, false);
       }
     */
    }


}
