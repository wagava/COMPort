using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Query
    {
        /// <summary>
        /// Class <c>Query</c> contains members of queries.
        /// </summary>

        #region Properties
        private byte address;
        public byte Address
        {
            get { return address; }
            set { address = value; }
        }

        private byte code;
        public byte Code
        {
            get { return code; }
            set { code = value; }
        }

        private byte length;
        public byte Length
        {
            get { return length; }
            set { length = value; }
        }

        private byte[] data;
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        private byte[] crc;
        public byte[] CRC
        {
            get { return crc; }
            set
            {
                crc = value;
                int offset = 3;

                if (data != null && data.Length > 0)
                {
                    message = new byte[3 + data.Length + crc.Length];
                    Buffer.BlockCopy(data, 0, message, offset, data.Length);
                    offset += data.Length;
                }
                else
                    message = new byte[3 + crc.Length];
                Buffer.BlockCopy(crc, 0, message, offset, crc.Length);
                message[0] = address;
                message[1] = code;
                message[2] = length;
            }
        }

        private byte[] message;
        public byte[] Message
        {
            get { return message; }
        }


        private byte responsemessagelength;
        public byte ResponseMessageLength
        {
            get { return responsemessagelength; }
        }

        private MessageTypes.QueryTypes messagetype;
        public MessageTypes.QueryTypes MessageType
        {
            get { return messagetype; }
            set { messagetype = value; }
        }


        #endregion

        #region Constructors
        public Query() { }

        // Query type without data
        public Query(MessageTypes.QueryTypes mtype, byte addr, byte code, byte length)
        {
            messagetype = mtype;
            this.address = addr;
            this.code = code;
            this.length = length;
            this.responsemessagelength = 23;
        }

        // Query type with data
        public Query(MessageTypes.QueryTypes mtype, byte addr, byte code, byte length, byte[] data)
        {
            messagetype = mtype;
            this.address = addr;
            this.code = code;
            this.data = data;
            this.length = length;
            this.responsemessagelength = 10;
        }
        #endregion
    }

    class Response_Data
    {
        /// <summary>
        /// Class <c>Response_Data</c> contains common properties of all 'Response...' classes.
        /// </summary>

        #region Properties

        private protected byte address;
        public byte Address
        {
            get { return address; }
            set { address = value; }
        }

        private protected byte code;
        public byte Code
        {
            get { return code; }
            set { code = value; }
        }

        private protected byte length;
        public byte Length
        {
            get { return length; }
        }

        private protected byte[] crc16;
        public byte[] Crc16
        {
            get { return crc16; }
            set { crc16 = value; }
        }
        #endregion
    }


    class Response_DataCurrentParam : Response_Data
    {
        /// <summary>
        /// Class <c>Response_CurrentCapacity</c> contains parses data response.
        /// </summary>

        #region Properties

        private float data;
        public float Data
        {
            get { return data; }
            set { data = value; }
        }

        private byte error;
        public byte Error
        {
            get { return error; }
            set { error = value; }
        }

        #endregion

        #region Constructor

        public Response_DataCurrentParam(byte[] data)
        {
            Console.WriteLine($"datalength = {data.Length}");
            this.address = data[0];
            this.code = data[1];
            this.length = data[2];
            this.data = BitConverter.ToSingle(BarsDriver.SwapBytes(data.Skip(3).Take(4).ToArray()), 0);// 3); //Float - Big Endian (ABCD)
            this.error = data[7];
            this.crc16 = new byte[2] { 0, 0 };
            Buffer.BlockCopy(data, 8, this.crc16, 0, 2);
        }
        #endregion


    }

    class Response_DataAllMeasuringData : Response_Data
    {
        /// <summary>
        /// Class <c>Response_DataAllMeasuringData</c> contains parses data response.
        /// </summary>

        #region Properties

        private float data1;
        public float Data1
        {
            get { return data1; }
            set { data1 = value; }
        }

        private float data2;
        public float Data2
        {
            get { return data2; }
            set { data2 = value; }
        }
        private float data3;
        public float Data3
        {
            get { return data3; }
            set { data3 = value; }
        }

        private float data4;
        public float Data4
        {
            get { return data4; }
            set { data4 = value; }
        }

        private byte data5;
        public byte Data5
        {
            get { return data5; }
            set { data5 = value; }
        }
        private byte error;
        public byte Error
        {
            get { return error; }
            set { error = value; }
        }


        #endregion

        #region Constructor

        public Response_DataAllMeasuringData(byte[] data)
        {
            Console.WriteLine($"datalength = {data.Length}");
            this.address = data[0];
            this.code = data[1];
            this.length = data[2];
            this.data1 = BitConverter.ToSingle(BarsDriver.SwapBytes(data.Skip(3).Take(4).ToArray()), 0);// 3); //Float - Big Endian (ABCD)
            this.data2 = BitConverter.ToSingle(BarsDriver.SwapBytes(data.Skip(7).Take(4).ToArray()), 0);//), 7);
            this.data3 = BitConverter.ToSingle(BarsDriver.SwapBytes(data.Skip(11).Take(4).ToArray()), 0);//), 11);
            this.data4 = BitConverter.ToSingle(BarsDriver.SwapBytes(data.Skip(15).Take(4).ToArray()), 0);//), 15);
            this.data5 = data[19];
            this.error = data[20];
            this.crc16 = new byte[2] { 0, 0 };
            Buffer.BlockCopy(data, 21, this.crc16, 0, 2);
        }
        #endregion

    }


    static class BarsDriver
    {
        /// <summary>
        /// Class <c>BarsDriver</c> includes static methods for:
        /// - Calculate checksum.
        /// - Swap bytes.
        /// </summary>

        public static byte[] CalcCRC(byte[] mes)//, int size)
        {
            ushort Crc16 = 0xffff;
            try
            {
                //for (int i = 0; i < mes.Length; i++) //можно использовать это условие, т.к. ниже вычисляется та же длина
                for (int i = 0; i < mes[2] + 2; i++)
                {
                    Crc16 = (ushort)(Crc16 ^ mes[i]);
                    for (int j = 0; j < 8; j++)
                    {
                        bool flag = ((Crc16 & 0b0001) == 1);
                        Crc16 >>= 1;
                        if (flag)
                            Crc16 ^= 0xA001;
                    }
                }
            }
            catch (Exception ex)
            { return new byte[2] { 0, 0 }; }

            return BitConverter.GetBytes(Crc16);

        }

        public static byte[] SwapBytes(byte[] mes)
        {
            return new byte[] { mes[3], mes[2], mes[1], mes[0] };

        }

    }
}
