// using log4net;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
namespace Game.Base
{
    public class PacketIn
    {
        protected byte[] m_buffer;
        protected int m_length;
        protected int m_offset;
        public static int[] SEND_KEY = new int[]
		{
			174,
			191,
			86,
			120,
			171,
			205,
			239,
			241
		};
        public volatile bool isSended = true;
        public volatile int m_sended = 0;
        public volatile int packetNum = 0;
        // private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public byte[] Buffer
        {
            get
            {
                return this.m_buffer;
            }
        }
        public int Length
        {
            get
            {
                return this.m_length;
            }
        }
        public int Offset
        {
            get
            {
                return this.m_offset;
            }
            set
            {
                this.m_offset = value;
            }
        }
        public int DataLeft
        {
            get
            {
                return this.m_length - this.m_offset;
            }
        }
        public PacketIn(byte[] buf, int len)
        {
            this.m_buffer = buf;
            this.m_length = len;
            this.m_offset = 0;
        }
        public void Skip(int num)
        {
            this.m_offset += num;
        }
        public virtual bool ReadBoolean()
        {
            return this.m_buffer[this.m_offset++] != 0;
        }
        public virtual byte ReadByte()
        {
            return this.m_buffer[this.m_offset++];
        }
        public virtual short ReadShort()
        {
            byte v = this.ReadByte();
            byte v2 = this.ReadByte();
            return Marshal.ConvertToInt16(v, v2);
        }
        public virtual short ReadShortLowEndian()
        {
            byte v = this.ReadByte();
            byte v2 = this.ReadByte();
            return Marshal.ConvertToInt16(v2, v);
        }
        public virtual int ReadInt()
        {
            byte v = this.ReadByte();
            byte v2 = this.ReadByte();
            byte v3 = this.ReadByte();
            byte v4 = this.ReadByte();
            return Marshal.ConvertToInt32(v, v2, v3, v4);
        }
        public virtual float ReadFloat()
        {
            byte[] v = new byte[4];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = this.ReadByte();
            }
            return BitConverter.ToSingle(v, 0);
        }
        public virtual double ReadDouble()
        {
            byte[] v = new byte[8];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = this.ReadByte();
            }
            return BitConverter.ToDouble(v, 0);
        }
        public virtual string ReadString()
        {
            short len = this.ReadShort();
            string temp = Encoding.UTF8.GetString(this.m_buffer, this.m_offset, (int)len);
            this.m_offset += (int)len;
            return temp.Replace("\0", "");
        }
        public virtual byte[] ReadBytes(int maxLen)
        {
            byte[] data = new byte[maxLen];
            Array.Copy(this.m_buffer, this.m_offset, data, 0, maxLen);
            this.m_offset += maxLen;
            return data;
        }
        public virtual byte[] ReadBytes()
        {
            return this.ReadBytes(this.m_length - this.m_offset);
        }
        public DateTime ReadDateTime()
        {
            return new DateTime((int)this.ReadShort(), (int)this.ReadByte(), (int)this.ReadByte(), (int)this.ReadByte(), (int)this.ReadByte(), (int)this.ReadByte());
        }
        public virtual int CopyTo(byte[] dst, int dstOffset, int offset)
        {
            int len = (this.m_length - offset < dst.Length - dstOffset) ? (this.m_length - offset) : (dst.Length - dstOffset);
            if (len > 0)
            {
                System.Buffer.BlockCopy(this.m_buffer, offset, dst, dstOffset, len);
            }
            return len;
        }
        public virtual int CopyTo(byte[] dst, int dstOffset, int offset, int key)
        {
            int len = (this.m_length - offset < dst.Length - dstOffset) ? (this.m_length - offset) : (dst.Length - dstOffset);
            if (len > 0)
            {
                key = (key & 16711680) >> 16;
                for (int i = 0; i < len; i++)
                {
                    dst[dstOffset + i] = (byte)((int)this.m_buffer[offset + i] ^ key);
                }
            }
            return len;
        }
        public virtual int CopyTo3(byte[] dst, int dstOffset, int offset, byte[] key, ref int packetArrangeSend)
        {
            int len = (this.m_length - offset < dst.Length - dstOffset) ? (this.m_length - offset) : (dst.Length - dstOffset);
            string str = string.Empty;
            Monitor.Enter(this);
            int result;
            try
            {
                if (len > 0)
                {
                    int indexKey = this.m_sended + dstOffset;
                    if (this.isSended)
                    {
                        this.packetNum = Interlocked.Increment(ref packetArrangeSend);
                        packetArrangeSend = this.packetNum;
                        this.m_sended = 0;
                        this.isSended = false;
                        indexKey = this.m_sended + dstOffset;
                    }
                    else
                    {
                        indexKey = 2048;
                    }
                    if (this.packetNum != packetArrangeSend)
                    {
                        result = 0;
                        return result;
                    }
                    for (int i = 0; i < len; i++)
                    {
                        int indexBuffex = offset + i;
                        while (indexKey > 2048)
                        {
                            indexKey -= 2048;
                        }
                        if (this.m_sended == 0)
                        {
                            dst[dstOffset] = (byte)(this.m_buffer[indexBuffex] ^ key[this.m_sended % 8]);
                        }
                        else
                        {
                            if (indexKey == 0)
                            {
                                // PacketIn.log.Error(string.Concat(new object[]
								// {
								// 	"IndexKey :  ",
								// 	indexKey,
								// 	"  i : ",
								// 	i,
								// 	"m_sended: ",
								// 	this.m_sended,
								// 	"  dstOffset :",
								// 	dstOffset,
								// 	"indexBuffex :",
								// 	indexBuffex
								// }));
                                result = 0;
                                return result;
                            }
                            key[this.m_sended % 8] = (byte)((int)(key[this.m_sended % 8] + dst[indexKey - 1]) ^ this.m_sended);
                            dst[dstOffset + i] = (byte)((this.m_buffer[indexBuffex] ^ key[this.m_sended % 8]) + dst[indexKey - 1]);
                        }
                        this.m_sended++;
                        indexKey++;
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
            result = len;
            return result;
        }
        public virtual int CopyFrom(byte[] src, int srcOffset, int offset, int count)
        {
            int result;
            if (count < this.m_buffer.Length && count - srcOffset < src.Length)
            {
                System.Buffer.BlockCopy(src, srcOffset, this.m_buffer, offset, count);
                result = count;
            }
            else
            {
                result = -1;
            }
            return result;
        }
        public virtual int CopyFrom(byte[] src, int srcOffset, int offset, int count, int key)
        {
            int result;
            if (count < this.m_buffer.Length && count - srcOffset < src.Length)
            {
                key = (key & 16711680) >> 16;
                for (int i = 0; i < count; i++)
                {
                    this.m_buffer[offset + i] = (byte)((int)src[srcOffset + i] ^ key);
                }
                result = count;
            }
            else
            {
                result = -1;
            }
            return result;
        }
        public virtual int[] CopyFrom3(byte[] src, int srcOffset, int offset, int count, byte[] key)
        {
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                this.m_buffer[i] = src[i];
            }
            if (count < this.m_buffer.Length && count - srcOffset < src.Length)
            {
                this.m_buffer[0] = (byte)(src[srcOffset] ^ key[0]);
                for (int i = 1; i < count; i++)
                {
                    key[i % 8] = (byte)((int)(key[i % 8] + src[srcOffset + i - 1]) ^ i);
                    this.m_buffer[i] = (byte)(src[srcOffset + i] - src[srcOffset + i - 1] ^ key[i % 8]);
                }
            }
            return result;
        }
        public virtual void WriteBoolean(bool val)
        {
            this.m_buffer[this.m_offset++] = (byte)(val ? 1 : 0);
            this.m_length = ((this.m_offset > this.m_length) ? this.m_offset : this.m_length);
        }
        public virtual void WriteByte(byte val)
        {
            this.m_buffer[this.m_offset++] = val;
            this.m_length = ((this.m_offset > this.m_length) ? this.m_offset : this.m_length);
        }
        public virtual void Write(byte[] src)
        {
            this.Write(src, 0, src.Length);
        }
        public virtual void Write(byte[] src, int offset, int len)
        {
            if ((m_offset + len) >= m_buffer.Length)
            {
                byte[] temp = m_buffer;
                m_buffer = new byte[m_buffer.Length * 2];
                Array.Copy(temp, m_buffer, temp.Length);
                Write(src, offset, len);
            }
            else
            {
                Array.Copy(src, offset, m_buffer, m_offset, len);
                m_offset += len;
                m_length = (m_offset > m_length) ? m_offset : m_length;
            }
        }

        public virtual void WriteShort(short val)
        {
            this.WriteByte((byte)(val >> 8));
            this.WriteByte((byte)(val & 255));
        }
        public virtual void WriteShortLowEndian(short val)
        {
            this.WriteByte((byte)(val & 255));
            this.WriteByte((byte)(val >> 8));
        }
        public virtual void WriteInt(int val)
        {
            this.WriteByte((byte)(val >> 24));
            this.WriteByte((byte)(val >> 16 & 255));
            this.WriteByte((byte)((val & 65535) >> 8));
            this.WriteByte((byte)(val & 65535 & 255));
        }
        public virtual void WriteFloat(float val)
        {
            byte[] src = BitConverter.GetBytes(val);
            this.Write(src);
        }
        public virtual void WriteDouble(double val)
        {
            byte[] src = BitConverter.GetBytes(val);
            this.Write(src);
        }
        public virtual void Fill(byte val, int num)
        {
            for (int i = 0; i < num; i++)
            {
                this.WriteByte(val);
            }
        }
        public virtual void WriteString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                this.WriteShort((short)(bytes.Length + 1));
                this.Write(bytes, 0, bytes.Length);
                this.WriteByte(0);
            }
            else
            {
                this.WriteShort(1);
                this.WriteByte(0);
            }
        }
        public virtual void WriteString(string str, int maxlen)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            int len = (bytes.Length < maxlen) ? bytes.Length : maxlen;
            this.WriteShort((short)len);
            this.Write(bytes, 0, len);
        }
        public void WriteDateTime(DateTime date)
        {
            this.WriteShort((short)date.Year);
            this.WriteByte((byte)date.Month);
            this.WriteByte((byte)date.Day);
            this.WriteByte((byte)date.Hour);
            this.WriteByte((byte)date.Minute);
            this.WriteByte((byte)date.Second);
        }
    }
}
