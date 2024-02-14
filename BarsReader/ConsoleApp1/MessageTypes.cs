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
            /// | Имя данных | Тип данных    | Количество байт | Допустимое значение | Описание                             |
            /// -------------------------------------------------------------------------------------------------------------
            /// | Address    |               |                 |       0..249,255    | Адрес ведомого в линии связи         |
            /// | Code       |               |                 |           2         | Код команды                          |
            /// | Length     |               |                 |           1         | Длина блока передав.информации       |
            /// | CRC 16 Lo  | unsigned char |         1       |        0..255       | Младший байт контрольной суммы CRC16 |
            /// | CRC 16 Hi  |               |                 |        0..255       | Старший байт контрольной суммы CRC16 |
            /// -------------------------------------------------------------------------------------------------------------
            /// </AllMeasuringData>
            AllMeasuringData,
            /// <CurrentParam>
            /// Команда чтения “Считать текущее расстояние”, “Считать текущий уровень”,
            /// “Считать свободное пространство”, “Считать текущий объем” 
            /// (стр.57 п.2.14.3 руководства)
            /// -------------------------------------------------------------------------------------------------------------
            /// | Имя данных | Тип данных    | Количество байт | Допустимое значение |  Описание                            |
            /// -------------------------------------------------------------------------------------------------------------
            /// | Address    |               |                 |       0..249,255    | Адрес ведомого в линии связи         |
            /// | Code       |               |                 |           2         | Код команды                          |
            /// | Length     |               |                 |           1         | Длина блока передав.информации       |
            /// | Data       | unsigned char |         1       |        1,2,3,6      | текущее расстояние/уровень/свободное |
            /// |            |               |                 |                     | пространство/текущий объем           |
            /// | CRC 16 Lo  |               |                 |        0..255       | Младший байт контрольной суммы CRC16 |
            /// | CRC 16 Hi  |               |                 |        0..255       | Старший байт контрольной суммы CRC16 |
            /// -------------------------------------------------------------------------------------------------------------
            /// </CurrentCapacity>
            CurrentParam,
            None

        }

        // Parse receiving data as AllMeasuringData structure.
        public static Response_DataAllMeasuringData ParseAllMeasuringData(byte[] data)
        {
            Response_DataAllMeasuringData respData = new Response_DataAllMeasuringData(data);
            return respData;
        }

        // Parse receiving data as CurrentParam structure
        public static Response_DataCurrentParam ParseCurrentCapacity(byte[] data)
        {
            Response_DataCurrentParam respData = new Response_DataCurrentParam(data);
            return respData;
        }

        // Parse receiving data as ... structure
        public static bool ParseNone(byte[] data)
        {
            return true;
        }

    }
}
