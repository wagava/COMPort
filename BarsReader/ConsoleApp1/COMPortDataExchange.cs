using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace ConsoleApp1
{
    class COMPortDataExchange : IDisposable
    {
        /// <summary>
        /// Class <c>COMPortDataExchange</c> is Main module.
        /// </summary>

        #region Properties, Events, etc

        SerialPort port;
        public byte[] BytesFromDevice;
        public PortData.ConnectionStatus ConnStatus; // Port Connection Status
        public PortData.ExchangeState ExchStatus; // Status of Data Exchanging
        private List<byte> CurrentBytesReceived = new List<byte>();

        List<Query> Queries;
        int indexOfQueries = -1; // Index can't less zero: default value

        TimeSpan timeout = new TimeSpan(0, 0, 5); // Idle Timout for answer
        DateTime RequestTimeFix;
        byte slaveid;

        public delegate void MessageGot(byte[] message);
        public event MessageGot MesGot;

        #endregion

        #region Constructors
        public COMPortDataExchange()
        {
            port = new SerialPort();
            port.DataReceived += new SerialDataReceivedEventHandler(Receiver);
        }

        public COMPortDataExchange(string PortNum, string SlaveId)
        {
            slaveid = Convert.ToByte(SlaveId);
            port = new SerialPort();
            port.DataReceived += new SerialDataReceivedEventHandler(Receiver);
            this.Connect($"COM{PortNum}");
            Queries = new List<Query>();
            Init();
        }
        #endregion

        #region Poll starting
        private async void Init()
        {

            ExchStatus = PortData.ExchangeState.Free;
            // Add useful request for polling. Checksum calculating.
            Queries.Add(new Query(MessageTypes.QueryTypes.AllMeasuringData, slaveid, 2, 1)); // The command: 'Read the measured data'
            Queries.Add(new Query(MessageTypes.QueryTypes.CurrentParam, slaveid, 1, 2, new byte[] { 1 })); // The command: 'Current distance'
            Queries.Add(new Query(MessageTypes.QueryTypes.CurrentParam, slaveid, 1, 2, new byte[] { 2 })); // The command: 'Current level'
            Queries.Add(new Query(MessageTypes.QueryTypes.CurrentParam, slaveid, 1, 2, new byte[] { 3 })); // The command: 'Free space'
            Queries.Add(new Query(MessageTypes.QueryTypes.CurrentParam, slaveid, 1, 2, new byte[] { 6 })); // The command: 'Current volume'

            for (int i = 0; i < Queries.Count; i++)
            {
                byte[] mes;
                mes = new byte[5 + (Queries[i].Data != null ? Queries[i].Data.Length : 0)];
                mes[0] = Queries[i].Address;
                mes[1] = Queries[i].Code;
                mes[2] = Queries[i].Length;
                if (Queries[i].Data != null)
                    Buffer.BlockCopy(Queries[i].Data, 0, mes, 3, Queries[i].Data.Length);

                Queries[i].CRC = BarsDriver.CalcCRC(mes);
                Buffer.BlockCopy(Queries[i].CRC, 0, mes, mes.Length - 2, Queries[i].CRC.Length);
            }

            await RunTaskPoll();
        }
        #endregion

        #region MainFunc
        private async Task RunTaskPoll()
        {
            await Task.Run(() => Running());
        }

        private void Running()
        {
            while (true)
            {

                if ((ExchStatus == PortData.ExchangeState.Sending) & (DateTime.Now - RequestTimeFix >= timeout))
                {
                    ExchStatus = PortData.ExchangeState.Timeout;
                    Console.WriteLine($"{DateTime.Now}: Running(): Timeout");
                }
                if ((ExchStatus == PortData.ExchangeState.Free) || (ExchStatus == PortData.ExchangeState.Timeout))
                {
                    Thread.Sleep(2000);
                    indexOfQueries = indexOfQueries == Queries.Count - 1 ? 0 : ++indexOfQueries;
                    RequestTimeFix = DateTime.Now;
                    Send(Queries[indexOfQueries]);

                }

                // Data is received by subscribing to the dataReceived SerialPort event.

            }
        }

        #endregion

        #region Methods
        // Send query to device.
        private bool Send(Query query)
        {
            ExchStatus = PortData.ExchangeState.Sending;
            if (CurrentBytesReceived != null && CurrentBytesReceived.Count > 0)
                CurrentBytesReceived.Clear();
            this.Write(query.Message);

            return true;

        }
        private void MesReceived()
        {
            MesGot?.Invoke(BytesFromDevice);
        }

        // Execute the method when port received the data.
        public void Receiver(object sender, SerialDataReceivedEventArgs e)
        {
            ExchStatus = PortData.ExchangeState.Receiving;

            List<byte> ByteReceived = new List<byte>();

            if (port.IsOpen)
            {
                SerialPort sp = (SerialPort)sender;

                bool boolRead = true;
                while (boolRead)
                {
                    try
                    {
                        // Read every byte
                        ByteReceived.Add((byte)sp.BaseStream.ReadByte());

                        // Bytes is no longer
                        if (sp.BytesToRead == 0)
                            // The buffer is empty
                            boolRead = false;
                    }
                    // Timeout happened...
                    catch (TimeoutException ex)
                    {
                        Console.WriteLine($"Error data reading from port, Error: {ex.Message}, StackTrace: {ex.StackTrace} ");
                        boolRead = false;
                    }
                }
                //port.DiscardInBuffer();
                Console.WriteLine(string.Join(" ", ByteReceived.ToArray()));
                try
                {
                    CurrentBytesReceived.AddRange(ByteReceived);

                    if ((CurrentBytesReceived.Count == Queries[indexOfQueries].ResponseMessageLength
                        && Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.AllMeasuringData)
                        || (CurrentBytesReceived.Count == Queries[indexOfQueries].ResponseMessageLength
                        && Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.CurrentParam))
                        Console.WriteLine(string.Join(" ", CurrentBytesReceived.ToArray()));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
                // Recognize the message
                try
                {


                    if ((CurrentBytesReceived.Count == Queries[indexOfQueries].ResponseMessageLength
                        && Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.AllMeasuringData)
                        || (CurrentBytesReceived.Count == Queries[indexOfQueries].ResponseMessageLength
                        && Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.CurrentParam)

                        )
                    {
                        BytesFromDevice = CurrentBytesReceived.ToArray();
                        byte[] buf = CurrentBytesReceived.ToArray();

                        Console.WriteLine("CRC: " + string.Join(" ", BarsDriver.CalcCRC((buf.Take(buf.Length - 2)).ToArray())));
                        //Console.WriteLine((new byte[2] { buf[buf.Length - 2], buf[buf.Length - 1] }).ToArray().ToString());
                        // Verify checksum
                        if (BarsDriver.CalcCRC((buf.Take(buf.Length - 2)).ToArray())
                            .SequenceEqual(new byte[2] { buf[buf.Length - 2], buf[buf.Length - 1] }))
                        {
                            if (Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.AllMeasuringData)
                            {
                                // All measured data
                                Response_DataAllMeasuringData rdata;
                                try
                                {
                                    rdata = MessageTypes.ParseAllMeasuringData(BytesFromDevice.ToArray());
                                    Console.WriteLine($"Receiver: Data is good. \n Address: {rdata.Address} \n " +
                                                                                $" Code: {rdata.Code} \n " +
                                                                                $" Length: {rdata.Length} \n " +
                                                                                $" Data_1: {rdata.Data1} \n " +
                                                                                $" Data_2: {rdata.Data2} \n " +
                                                                                $" Data_3: {rdata.Data3} \n " +
                                                                                $" Data_4: {rdata.Data4} \n " +
                                                                                $" Data_5: {rdata.Data5} \n " +
                                                                                $" Error: {rdata.Error} \n " +
                                                                                $" CRC: {rdata.Crc16}:X2 \n");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Receiver(): Error parsing the message from device: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }

                            }
                            else if (Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.CurrentParam)
                            {
                                // Only one measuring
                                Response_DataCurrentParam rdata;
                                try
                                {
                                    rdata = MessageTypes.ParseCurrentCapacity(BytesFromDevice.ToArray());
                                    Console.WriteLine($"Receiver: Data is good. \n Address: {rdata.Address} \n " +
                                                                                $" Code: {rdata.Code} \n " +
                                                                                $" Length: {rdata.Length} \n " +
                                                                                $" Data: {rdata.Data} \n " +
                                                                                $" Error: {rdata.Error} \n " +
                                                                                $" CRC: {rdata.Crc16}:X2 \n");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Receiver(): Error parsing the message from device: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }
                            }
                            else if (Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.None)
                            {
                                Console.WriteLine($"Receiver: Data is good. Update struture class");
                            }
                            else
                                Console.WriteLine("Receiver(): Data is corrupted");

                        }
                        else if (Queries[indexOfQueries].MessageType == MessageTypes.QueryTypes.None)
                        { }
                        else
                        { }

                    }
                    ExchStatus = PortData.ExchangeState.Free;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);

                }
            }

        }

        // Configure the port properties to open the port
        public bool Connect(string PortName)
        {
            try
            {
                port.PortName = PortName; // Example: 'COM2'
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = Parity.Mark;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 2000;
                port.WriteTimeout = 2000;
                port.DtrEnable = false;
                port.RtsEnable = false;
                //port.WriteBufferSize = 6;
                //port.Handshake = Handshake.None;
                //port.ReadBufferSize = 4096;
                //port.WriteBufferSize = 4096;
                port.Open();
                // port.SetXonXoffChars(0x00, 0x00, 0x00, 0x00, 0x00);
                ConnStatus = PortData.ConnectionStatus.Connected;
                Console.WriteLine($"Port {PortName} opened");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connect(): Error by connecting to port: {ex.Message}, StackTrace: {ex.StackTrace}");
                ConnStatus = PortData.ConnectionStatus.Disconnected;
                return false;
            }

        }
        public bool Disconnect()
        {
            try
            {
                port.Close();
                Console.WriteLine($"Disconnect(): Com port closed");
                ConnStatus = PortData.ConnectionStatus.Disconnected;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnect(): Error by disconnecting: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }

        }

        public byte[] Read(int sizeBuf)
        {
            byte[] resp = { };
            try
            {
                port.Read(resp, 0, sizeBuf);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read(): Error by reading: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
            return resp;
        }

        public bool Write(byte[] req)
        {
            try
            {
                // It must be very funny to change the parity along request data process, but device need it
                Console.WriteLine($"{DateTime.Now.ToString("O")}: Send message: {BitConverter.ToString(req)}");
                port.Parity = Parity.Mark;
                port.Write(req, 0, 1);
                Thread.Sleep(5);
                port.Parity = Parity.Space;
                port.Write(req, 1, req.Length - 1);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Write(): Error by writing: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public void Dispose()
        {
            this.Disconnect();
        }
        #endregion
    }
}
