using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Modbus.Data;

namespace Modbus.Message
{
    using Unme.Common;

    internal 	class WriteMultipleCoilsRequest : ModbusMessageWithData<DiscreteCollection>, IModbusRequest
	{		
		public WriteMultipleCoilsRequest()
		{
		}

		public WriteMultipleCoilsRequest(byte slaveAddress, ushort startAddress, DiscreteCollection data)
			: base(slaveAddress, Modbus.WriteMultipleCoils)
		{
			StartAddress = startAddress;
			NumberOfPoints = (ushort) data.Count;
			ByteCount = (byte) ((data.Count + 7) / 8);
			Data = data;
		}

		public byte ByteCount
		{
			get { return MessageImpl.ByteCount.Value; }
			set { MessageImpl.ByteCount = value; }
		}

		public ushort NumberOfPoints
		{
			get
			{
				return MessageImpl.NumberOfPoints.Value;
			}
			set
			{
				if (value > Modbus.MaximumDiscreteRequestResponseSize)
					throw new ArgumentOutOfRangeException("NumberOfPoints", String.Format(CultureInfo.InvariantCulture, "Maximum amount of data {0} coils.", Modbus.MaximumDiscreteRequestResponseSize));

				MessageImpl.NumberOfPoints = value;
			}
		}

		public ushort StartAddress
		{
			get { return MessageImpl.StartAddress.Value; }
			set { MessageImpl.StartAddress = value; }
		}

		public override int MinimumFrameSize
		{
			get { return 7; }
		}

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "Write {0} coils starting at address {1}.", NumberOfPoints, StartAddress);
		}

		public void ValidateResponse(IModbusMessage response)
		{
			var typedResponse = (WriteMultipleCoilsResponse) response;

			if (StartAddress != typedResponse.StartAddress)
			{
				throw new IOException(String.Format(CultureInfo.InvariantCulture,
					"Unexpected start address in response. Expected {0}, received {1}.", 
					StartAddress, 
					typedResponse.StartAddress));
			}

			if (NumberOfPoints != typedResponse.NumberOfPoints)
			{
				throw new IOException(String.Format(CultureInfo.InvariantCulture,
					"Unexpected number of points in response. Expected {0}, received {1}.", 
					NumberOfPoints, 
					typedResponse.NumberOfPoints));
			}
		}

		protected override void InitializeUnique(byte[] frame)
		{
			if (frame.Length < MinimumFrameSize + frame[6])
				throw new FormatException("Message frame does not contain enough bytes.");

			StartAddress = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
			NumberOfPoints = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
			ByteCount = frame[6];
			Data = new DiscreteCollection(frame.Slice(7, ByteCount).ToArray());
		}
	}
}
