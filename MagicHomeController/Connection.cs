using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MagicHomeController
{
    class Connection : IDisposable
    {
        private const int WIFI_PORT = 5577;
        Socket m_socket;

        private IPAddress m_ipAddress;
        private string ipAddress;

        private int m_timeout = 5000;
        private int m_queryLen;
        private static readonly byte[] NEW_QUERY_MSG = { 0x81, 0x8a, 0x8b, 0x96 };


        public Connection(Bulb bulb)
        {
            m_queryLen = 14;
            m_ipAddress = IPAddress.Parse(bulb.IpAddress);
            ipAddress = bulb.IpAddress;
            Console.WriteLine($"Creating new socket for {bulb.IpAddress}...");
            m_socket = new Socket(m_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (!m_socket.Connected)
                Connect();
        }

        public byte[] QueryState(int retries)
        {

            try
            {
                if (!m_socket.Connected)
                    Connect();
                SendMessage(NEW_QUERY_MSG, false);
                return ReadMsg(m_queryLen);
            }
            catch (SocketException)
            {
                if (retries < 1)
                {
                    Console.WriteLine("No reply");
                    return null;
                }
                return QueryState(retries - 1);
            }
        }
        public void SendMessage(byte[] bytes, bool sendChecksum)
        {
            if (!m_socket.Connected)
                Connect();
            if (sendChecksum)
            {
                var checksum = CalculateChecksum(bytes);
                Array.Resize(ref bytes, bytes.Length + 1);
                bytes[bytes.Length - 1] = checksum;
            }

            m_socket.Send(bytes);


        }

        private byte[] ReadMsg(int expected)
        {
            byte[] buffer = new byte[expected];
            m_socket.ReceiveTimeout = m_timeout;
            int bytes_read = m_socket.Receive(buffer);
            Array.Resize(ref buffer, bytes_read);
            return buffer;
        }
        public byte CalculateChecksum(byte[] bytes)
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


        public void Connect()
        {
            Console.WriteLine($"Attempting to connect socket to {ipAddress}...");
            if (m_socket.Connected)
            {
                Console.WriteLine($"Closing socket and reopening to {ipAddress}...");
                m_socket.Close();
                m_socket = new Socket(m_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            m_socket.Connect(m_ipAddress, WIFI_PORT);
            Console.WriteLine($"Connection to {ipAddress} successful...");
        }

        public void Dispose()
        {
            if (m_socket.Connected)
            {
                m_socket.Close();
            }
            m_socket.Dispose();
        }


    }
}
