using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] request = new byte[12]; // napravili prazan niz od 12 bajtova
            //konkretizujemo sta nam treba
            ModbusWriteCommandParameters wParams = this.CommandParameters as ModbusWriteCommandParameters;

            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)wParams.TransactionId)).CopyTo(request, 0);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)wParams.ProtocolId)).CopyTo(request, 2);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)wParams.Length)).CopyTo(request, 4);

            request[6] = wParams.UnitId;
            request[7] = wParams.FunctionCode;
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)wParams.OutputAddress)).CopyTo(request, 8);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)wParams.Value)).CopyTo(request, 10);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            return new Dictionary<Tuple<PointType, ushort>, ushort>();
        }
    }
}