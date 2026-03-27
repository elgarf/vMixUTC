using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace vMixStreamDeckLibrary
{
    // Token: 0x02000014 RID: 20
    public class WebSocket : IDisposable
    {
        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000045 RID: 69 RVA: 0x0000323C File Offset: 0x0000143C
        // (set) Token: 0x06000046 RID: 70 RVA: 0x00003250 File Offset: 0x00001450
        public virtual SocketAsyncEventArgs m_ReceiveArgs
        {
            get
            {
                return this._m_ReceiveArgs;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                EventHandler<SocketAsyncEventArgs> value2 = new EventHandler<SocketAsyncEventArgs>(this.m_ReceiveArgs_Completed);
                if (this._m_ReceiveArgs != null)
                {
                    this._m_ReceiveArgs.Completed -= value2;
                }
                this._m_ReceiveArgs = value;
                if (this._m_ReceiveArgs != null)
                {
                    this._m_ReceiveArgs.Completed += value2;
                }
            }
        }

        // Token: 0x14000003 RID: 3
        // (add) Token: 0x06000047 RID: 71 RVA: 0x000021E5 File Offset: 0x000003E5
        // (remove) Token: 0x06000048 RID: 72 RVA: 0x000021FE File Offset: 0x000003FE
        public event WebSocket.TextReceivedEventHandler TextReceived;

        // Token: 0x14000004 RID: 4
        // (add) Token: 0x06000049 RID: 73 RVA: 0x00002217 File Offset: 0x00000417
        // (remove) Token: 0x0600004A RID: 74 RVA: 0x00002230 File Offset: 0x00000430
        public event WebSocket.SocketReadyEventHandler SocketReady;

        // Token: 0x14000005 RID: 5
        // (add) Token: 0x0600004B RID: 75 RVA: 0x00002249 File Offset: 0x00000449
        // (remove) Token: 0x0600004C RID: 76 RVA: 0x00002262 File Offset: 0x00000462
        public event WebSocket.SocketClosedEventHandler SocketClosed;

        // Token: 0x0600004D RID: 77 RVA: 0x0000329C File Offset: 0x0000149C
        protected void OnTextReceived(string msg)
        {
            WebSocketEventArgs e = new WebSocketEventArgs(msg);
            //WebSocket.TextReceivedEventHandler textReceivedEvent = this.TextReceivedEvent;
            this.TextReceived?.Invoke(this, e);
        }

        // Token: 0x0600004E RID: 78 RVA: 0x000032C4 File Offset: 0x000014C4
        protected void OnSocketReady()
        {
            //WebSocket.SocketReadyEventHandler socketReadyEvent = this.SocketReadyEvent;
            this.SocketReady?.Invoke(this, EventArgs.Empty);
        }

        // Token: 0x0600004F RID: 79 RVA: 0x000032E8 File Offset: 0x000014E8
        protected void OnSocketClosed()
        {
            //WebSocket.SocketClosedEventHandler socketClosedEvent = this.SocketClosedEvent;
            SocketClosed?.Invoke(this, EventArgs.Empty);
        }

        // Token: 0x06000050 RID: 80 RVA: 0x0000330C File Offset: 0x0000150C
        public WebSocket(Uri url, string protocol, Uri origin)
        {
            this.m_Buffer = new byte[8192];
            this.m_FrameMode = false;
            this.m_URL = url;
            this.m_Client = new TcpClient(url.Host, url.Port);
            this.m_Client.SendBufferSize = 65536;
            this.m_ReceiveStream = new MemoryStream();
            this.m_HttpReceiveStream = new StringBuilder();
            this.BeginReceive();
            this.m_Random = new Random();
            byte[] array = new byte[16];
            this.m_Random.NextBytes(array);
            this.m_Key = Convert.ToBase64String(array);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("GET / HTTP/1.1");
            stringBuilder.AppendLine("Host: " + this.m_URL.Host);
            stringBuilder.AppendLine("Upgrade: websocket");
            stringBuilder.AppendLine("Connection: Upgrade");
            stringBuilder.AppendLine("Sec-WebSocket-Key: " + this.m_Key);
            if (!string.IsNullOrEmpty(protocol))
            {
                stringBuilder.AppendLine("Sec-WebSocket-Protocol: " + protocol);
            }
            stringBuilder.AppendLine("Sec-WebSocket-Version: 13");
            if (origin != null)
            {
                stringBuilder.AppendLine("Origin: " + origin.ToString());
            }
            stringBuilder.AppendLine();
            this.SendMessage(stringBuilder.ToString());
        }

        // Token: 0x06000051 RID: 81 RVA: 0x0000345C File Offset: 0x0000165C
        private void BeginReceive()
        {
            if (this.m_ReceiveArgs == null)
            {
                this.m_ReceiveArgs = new SocketAsyncEventArgs();
            }
            this.m_ReceiveArgs.SetBuffer(this.m_Buffer, 0, this.m_Buffer.Length);
            this.m_Client.Client.ReceiveAsync(this.m_ReceiveArgs);
        }

        // Token: 0x06000052 RID: 82 RVA: 0x000034B0 File Offset: 0x000016B0
        private void SendMessage(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            this.SendData(bytes);
        }

        // Token: 0x06000053 RID: 83 RVA: 0x000034D0 File Offset: 0x000016D0
        private void SendData(byte[] b)
        {
            try
            {
                this.m_Client.Client.Send(b);
            }
            catch (SocketException ex)
            {
                this.OnSocketClosed();
            }
        }

        // Token: 0x06000054 RID: 84 RVA: 0x00003518 File Offset: 0x00001718
        private void m_ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred == 0)
                {
                    this.OnSocketClosed();
                }
                else
                {
                    if (this.m_FrameMode)
                    {
                        this.m_ReceiveStream.Write(e.Buffer, 0, e.BytesTransferred);
                        this.ProcessFrames();
                    }
                    else
                    {
                        this.m_HttpReceiveStream.Append(Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred));
                        this.ProcessHttp();
                    }
                    if (this.m_Client != null)
                    {
                        this.BeginReceive();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        // Token: 0x06000055 RID: 85 RVA: 0x000035B8 File Offset: 0x000017B8
        private void ProcessHttp()
        {
            string text = this.m_HttpReceiveStream.ToString();
            if (text.Contains("\r\n\r\n") && text.StartsWith("HTTP/1.1 101"))
            {
                this.m_HttpReceiveStream = new StringBuilder();
                this.m_FrameMode = true;
                this.OnSocketReady();
            }
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00003604 File Offset: 0x00001804
        private void ProcessFrames()
        {
            long position = this.m_ReceiveStream.Position;
            this.m_ReceiveStream.Position = this.m_ReadPos;
            checked
            {
                try
                {
                    for (; ; )
                    {
                        if (this.m_PendingData > 0)
                        {
                            long num = this.m_ReceiveStream.Length - this.m_ReceiveStream.Position;
                            if (num < unchecked((long)this.m_PendingData))
                            {
                                break;
                            }
                            byte[] array = new byte[this.m_PendingData - 1 + 1];
                            this.m_ReceiveStream.Read(array, 0, array.Length);
                            string @string = Encoding.UTF8.GetString(array);
                            this.OnTextReceived(@string);
                            this.m_PendingData = 0;
                        }
                        else
                        {
                            int num2 = this.m_ReceiveStream.ReadByte();
                            if (num2 < 0)
                            {
                                break;
                            }
                            if ((num2 & 15) == 1 && (num2 & 128) == 128)
                            {
                                int pendingData = this.m_ReceiveStream.ReadByte();
                                switch (pendingData)
                                {
                                    case 126:
                                        {
                                            byte[] array2 = new byte[2];
                                            this.m_ReceiveStream.Read(array2, 0, 2);
                                            Array.Reverse(array2);
                                            this.m_PendingData = (int)BitConverter.ToUInt16(array2, 0);
                                            break;
                                        }
                                    case 127:
                                        {
                                            byte[] array3 = new byte[64];
                                            this.m_ReceiveStream.Read(array3, 0, 8);
                                            Array.Reverse(array3);
                                            this.m_PendingData = (int)BitConverter.ToUInt64(array3, 0);
                                            break;
                                        }
                                    default:
                                        this.m_PendingData = pendingData;
                                        break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.m_ReadPos = this.m_ReceiveStream.Position;
                    if (this.m_ReadPos == this.m_ReceiveStream.Length)
                    {
                        position = 0L;
                        this.m_ReadPos = 0L;
                        this.m_ReceiveStream.SetLength(0L);
                    }
                    this.m_ReceiveStream.Position = position;
                }
            }
        }

        // Token: 0x06000057 RID: 87 RVA: 0x000037C4 File Offset: 0x000019C4
        public void SendTextFrame(string message)
        {
            checked
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(message);
                        byte value = 129;
                        binaryWriter.Write(value);
                        if (bytes.Length > 125)
                        {
                            byte maxValue = byte.MaxValue;
                            binaryWriter.Write(maxValue);
                            ulong value2 = (ulong)bytes.Length;
                            byte[] bytes2 = BitConverter.GetBytes(value2);
                            Array.Reverse(bytes2);
                            binaryWriter.Write(bytes2);
                        }
                        else
                        {
                            byte value3 = (byte)(bytes.Length | 128);
                            binaryWriter.Write(value3);
                        }
                        byte[] array = new byte[4];
                        this.m_Random.NextBytes(array);
                        binaryWriter.Write(array);
                        int num = 0;
                        int num2 = bytes.Length - 1;
                        for (int i = num; i <= num2; i++)
                        {
                            int num3 = i % 4;
                            bytes[i] ^= array[num3];
                        }
                        binaryWriter.Write(bytes);
                        byte[] b = memoryStream.ToArray();
                        this.SendData(b);
                    }
                }
            }
        }

        // Token: 0x06000058 RID: 88 RVA: 0x000038DC File Offset: 0x00001ADC
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (this.m_Client != null)
                {
                    this.m_Client.Close();
                    this.m_Client = null;
                }
                if (this.m_ReceiveArgs != null)
                {
                    this.m_ReceiveArgs.Dispose();
                    this.m_ReceiveArgs = null;
                }
            }
            this.disposedValue = true;
        }

        // Token: 0x06000059 RID: 89 RVA: 0x0000227B File Offset: 0x0000047B


        // Token: 0x0600005A RID: 90 RVA: 0x0000228A File Offset: 0x0000048A
        public void Dispose()
        {
            this.Dispose(true);
        }

        // Token: 0x04000031 RID: 49
        private TcpClient m_Client;

        // Token: 0x04000032 RID: 50
        private Uri m_URL;

        // Token: 0x04000033 RID: 51
        private string m_Key;

        // Token: 0x04000034 RID: 52
        private byte[] m_Buffer;

        // Token: 0x04000035 RID: 53
        [AccessedThroughProperty("m_ReceiveArgs")]
        private SocketAsyncEventArgs _m_ReceiveArgs;

        // Token: 0x04000036 RID: 54
        private bool m_FrameMode;

        // Token: 0x04000037 RID: 55
        private Random m_Random;

        // Token: 0x04000038 RID: 56
        private MemoryStream m_ReceiveStream;

        // Token: 0x04000039 RID: 57
        private StringBuilder m_HttpReceiveStream;

        // Token: 0x0400003A RID: 58
        private long m_ReadPos;

        // Token: 0x0400003B RID: 59
        private int m_PendingData;

        // Token: 0x0400003F RID: 63
        private bool disposedValue;

        // Token: 0x02000015 RID: 21
        // (Invoke) Token: 0x0600005E RID: 94
        public delegate void TextReceivedEventHandler(object sender, WebSocketEventArgs e);

        // Token: 0x02000016 RID: 22
        // (Invoke) Token: 0x06000062 RID: 98
        public delegate void SocketReadyEventHandler(object sender, EventArgs e);

        // Token: 0x02000017 RID: 23
        // (Invoke) Token: 0x06000066 RID: 102
        public delegate void SocketClosedEventHandler(object sender, EventArgs e);
    }
}
