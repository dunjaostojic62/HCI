using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] request = new byte[12];
            ModbusReadCommandParameters rParams = this.CommandParameters as ModbusReadCommandParameters;

            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)rParams.TransactionId)).CopyTo(request, 0);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)rParams.ProtocolId)).CopyTo(request, 2);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)rParams.Length)).CopyTo(request, 4);
            request[6] = rParams.UnitId;
            request[7] = rParams.FunctionCode;
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)rParams.StartAddress)).CopyTo(request, 8);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)rParams.Quantity)).CopyTo(request, 10);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
          
            Dictionary<Tuple<PointType, ushort>, ushort> dict = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters rParams = this.CommandParameters as ModbusReadCommandParameters;

            for (int i = 0; i < rParams.Quantity; i++)
            {
                ushort value = (ushort)((response[9 + i / 8] >> (i % 8)) & 1);
                dict.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, (ushort)(rParams.StartAddress + i)), value);
            }

            return dict;
        }
    }
}