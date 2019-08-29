using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YeeLightAPI
{
    public class YeeLight
    {
        private TcpClient yeelightTcpClient = new TcpClient();
        private bool musicMode = false;
        private Exception lastError = null;

        public YeeLight()
        {

        }

        public YeeLight(IPAddress ipAddress, int port)
        {
            Connect(ipAddress, port);
        }

        public bool Connect(IPAddress ipAddress, int port)
        {
            try
            {
                yeelightTcpClient.Connect(ipAddress, port);
                musicMode = false;
                return true;
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        public bool isConnected()
        {
            return yeelightTcpClient.Connected;
        }

        public bool isMusicMode()
        {
            return musicMode;
        }

        public Exception GetLastError()
        {
            return lastError;
        }

        public bool CloseConnection()
        {
            try
            {
                yeelightTcpClient.GetStream().Close();
                yeelightTcpClient.Close();

                yeelightTcpClient = new TcpClient();
                return true;
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        private bool SendString(string command)
        {
            try
            {
                if (yeelightTcpClient.Connected)
                {
                    return yeelightTcpClient.Client.Send(Encoding.ASCII.GetBytes(command)) > 0;
                }
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        public bool SetBrightness(int brightness) //Range is 0 - 100
        {
            try
            {
                if (yeelightTcpClient.Connected && brightness <= 100 && brightness >= 0)
                {
                    return SendString("{\"id\":" + 1 + ",\"method\":\"set_bright\",\"params\":[ " + brightness.ToString() + ", \"sudden\", 100" + "]}\r\n");
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        public bool SetPower(bool status)
        {
            try
            {
                if (yeelightTcpClient.Connected)
                {
                    return SendString("{\"id\":" + 1 + ",\"method\":\"set_power\",\"params\":[ " + (status ? "on" : "off") + ", \"sudden\", 100" + "]}\r\n");
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        public bool SetColor(int red, int green, int blue)
        {
            try
            {
                if (yeelightTcpClient.Connected)
                {
                    int value = ((red) << 16) | ((green) << 8) | (blue);
                    return SendString("{\"id\":" + 1 + ",\"method\":\"set_rgb\",\"params\":[ " + value.ToString() + ", \"sudden\", 100" + "]}\r\n");
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }

        public bool SetMusicMode(IPAddress localIP, int localPort)
        {
            try
            {
                if (!musicMode)
                {
                    TcpListener musicModeTcpListener;
                    musicModeTcpListener = new TcpListener(localIP, localPort);
                    musicModeTcpListener.Start();
                    SendString("{\"id\":" + 1 + ",\"method\":\"set_music\",\"params\":[1, \"" + localIP + "\"," + localPort + "]}\r\n");
                    yeelightTcpClient.GetStream().Close();
                    yeelightTcpClient.Close();

                    int i = 0;
                    while (!musicModeTcpListener.Pending())
                    {
                        Thread.Sleep(100);
                        if (i >= 10)
                        {
                            musicModeTcpListener.Stop();
                            throw new TimeoutException();
                        }
                        i++;
                    }
                    yeelightTcpClient = musicModeTcpListener.AcceptTcpClient();
                    musicModeTcpListener.Stop();
                    musicMode = true;
                    return true;
                }
            }
            catch (Exception exc) { lastError = exc; }
            return false;
        }
    }
}
