using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Collections;
using Game.Base;
using System.Timers;
using System.Net.Sockets;
using System.Threading;
//using Game.Base.Events;
using Game.Base.Packets;
using Road.Base.Packets;

namespace Game.Base.Packets
{
    /// <summary>
    /// This class handles the packets, receiving and sending
    /// </summary>
    public class StreamProcessor
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Holds the current client for this processor
        /// </summary>
        protected readonly BaseClient m_client;

        private FSM send_fsm;

        private FSM receive_fsm;

        private SocketAsyncEventArgs send_event;

        /// <summary>
        /// Constructs a new PacketProcessor
        /// </summary>
        /// <param name="client">The processor client</param>
        public StreamProcessor(BaseClient client)
        {
            m_client = client;
            m_client.resetKey();

            m_tcpSendBuffer = client.SendBuffer;
            m_tcpQueue = new Queue(256);

            send_event = new SocketAsyncEventArgs();
            send_event.UserToken = this;
            send_event.Completed += AsyncTcpSendCallback;
            send_event.SetBuffer(m_tcpSendBuffer, 0, 0);

            send_fsm = new FSM(0x7abcdef7, 1501,"send_fsm");
            receive_fsm = new FSM(0x7abcdef7, 1501,"receive_fsm");
        }

        public void SetFsm(int adder, int muliter)
        {
            send_fsm.Setup(adder, muliter);
            receive_fsm.Setup(adder, muliter);
        }


        #region Send TCP Package

        /// <summary>
        /// Holds the TCP send buffer
        /// </summary>
        protected byte[] m_tcpSendBuffer;

        /// <summary>
        /// The client TCP packet send queue
        /// </summary>
        protected Queue m_tcpQueue;

        /// <summary>
        /// Indicates whether data is currently being sent to the client
        /// </summary>
        protected bool m_sendingTcp;

        /// <summary>
        /// 
        /// </summary>
        protected int m_firstPkgOffset = 0;


        protected int m_sendBufferLength = 0;

        /// <summary>
        /// Sends a packet via TCP
        /// </summary>
        /// <param name="packet">The packet to be sent</param>
        /// 

       
       public static byte[] KEY = { 174, 191, 86, 120, 171, 205, 239, 241 };
        public void SendTCP(GSPacketIn packet)
        {

            //Fix the packet size
            packet.WriteHeader();

            //reset the offset for read
            packet.Offset = 0;

            //Check if client is connected
            if (m_client.Socket.Connected)
            {
                try
                {
                    Statistics.BytesOut += packet.Length;
                    Statistics.PacketsOut++;

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(Marshal.ToHexDump(string.Format("Send Pkg to {0} :", m_client.TcpEndpoint), packet.Buffer, 0, packet.Length));
                    }

                    lock (m_tcpQueue.SyncRoot)
                    {
                        m_tcpQueue.Enqueue(packet);
                        if (m_sendingTcp)
                        {
                            return;
                        }
                        else
                        {
                            m_sendingTcp = true;
                        }
                    }

                    if (m_client.AsyncPostSend)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncSendTcpImp), this);
                    }
                    else
                    {
                        AsyncTcpSendCallback(this, send_event);
                    }
                }
                catch (Exception e)
                {
                    log.Error("SendTCP", e);
                    log.WarnFormat("It seems <{0}> went linkdead. Closing connection. (SendTCP, {1}: {2})", m_client, e.GetType(), e.Message);
                    m_client.Disconnect();
                }
            }
        }

        private static void AsyncSendTcpImp(object state)
        {
            StreamProcessor proc = state as StreamProcessor;
            BaseClient client = proc.m_client;
            try
            {
                AsyncTcpSendCallback(proc, proc.send_event);
            }
            catch (Exception ex)
            {
                log.Error("AsyncSendTcpImp", ex);
                client.Disconnect();
            }
        }


        /// <summary>
        /// Callback method for async sends
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncTcpSendCallback(object sender, SocketAsyncEventArgs e)
        {
            StreamProcessor proc = (StreamProcessor)e.UserToken;
            BaseClient client = proc.m_client;
            try
            {
                Queue q = proc.m_tcpQueue;
                if (q == null || !client.Socket.Connected)
                    return;
                int sent = e.BytesTransferred;
                byte[] data = proc.m_tcpSendBuffer;
                int count = 0;
                if (sent != e.Count)
                {
                    //log.Error("Count:" + e.Count + ",sent:" + sent + ",offset:" + e.Offset + ",m_sendBufferLength:" + proc.m_sendBufferLength + ",client:" + client.TcpEndpoint);
                    if (proc.m_sendBufferLength > sent)
                    {
                        count = proc.m_sendBufferLength - sent;
                        Array.Copy(data, sent, data, 0, count);
                    }
                }
                e.SetBuffer(0, 0);

                int firstOffset = proc.m_firstPkgOffset;
                lock (q.SyncRoot)
                {
                    if (q.Count > 0)
                    {
                        do
                        {
                            PacketIn pak = (PacketIn)q.Peek();
                            
                            int len = 0;
                            if (client.Encryted)
                            {
                                int key = proc.send_fsm.getState();
                                //len = pak.CopyTo(data, count, firstOffset,key);
                                
                                len = pak.CopyTo3(data, count, firstOffset, client.SEND_KEY,ref client.numPacketProcces);
                               
                                //if (pak.m_sended == 0)
                                //{
                                //    log.Info("KeySendKey" + PrintArray(client.SEND_KEY));
                                //    log.Info("Packet" + PrintArray(data, count, 8));
                                //    log.Info("");
                                //}



                            }
                            else
                            {
                                len = pak.CopyTo(data, count, firstOffset);
                            }

                            firstOffset += len;
                            count += len;

                            if (pak.Length <= firstOffset)
                            {
                                q.Dequeue();
                                firstOffset = 0;
                                if (client.Encryted)
                                {
                                    proc.send_fsm.UpdateState();
                                   // log.Info("Update KEy");
                                    //client.numPacketProcces+=1;
                                    pak.isSended = true;
                                }
                            }
                            if (data.Length == count)
                            {
                                //pak.isSended = true;
                                break;
                            }
                        } while (q.Count > 0);

                    }
                    proc.m_firstPkgOffset = firstOffset;
                    if (count <= 0)
                    {
                        proc.m_sendingTcp = false;
                        return;
                    }
                }

                proc.m_sendBufferLength = count;
                e.SetBuffer(0, count);
                if (client.SendAsync(e) == false)
                {
                    AsyncTcpSendCallback(sender, e);
                }
            }
            catch (Exception ex)
            {
                log.Error("AsyncTcpSendCallback", ex);
                log.WarnFormat("It seems <{0}> went linkdead. Closing connection. (SendTCP, {1}: {2})", client, ex.GetType(), ex.Message);
                client.Disconnect();
            }
        }


        #endregion

        #region Receive Package
        /// <summary>
        /// Called when the client receives bytes
        /// </summary>
        /// <param name="numBytes">The number of bytes received</param>
        public void ReceiveBytes(int numBytes)
        {
            // UnityEngine.Debug.Log("Nhan duoc pkg");
            lock (this)
            {
                byte[] buffer = m_client.PacketBuf;
                int bufferSize = m_client.PacketBufSize + numBytes;
                if (bufferSize < 20)
                {
                    // UnityEngine.Debug.Log("bufferSize < 20");
                    m_client.PacketBufSize = bufferSize;
                }
                else
                {
                    // UnityEngine.Debug.Log("bufferSize >= 20");
                    m_client.PacketBufSize = 0;
                    int curOffset = 0;
                    do
                    {
                        int packetLength = 0;
                        int header3 = 0;
                        if (m_client.Encryted)
                        {
                            // UnityEngine.Debug.Log("m_client.Encryted");
                            int i = receive_fsm.count;
                            byte[] tempKey = cloneArray(m_client.RECEIVE_KEY);
                            for (; curOffset + 4 < bufferSize; curOffset++)
                            {
                                byte[] tempBuffer = decryptBytes(buffer, curOffset, 8, tempKey);
                                header3 = (tempBuffer[0] << 8) + tempBuffer[1];
                                if (header3 == 29099)
                                {
                                    packetLength = (tempBuffer[2] << 8) + tempBuffer[3];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // UnityEngine.Debug.Log("NOT m_client.Encryted");
                            for (; curOffset + 4 < bufferSize; curOffset++)
                            {
                                header3 = (buffer[curOffset] << 8) + buffer[curOffset + 1];
                                if (header3 == 29099)
                                {
                                    packetLength = (buffer[curOffset + 2] << 8) + buffer[curOffset + 3];
                                    break;
                                }
                            }
                        }
                        if ((packetLength != 0 && packetLength < 20) || packetLength > 8192)
                        {
                            // UnityEngine.Debug.Log("[Err] packetLength:" + packetLength + ",GSPacketIn.HDR_SIZE:" + (ushort)20 + ",offset:" + curOffset + ",bufferSize:" + bufferSize + ",numBytes:" + numBytes);
                            // UnityEngine.Debug.Log("[Err] pkg from "+ m_client.TcpEndpoint);
                            m_client.PacketBufSize = 0;
                            if (m_client.Strict)
                            {
                                m_client.Disconnect();
                            }
                            return;
                        }
                        int dataLeft = bufferSize - curOffset;
                        if (dataLeft < packetLength || packetLength == 0)
                        {
                            // UnityEngine.Debug.Log("dataLeft < packetLength || packetLength == 0");
                            Array.Copy(buffer, curOffset, buffer, 0, dataLeft);
                            m_client.PacketBufSize = dataLeft;
                            break;
                        }
                        GSPacketIn pkg = new GSPacketIn(new byte[8192], 8192);
                        if (m_client.Encryted)
                        {
                            pkg.CopyFrom3(buffer, curOffset, 0, packetLength, m_client.RECEIVE_KEY);
                        }
                        else
                        {
                            pkg.CopyFrom(buffer, curOffset, 0, packetLength);
                        }
                        pkg.ReadHeader();
                        // UnityEngine.Debug.Log(Marshal.ToHexDump("Recieve Packet:", pkg.Buffer, 0, packetLength));
                        try
                        {
                            m_client.OnRecvPacket(pkg);
                        }
                        catch (Exception e)
                        {
                            if (log.IsErrorEnabled)
                            {
                                UnityEngine.Debug.Log("HandlePacket(pak)"+ e);
                            }
                        }
                        curOffset += packetLength;
                    }
                    while (bufferSize - 1 > curOffset);
                    if (bufferSize - 1 == curOffset)
                    {
                        buffer[0] = buffer[curOffset];
                        m_client.PacketBufSize = 1;
                    }
                }
            }
        }

        #endregion
        public static byte[] cloneArray(byte[] arr, int length = 8)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)arr[i];
            }

            return result;
        }
        public static string PrintArray(byte[] arr, int length = 8)
        {
            byte[] result = new byte[length];
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("[");
            for (int i = 0; i < length; i++)
            {
                strBuilder.AppendFormat("{0} ",

                    (byte)arr[i]);
            }
            strBuilder.Append("]");
            return strBuilder.ToString();
        }
        public static string PrintArray(byte[] arr,int first, int length)
        {
            byte[] result = new byte[length];
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("[");
            for (int i = first; i < first+length; i++)
            {
                strBuilder.AppendFormat("{0} ",

                    (byte)arr[i]);
            }
            strBuilder.Append("]");
            return strBuilder.ToString();
        }
        // public function decrptBytes(param1:ByteArray, param2:int, param3:ByteArray) : ByteArray
        //{
        //    var _loc_4:int = 0;
        //    var _loc_5:* = new ByteArray();
        //    _loc_4 = 0;
        //    while (_loc_4 < param2)
        //    {
                
        //        _loc_5.writeByte(param1[_loc_4]);
        //        _loc_4 = _loc_4 + 1;
        //    }
        //    _loc_4 = 0;
        //    while (_loc_4 < param2)
        //    {
                
        //        if (_loc_4 > 0)
        //        {
        //            param3[_loc_4 % 8] = param3[_loc_4 % 8] + param1[(_loc_4 - 1)] ^ _loc_4;
        //            _loc_5[_loc_4] = param1[_loc_4] - param1[(_loc_4 - 1)] ^ param3[_loc_4 % 8];
        //        }
        //        else
        //        {
        //            _loc_5[0] = param1[0] ^ param3[0];
        //        }
        //        _loc_4 = _loc_4 + 1;
        //    }
        //    return _loc_5;
        //}// end function
        public static byte[] decryptBytes(byte[] param1,int curOffset, int param2, byte[] param3)
        {

            byte[] result = new byte[param2];
            for (int i = 0; i < param2; i++)
            {
                result[i] = param1[i];
            }
            for (int i = 0; i < param2; i++)
            {
                if (i> 0)
                {
                    param3[i % 8] = (byte)(param3[i % 8] + param1[(curOffset+i - 1)] ^ i);
                    result[i] = (byte)(param1[curOffset+i] - param1[(curOffset + i - 1)] ^ param3[i % 8]);
                }
                else
                {
                    result[0] = (byte)(param1[curOffset] ^ param3[0]);
                }
            }
            return result;

        }
        public void Dispose()
        {
            send_event.Dispose();
            m_tcpQueue.Clear();
        }
    }
}

