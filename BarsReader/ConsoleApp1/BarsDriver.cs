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

		private MessageTypes.QueryTypes messagetype;
		public MessageTypes.QueryTypes MessageType
		{
			get { return messagetype; }
			set { messagetype = value; }
		}


		#endregion

		#region Constructors
		public Query() { }

		//Type of Query without data
		public Query(MessageTypes.QueryTypes mtype, byte addr, byte code) 
		{
			messagetype = mtype;
			this.address = addr;
			this.code = code;
			this.length = 3;
			//this.crc = crc;
		}
		//Type of Query with data
		public Query(MessageTypes.QueryTypes mtype, byte addr, byte code, byte[] data)
		{
			messagetype = mtype;
			this.address = addr;
			this.code = code;
			this.data = data;
			try
			{
				this.length = (byte)(3 + checked(Convert.ToByte(data.Length)));
			}
			catch (OverflowException ex)
			{
				Console.WriteLine($"{this.ToString()} Overflow happened: {ex.Message}, StackTrace: {ex.StackTrace}");
			}
		}
		#endregion
	}

	class ResponseData
	{
		/// <summary>
		/// Class <c>ResponseData</c> contains members of response.
		/// </summary>
		#region Properties
		//
		private byte[] data;
		public byte[] Data
		{
			get { return data; }
			set { data = value; }
		}
		#endregion

    }

	class Response_DataAllMeasuringData
	{
		/// <summary>
		/// Class <c>Response_DataAllMeasuringData</c> contains parses data response.
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
		}

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

		private byte[] crc16;
		public byte[] Crc16
		{
			get { return crc16; }
			set { crc16 = value; }
		}

		#endregion

		#region Constructor

		public Response_DataAllMeasuringData(byte[] data)
		{
			this.address = data[0];
			this.code = data[1];
			this.length = data[2];
			this.data1 = BitConverter.ToSingle(data, 3);
			this.data2 = BitConverter.ToSingle(data, 7);
			this.data3 = BitConverter.ToSingle(data, 11);
			this.data4 = BitConverter.ToSingle(data, 15);
			this.data5 = data[16];
			this.error = data[17];
			Buffer.BlockCopy(data, 18, this.crc16, 0, 2);
		}
		#endregion


	}

    static class BarsDriver
	{
		/// <summary>
		/// Class <c>BarsDriver</c> includes static methods for:
		/// - Calculate checksum.
		/// - ...
		/// </summary>

		public static byte[] CalcCRC(byte[] mes)//, int size)
		{
			ushort Crc16 = 0xffff;

			for (int i = 0; i < mes.Length; i++)
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

			return BitConverter.GetBytes(Crc16);

		}


		public static void CreateRequest(byte NumChannel)
		{

		}

		public static void DataParse(byte[] mes, int channel)
		{

		}
	}
}
