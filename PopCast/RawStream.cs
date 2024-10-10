
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace PopCast {
    public class RawStream : Stream {
        Socket socket;
        string name;
        Device parent;
        Decrypt decrypt;

        public RawStream(Socket s, string n, Device parent) {
            socket = s;
            latest = 10 * 1024;
            name = n;
            
            
            this.parent = parent;
            dataBuffer = new BlockingCollection<DataPacket>(new ConcurrentQueue<DataPacket>());

            Thread rThread = new Thread(receiveThread);
            rThread.IsBackground = true;
            rThread.Start();

            decrypt = new Decrypt(false);
        }

        public RawStream(Socket s, string n, Device parent, RawStream sTo) {
            socket = s;
            latest = 10 * 1024;
            name = n;
            syncTo = sTo;

            /*Thread thread = new Thread(sync);
            thread.IsBackground = true;
            thread.Start();*/

            this.parent = parent;
            dataBuffer = new BlockingCollection<DataPacket>(new ConcurrentQueue<DataPacket>());

            Thread rThread = new Thread(receiveThread);
            rThread.IsBackground = true;
            rThread.Start();
            
            decrypt = new Decrypt(false);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length {
            //get => stop ? 0 : len;
            get => 5000 * 1024;
        }

        int latest;
        public long time, first;
        public override long Position {
            get => stop ? 0 : len;
            set => throw new NotImplementedException();
        }

        public override void Flush() {
            try {
                dataBuffer.CompleteAdding();
                dataBuffer.Dispose();

                if(src != null) src.Cancel();

                decrypt.stop();

            } catch(ThreadInterruptedException e) {

            } catch(ObjectDisposedException e) {

            }
        }

        int len;
        public bool timeSet;
        public bool wait;
        long current, last;
        public RawStream syncTo;
        public bool stop;

        
        public bool started, exited;
        BlockingCollection<DataPacket> dataBuffer;

        public bool silent() {
            return latest <= 32;
        }

        void receiveThread() {
            exited = false;
            
            int f = 0;
            started = true;
            while(!stop) {
                try {
                    start:
                    byte[] size = new byte[4];

                    f++;
                   
                    socket.Receive(size);


                    int dataSize = 0;
                    dataSize = BitConverter.ToInt32(size);

                    if(dataSize == -1) {
                        exited = true;
                        break;
                    }

                    //Console.WriteLine(name + " SIZE: " + dataSize);

                    latest = dataSize;

                    bool discard = false;


                    if(dataSize < 0 || dataSize > 10000 * 1024) {
                        dataSize = socket.Available;
                        discard = true;
                    }

                    byte[] data = new byte[dataSize];
                    int read = 0;
                    int i = 0;

                    while (read < dataSize) {
                        int left = dataSize - read;
                        int received = socket.Receive(data, read, left, SocketFlags.None);
                        read += received;
                        i++;
                    }

                    //Console.WriteLine(name + " READ: " + read);

                    if(dataSize > 0) {

                        byte[] decrypted = decrypt.decrypt(data);


                        if(decrypted != null && decrypted.Length > 0) {
                            dataBuffer.Add(new DataPacket(decrypted, dataSize));
                        }

                        decrypted = null;
                        data = null;
                    }
                    
                } catch (SocketException e) {
                    stop = true;
                    break;
                } catch (ObjectDisposedException e) {
                    stop = true;
                    break;
                }
            }
            exited = true;
        }
        DataPacket packet;
        CancellationTokenSource src;
        long lastTime;
        public override int Read(byte[] buffer, int offset, int count) {
        start:
            
            try {

                if (!stop) {

                    SpinWait.SpinUntil(() => started, new TimeSpan(0, 0, 5));

                    SpinWait.SpinUntil(() => !wait, Timeout.InfiniteTimeSpan);

                    if (stop || dataBuffer.IsAddingCompleted) return 0;

                    if (packet == null || packet.finished) {
                        src = new CancellationTokenSource(10000);
                        CancellationToken token = src.Token;

                        try {
                            if(!dataBuffer.IsAddingCompleted)
                                packet = dataBuffer.Take(token);
                        }
                        catch (OperationCanceledException e) {
                            //Console.WriteLine("packet operation canceled");
                            goto start;
                        }
                    }

                    if (packet.offset == 0) {
                        byte[] ts = new byte[8];
                        packet.read(ts, 8);
                        long timestamp = BitConverter.ToInt64(ts);

                        if (!timeSet && timestamp > 0) {
                            lastTime = first = timestamp;
                            timeSet = true;   
                        }
                        if(timeSet) {
                            long l = timestamp - lastTime;
                            time += l;
                            lastTime = timestamp;
                        }
                    }

                    //Console.WriteLine(name + "TIME: " + time);

                    int r = packet.read(buffer, count);

                    if (packet.finished) packet = null;

                    //dataBuffer.Remove(packet);

                    len += r;

                    received?.Invoke(this, EventArgs.Empty);

                    
                    return r;
                }
                else if (!stop) goto start;
                else return 0;
            } catch (SocketException ex) {
                return 0;
            } catch(OverflowException e) {
                //Console.WriteLine(e.Message);
                goto start;
            } catch (ObjectDisposedException e) {
                //Console.WriteLine(e.Message);
                return 0;
            }
        }

        public event EventHandler received;

        public override long Seek(long offset, SeekOrigin origin) {
            return len;
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            
        }

    }
}
class DataPacket {

    public byte[] data;
    public int size, offset;
    public bool finished;

    public DataPacket() {
        
    }

    public int read(byte[] buffer, int length) {

        if(length + offset > data.Length) {
            length -=  (length + offset) - data.Length;
        }

        Array.ConstrainedCopy(data, offset, buffer, 0, length);

        offset += length;

        if(offset >= data.Length) finished = true;

        return length;
    }

    public DataPacket(byte[] d, int s) {
        data = d;
        size = s;
    }
}
