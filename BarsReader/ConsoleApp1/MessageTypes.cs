using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class MessageTypes
    {
        /// <summary>
        /// Class <c>MessageTypes</c> includes specific data for data exchange.
        /// </summary>
        public enum QueryTypes
        {
            /// <AllMeasuringData>
            /// Команда чтения “Считать измеренные данные” 
            /// (стр.58 п.2.14.4 руководства)
            /// -------------------------------------------------------------------------------------------------------------
            /// | Имя данных | Тип данных    | Количество байт | Допустимое значение | Description                          |
            /// -------------------------------------------------------------------------------------------------------------
            /// | Address    |               |                 |       0..249,255    | Адрес ведомого в линии связи         |
            /// | Code       |               |                 |           2         | Код команды                          |
            /// | Length     |               |                 |           1         | Длина блока передав.информации       |
            /// | CRC 16 Lo  | unsigned char |         1       |        0..255       | Младший байт контрольной суммы CRC16 |
            /// | CRC 16 Hi  |               |                 |        0..255       | Старший байт контрольной суммы CRC16 |
            /// -------------------------------------------------------------------------------------------------------------
            /// </AllMeasuringData>
            AllMeasuringData,
            None

        }

        // Parse receiving data as AllMeasuringData structure.
        public static Response_DataAllMeasuringData ParseAllMeasuringData(byte[] data)
        {
            Response_DataAllMeasuringData respData = new Response_DataAllMeasuringData(data);

            return respData;
        }

        //Parse receiving data as ... structure
        public static bool ParseNone(byte[] data)
        {
           return true;
        }


    }
}
