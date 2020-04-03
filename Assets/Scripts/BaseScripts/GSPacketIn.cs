// using log4net;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace Game.Base.Packets
{
	public class GSPacketIn : PacketIn
	{
		public const ushort HDR_SIZE = 20;
		public const short HEADER = 29099;
		// private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		protected short m_code;
		protected int m_cliendId;
		protected int m_parameter1;
		protected int m_parameter2;
		public short Code
		{
			get
			{
				return this.m_code;
			}
			set
			{
				this.m_code = value;
			}
		}
		public int ClientID
		{
			get
			{
				return this.m_cliendId;
			}
			set
			{
				this.m_cliendId = value;
			}
		}
		public int Parameter1
		{
			get
			{
				return this.m_parameter1;
			}
			set
			{
				this.m_parameter1 = value;
			}
		}
		public int Parameter2
		{
			get
			{
				return this.m_parameter2;
			}
			set
			{
				this.m_parameter2 = value;
			}
		}
		public GSPacketIn(byte[] buf, int size) : base(buf, size)
		{
		}
		public GSPacketIn(short code) : this(code, 0, 2048)
		{
		}
		public GSPacketIn(short code, int clientId) : this(code, clientId, 2048)
		{
		}
		public GSPacketIn(short code, int clientId, int size) : base(new byte[size], 20)
		{
			this.m_code = code;
			this.m_cliendId = clientId;
			this.m_offset = 20;
		}
		public void ReadHeader()
		{
            Console.WriteLine("read header");
			this.ReadShort();
			this.m_length = (int)this.ReadShort();
			this.ReadShort();
			this.m_code = this.ReadShort();
			this.m_cliendId = this.ReadInt();
			this.m_parameter1 = this.ReadInt();
			this.m_parameter2 = this.ReadInt();
		}
		public void WriteHeader()
		{
			GSPacketIn obj = this;
			Monitor.Enter(this);
			try
			{
				int offset = this.m_offset;
				this.m_offset = 0;
				base.WriteShort(29099);
				base.WriteShort((short)this.m_length);
				base.WriteShort(this.checkSum());
				base.WriteShort(this.m_code);
                base.WriteInt(this.m_cliendId);
                base.WriteInt(this.m_parameter1);
                base.WriteInt(this.m_parameter2);
                this.m_offset = offset;
			}
			finally
			{
				Monitor.Exit(obj);
			}
			obj = this;
			Monitor.Enter(this);
			try
			{
				int offset = this.m_offset;
				this.m_offset = 0;
				base.WriteShort(29099);
				base.WriteShort((short)this.m_length);
				base.WriteShort(this.checkSum());
				base.WriteShort(this.m_code);
				base.WriteInt(this.m_cliendId);
				base.WriteInt(this.m_parameter1);
				base.WriteInt(this.m_parameter2);
				this.m_offset = offset;
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
		public short checkSum()
		{
			short num = 119;
			int i = 6;
			while (i < this.m_length)
			{
				num += (short)this.m_buffer[i++];
			}
			int num2 = (int)(num & 32639);
			// UnityEngine.Debug.Log(num2);
			// GameController.LogToScreen(num2.ToString());
			return (short)num2;
		}
		public void WritePacket(GSPacketIn pkg)
		{
			pkg.WriteHeader();
			this.Write(pkg.Buffer, 0, pkg.Length);
		}
		public GSPacketIn ReadPacket()
		{
			byte[] array = this.ReadBytes();
			GSPacketIn gSPacketIn = new GSPacketIn(array, array.Length);
			gSPacketIn.ReadHeader();
			return gSPacketIn;
		}
		public void Compress()
		{
			byte[] array = Marshal.Compress(this.m_buffer, 20, base.Length - 20);
			this.m_offset = 20;
			this.Write(array);
			this.m_length = array.Length + 20;
		}
		public void UnCompress()
		{
			// byte[] array = Marshal.Uncompress(this.m_buffer);
			// this.m_offset = 0;
			// this.Write(array);
			// this.m_length = array.Length - 20;
		}
		public void ClearContext()
		{
			this.m_offset = 20;
			this.m_length = 20;
		}
		public GSPacketIn Clone()
		{
			GSPacketIn gSPacketIn = new GSPacketIn(this.m_buffer, this.m_length);
			gSPacketIn.ReadHeader();
			gSPacketIn.Offset = this.m_length;
			return gSPacketIn;
		}
	}
}
