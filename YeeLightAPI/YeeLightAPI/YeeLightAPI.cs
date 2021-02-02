using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YeeLightAPI.YeeLightConstants;
using YeeLightAPI.YeeLightExceptions;

namespace YeeLightAPI
{
    public class YeeLightDevice
    {
        private IPAddress lightIpAddress = null;
        private ushort lightPort = 0;
        private TcpClient yeelightTcpClient = new TcpClient();
        private bool musicMode = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public YeeLightDevice() { }

        /// <summary>
        /// Constructor that takes IP address and a TCP port as arguments and sets the internal IP address and TCP port accordingly
        /// </summary>
        /// <param name="ipAddress"> IP address of the light device</param>
        /// <param name="port"> TCP port of the light device, default value is Constants.DefaultCommandPort</param>
        public YeeLightDevice(IPAddress ipAddress, ushort port = Constants.DefaultCommandPort)
        {
            lightIpAddress = ipAddress;
            lightPort = port;
        }

        /// <summary>
        /// Constructor that takes hostname and a TCP port as arguments and sets the internal IP address and TCP port accordingly
        /// by resolving the hostname to an IP address
        /// </summary>
        /// <param name="hostname"> IP address of the light device</param>
        /// <param name="port"> TCP port of the light device, default value is Constants.DefaultCommandPort</param>
        /// <remarks>
        /// Throws if hostname can't be resolve to an IP address
        /// </remarks>
        public YeeLightDevice(string hostname, ushort port = Constants.DefaultCommandPort)
        {
            if (!IPAddress.TryParse(hostname, out lightIpAddress))
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                if (hostEntry.AddressList.Length > 0)
                {
                    lightIpAddress = hostEntry.AddressList[0];
                }
                else
                {
                    throw new Exceptions.InvalidHostnameArgument();
                }
            }
            lightPort = port;
        }

        /// <summary>
        /// Connects to the light device using the last set IP address and TCP port
        /// </summary>
        /// <remarks>
        /// If TcpClient.Connect throws, the exception is passed to the caller
        /// </remarks>
        public void Connect()
        {
            musicMode = false;
            yeelightTcpClient.Connect(lightIpAddress, lightPort);
        }

        /// <summary>
        /// Returns the internal TCP port and IP address
        /// </summary>
        /// <returns>
        /// A tuple that has the internal IP address and TCP port
        /// </returns>
        public (IPAddress ipAddress, ushort port) GetLightIPAddressAndPort()
        {
            return (ipAddress: lightIpAddress, port: lightPort);
        }

        /// <summary>
        /// Sets internal IP address and TCP port to the ones provided in the arguments
        /// </summary>
        /// <param name="ipAddress"> IP address of the light device</param>
        /// <param name="port"> TCP port of the light device</param>
        public void SetLightIPAddressAndPort(IPAddress ipAddress, ushort port)
        {
            lightIpAddress = ipAddress;
            lightPort = port;
        }

        /// <summary>
        /// Returns true if there is a open connection to the light device, otherwise false
        /// </summary>
        /// <returns>
        /// true if there is a open connection, otherwise false
        /// </returns>
        public bool IsConnected()
        {
            return yeelightTcpClient.Connected;
        }

        /// <summary>
        /// Returns true if there is a open connection to the light device and the connection is a music mode connection, otherwise false
        /// </summary>
        /// <returns>
        /// true if there is a open connection and it is a music mode connection, otherwise false
        /// </returns>
        public bool IsMusicMode()
        {
            return musicMode && IsConnected();
        }

        /// <summary>
        /// Closes the connection to the light device
        /// </summary>
        /// <remarks>
        /// Throws DeviceIsNotConnected exception if there is no open connection to the light device
        /// </remarks>
        public void CloseConnection()
        {
            ThrowExceptionIfNotConnected();
            yeelightTcpClient.Close();

            yeelightTcpClient = new TcpClient();
            musicMode = false;
        }

        /// <summary>
        /// Sets the light device's brightness
        /// </summary>
        /// <param name="brightness"> The brightness to set the light device to, range is 0 to 100.</param>
        /// <param name="duration"> Duration of the effect, minimum value for this argument is Constants.MinValueForDurationParameter and so is the default value</param>
        /// <param name="effectType"> Type of the effect, can be anything from Constants.EffectParamValues and default value is Constants.EffectParamValues.SUDDEN</param>
        /// <remarks>
        /// Throws if duration or brightness arguments are out of range or if device is not connected
        /// </remarks>
        public bool SetBrightness(int brightness,
            int duration = Constants.MinValueForDurationParameter,
            Constants.EffectParamValues effectType = Constants.EffectParamValues.SUDDEN)
        {
            ThrowExceptionIfIntArgIsOutOfRange("duration", duration, Constants.MinValueForDurationParameter);
            ThrowExceptionIfIntArgIsOutOfRange("brightness", brightness, 0, 100);
            ThrowExceptionIfNotConnected();
            return SendCommandMessage(1, "set_bright", new string[] { brightness.ToString(System.Globalization.CultureInfo.InvariantCulture), Utils.GetJsonStringFromParamEnum(effectType), duration.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        }

        /// <summary>
        /// Sets the light device's power state
        /// </summary>
        /// <param name="powerState"> The power state to set the light device to, can be anything from Constants.PowerStateParamValues</param>
        /// <param name="duration"> Duration of the effect, minimum value for this argument is Constants.MinValueForDurationParameter and so is the default value</param>
        /// <param name="effectType"> Type of the effect, can be anything from Constants.EffectParamValues and default value is Constants.EffectParamValues.SUDDEN</param>
        /// <remarks>
        /// Throws if duration argument is out of range or if device is not connected
        /// </remarks>
        public bool SetPower(Constants.PowerStateParamValues powerState,
            int duration = Constants.MinValueForDurationParameter,
            Constants.EffectParamValues effectType = Constants.EffectParamValues.SUDDEN)
        {
            ThrowExceptionIfIntArgIsOutOfRange("duration", duration, Constants.MinValueForDurationParameter);
            ThrowExceptionIfNotConnected();
            return SendCommandMessage(1, "set_power",
                new string[] { Utils.GetJsonStringFromParamEnum(powerState), Utils.GetJsonStringFromParamEnum(effectType), duration.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        }

        /// <summary>
        /// Sets the light device's color
        /// </summary>
        /// <param name="red"> The integer value of red channel to set the light device's color to</param>
        /// <param name="green"> The integer value of green channel to set the light device's color to</param>
        /// <param name="blue"> The integer value of blue channel to set the light device's color to</param>
        /// <param name="duration"> Duration of the effect, minimum value for this argument is Constants.MinValueForDurationParameter and so is the default value</param>
        /// <param name="effectType"> Type of the effect, can be anything from Constants.EffectParamValues and default value is Constants.EffectParamValues.SUDDEN</param>
        /// <remarks>
        /// Throws if duration argument is out of range or if device is not connected
        /// </remarks>
        public bool SetColor(byte red, byte green, byte blue,
            int duration = Constants.MinValueForDurationParameter,
            Constants.EffectParamValues effectType = Constants.EffectParamValues.SUDDEN)
        {
            ThrowExceptionIfIntArgIsOutOfRange("duration", duration, Constants.MinValueForDurationParameter);
            ThrowExceptionIfNotConnected();
            int value = (red << 16) | (green << 8) | blue;
            effectType.ToString();
            return SendCommandMessage(1, "set_rgb", new string[] { value.ToString(System.Globalization.CultureInfo.InvariantCulture), Utils.GetJsonStringFromParamEnum(effectType), duration.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        }

        /// <summary>
        /// Closes the normal connection to the light 
        /// and tells light to make a music mode connection to the local device according to the arguments provided.
        /// The music mode connections have no command rate limit.
        /// </summary>
        /// <param name="localIP"> The IP address to tell to the light to connect to</param>
        /// <param name="localPort"> The TCP port to tell to the light to connect to</param>
        /// <param name="state"> If set to true the connection will be switched to music mode connection, otherwise the connection will be switched to normal connection</param>
        /// <remarks>
        /// Throws if state is set to true and device is not connected or if the current connection is already music mode connection
        /// Throws if there is no connection made from light in 1000ms
        /// Throws if there was no connection made in the end of the function
        /// </remarks>
        public void SetMusicMode(IPAddress localIP, ushort localPort, bool state)
        {
            if (state)
            {
                ThrowExceptionIfNotConnected();
                ThrowExceptionIfInMusicMode();
                TcpListener musicModeTcpListener = new TcpListener(localIP, localPort);
                musicModeTcpListener.Start();
                SendCommandMessage(1, "set_music", new string[] { "1", $"\"{localIP}\"", $"{localPort}" });
                CloseConnection();

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
            }
            else
            {
                if (IsConnected())
                {
                    SendCommandMessage(1, "set_music", new string[] { "0" });
                }
                CloseConnection();
                Connect();
                musicMode = false;
            }
            ThrowExceptionIfNotConnected();
        }

        
        public string[] GetProperties(string[] props)
        {
            return (string[])SendCommandAndGetMessage(1, "get_prop", props.Select(x => '\"' + x + '\"').ToArray()).Result;
        }

        private static void ThrowExceptionIfIntArgIsOutOfRange(string argumentName, int argument, int lowLimit, int highLimit = Int32.MaxValue)
        {
            if (argument < lowLimit || argument > highLimit)
            {
                throw new ArgumentOutOfRangeException($"\"{argumentName}\"'s range is {lowLimit} to {highLimit}");
            }
        }
        private void ThrowExceptionIfNotConnected()
        {
            if (!IsConnected())
            {
                throw new Exceptions.DeviceIsNotConnected();
            }
        }
        private void ThrowExceptionIfInMusicMode()
        {
            if (IsMusicMode())
            {
                throw new Exceptions.DeviceIsAlreadyInMusicMode();
            }
        }
        private bool SendCommandMessage(int id_pair, string method_pair, string[] params_pair)
        {
            //TODO: use proper json serializer library here
            string commandMessage = $"{{\"id\":{id_pair},\"method\":\"{method_pair}\",\"params\":[{string.Join(",", params_pair)}]}}\r\n";
            return SendString(commandMessage);
        }

        class ResponseMessage
        {
            [JsonProperty("id")]
            public int Id;

            [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
            public object[] Result;

            [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
            public object Error;
        };

        private ResponseMessage SendCommandAndGetMessage(int id_pair, string method_pair, string[] params_pair)
        {
            using (var stream = yeelightTcpClient.GetStream())
            {
                //TODO: remove the stream read buffer flushing as soon as other methods requiring responses are implemented
                byte[] tmpBuf = new byte[1024];
                while (stream.DataAvailable) { stream.Read(tmpBuf, 0, tmpBuf.Length); }
            }

            SendCommandMessage(id_pair, method_pair, params_pair);
            return JsonConvert.DeserializeObject<ResponseMessage>(ReadString());
        }

        private string ReadString()
        {
            ThrowExceptionIfNotConnected();

            string retVal = string.Empty;
            using (var stream = yeelightTcpClient.GetStream())
            {
                byte[] buffer = new byte[1024];

                int numBytesRead;
                while ((numBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    retVal += Encoding.ASCII.GetString(buffer, 0, numBytesRead);
                }
            }

            return retVal;
        }
        private bool SendString(string command)
        {
            ThrowExceptionIfNotConnected();

            byte[] bytesOfCommand = Encoding.ASCII.GetBytes(command);
            return yeelightTcpClient.Client.Send(bytesOfCommand) == bytesOfCommand.Length;
        }
    }
}
