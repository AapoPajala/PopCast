
using System.Net.Sockets;
using LibVLCSharp.Shared;
using VideoView = LibVLCSharp.WinForms.VideoView;
using Player = LibVLCSharp.Shared.MediaPlayer;
using PopCast;
using System.Text;

public class Device {


    public string ipAddress, name;
    Form form;

    public Device(Socket d, Socket a, Socket c) {
        device = d;
        audioSocket = a;
        commandSocket = c;
        receive();

    }

    
    public Socket device, audioSocket, commandSocket;
    Thread receiveThread;
    
    public void receive() {
        receiveThread = new Thread(stream);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    public int width, height;
    int origW, origH;
    VideoView view;
    TransparentPanel mouse;
    string[] args;
    public void stream() {
        
        device.Blocking = true;
        device.ReceiveBufferSize = 10000 * 1024;
        /*device.DontFragment = true;
        device.NoDelay = true;*/

        device.Send(new byte[1] { 4 }, SocketFlags.None);

        commandSocket.Blocking = true;

        scale = 1f;
        
        Decrypt decrypt = new Decrypt(true);

        byte[] w = new byte[16];
        int r = 0;
        try {
            r = device.Receive(w);
        } catch(SocketException e) {
            disconnect();
            return;
        }

        if(r == 0) {
            disconnect();
            return;
        }

        try {
            origW = width = BitConverter.ToInt32(decrypt.decrypt(w), 0);
        } catch(ArgumentOutOfRangeException e) {
            try {
                device.Send(new byte[1] { 7 });
            } catch(SocketException ex) {
                disconnect();
                return;
            }
            disconnect();
            return;
        }

        byte[] h = new byte[16];
        device.Receive(h);
        origH = height = BitConverter.ToInt32(decrypt.decrypt(h), 0);

        aspectRatio = (float) width / height;

        //Console.WriteLine(width + " " + height);
        
        audio();

        
        form = new Form();

        Screen screen = Screen.FromControl(form);
        Size max = new Size(screen.Bounds.Width, screen.Bounds.Height);

        if(height > max.Height / 1.5f) {
            height = (int)(max.Height / 1.5f);
            width = (int)((float)height * aspectRatio);
        }

        form.Size = new Size(width, height);
        form.Show();
        form.MaximizeBox = false;
        lastSize = form.Size;

        form.TopLevel = true;

        Core.Initialize();

        args = new string[] {
            "--demux", "h264"
        };

        
        view = new VideoView();
        view.Bounds = new Rectangle(0, 0, width, height);
        view.Show();
        view.CreateControl();
        view.Dock = DockStyle.Fill;

        Form1.onTopChanged += onTop;

        bool started = false;

        Thread thread = new Thread(play);
        void play() {
            vlc = new LibVLC(args);
            vlc.Log += log;

            player = new Player(vlc);
            player.EnableHardwareDecoding = true;
            player.EnableMouseInput = false;
            player.FileCaching = 100;
            player.NetworkCaching = 100;
            

            videoStream = new RawStream(device, "video", this, audioStream);

            StreamMediaInput input = new StreamMediaInput(videoStream);

            Media media = new Media(vlc, input);
            media.AddOption(":no-audio");
            //media.AddOption(":avcodec-hw=none");
            player.Media = media;

            player.Play();

            started = true;
        }
        thread.Start();
        thread.IsBackground = true;

        SpinWait.SpinUntil(() => started, Timeout.InfiniteTimeSpan);

        form.ResizeEnd += resize;
        form.FormClosed += close;
        form.FormClosing += formClosing;

        view.MediaPlayer = player;

        form.Controls.Add(view);
        form.Text = name + " volume: " + audioPlayer.Volume;
        volume = audioPlayer.Volume;

        videoStream.received += skip;
        connectionLost += close;

        mouse = new TransparentPanel();
        mouse.Dock = DockStyle.Fill;
        mouse.Size = view.Size;
        mouse.MouseDoubleClick += fullscreen;
        mouse.MouseWheel += changeVolume;
        mouse.Bounds = view.Bounds;
        
        mouse.Show();
        mouse.CreateControl();
        form.Controls.Add(mouse);
        mouse.BringToFront();

        device.SendTimeout = 1000;

        audioPlayer.VolumeChanged += volumeChange;

        Thread test = new Thread(testConnection);
        test.IsBackground = true;
        test.Start();

        Thread comm = new Thread(commands);
        comm.IsBackground = true;
        comm.Start();

        initialized = true;

        Application.Run(form);
    }

    bool initialized;

    private void onTop(object? sender, EventArgs e) {
        if(form.IsHandleCreated)
            form.BeginInvoke((MethodInvoker)delegate {
                form.TopMost = Form1.onTop;
            });
        
    }

    private void volumeChange(object? sender, MediaPlayerVolumeChangedEventArgs e) {
        if(form.Created && !form.IsDisposed)
            form.BeginInvoke((MethodInvoker)delegate {
                form.Text = name + " volume: " + audioPlayer.Volume;
            });
    }

    static int volume;
    private void changeVolume(object? sender, MouseEventArgs e) {
        volume += Math.Sign(e.Delta) * 2;
        volume = Math.Min(volume, 100);
        if(volume < 0) volume = 0;
        audioPlayer.Volume = volume;
    }

    bool closing; 
    private void formClosing(object? sender, FormClosingEventArgs e) {
        closing = true;
    }

    public event EventHandler connectionLost;
    void testConnection() {
        while(!closing) {
            if (!disconnected) {
                try {
                    device.Send(new byte[1]);
                    
                }
                catch (SocketException e) {
                    if (!form.IsDisposed)
                        form.BeginInvoke((MethodInvoker)delegate {
                            disconnect();
                        });
                    break;
                }
            }
            else break;
            Thread.Sleep(1000);
        }
    }

    float scale;

    public bool hidden;

    void commands() {
        while(!closing) {
            if (!disconnected && !closing) {
                if (commandSocket.Available > 0) {
                    byte[] data = new byte[commandSocket.Available];

                    commandSocket.ReceiveAsync(data, SocketFlags.None);
                    string command = Encoding.UTF8.GetString(data);
                    //Console.WriteLine("COMMAND: " + command);

                    if(command.Contains("hide")) {
                        player.Pause();
                        audioPlayer.Pause();
                        hidden = true;
                        if(Form1.hideOnLock)
                            form.BeginInvoke((MethodInvoker)delegate {
                                form.Hide();
                            });
                        commandSocket.Send(new byte[1] { 3 });
                    }

                    if(command.Contains("show")) {
                        hidden = false;
                        player.Play();
                        audioPlayer.Play();
                        if(Form1.hideOnLock)
                            form.BeginInvoke((MethodInvoker)delegate {
                                form.Show();
                            });
                        commandSocket.Send(new byte[1] { 4 });
                    }

                    if (command.Contains("orientation changed")) {
                        form.ResizeEnd -= resize;

                        /*device.Blocking = false;
                        audioSocket.Blocking = false;*/

                        bool stopped = false;

                        Task task = Task.Run(() => {
                            player.Stop();
                            audioPlayer.Stop();
                            stopped = true;
                        });

                        SpinWait.SpinUntil(() => videoStream.exited && audioStream.exited, new TimeSpan(0, 0, 20));

                        if(!videoStream.exited) {
                            device.Blocking = false;
                            SpinWait.SpinUntil(() => videoStream.exited, new TimeSpan(0, 0, 5));
                            device.Blocking = true;
                        }
                        if(!audioStream.exited) {
                            audioSocket.Blocking = false;
                            SpinWait.SpinUntil(() => audioStream.exited, new TimeSpan(0, 0, 5));
                            audioSocket.Blocking = true;
                        }

                        videoStream.stop = true;
                        audioStream.stop = true;

                        videoStream.Flush();
                        audioStream.Flush();

                        if(player.Media != null)
                            player.Media.Dispose();
                        if(audioPlayer.Media != null)
                            audioPlayer.Media.Dispose();

                        
                        
                        
                        videoStream.received -= skip;

                        if (command.Contains("LS")) {
                            width = lastSize.Height;
                            height = lastSize.Width;
                            //Console.WriteLine("landscape");
                        }
                        else if (command.Contains("PT")) {
                            width = lastSize.Height;
                            height = lastSize.Width;
                            //Console.WriteLine("portrait");
                        }

                        
                        SpinWait.SpinUntil(() => stopped, new TimeSpan(0, 0, 2));

                        audio();
                        audioPlayer.VolumeChanged += volumeChange;

                        player = new Player(vlc);
                        videoStream = new RawStream(device, "video", this, audioStream);
                        StreamMediaInput input = new StreamMediaInput(videoStream);
                        Media media = new Media(vlc, input);
                        media.AddOption(":no-audio");
                        player.Media = media;

                        

                        form.BeginInvoke((MethodInvoker)delegate {
                            view.MediaPlayer = player;
                            form.Text = name + " volume: " + audioPlayer.Volume;
                            if(!fullscreened) {
                                form.Size = new Size((int)(width * scale), (int)(height * scale));
                                view.Size = form.Size;
                                lastSize = form.Size;
                            } else lastSize = new Size(width, height);
                            aspectRatio = (float)width / height;
                        });
                        form.ResizeEnd += resize;
                        videoStream.received += skip;

                        SpinWait.SpinUntil(() => view.MediaPlayer == player, new TimeSpan(0, 0, 5));

                        Thread thread = new Thread(play);
                        void play() {
                            player.Play();
                        }
                        thread.IsBackground = true;
                        thread.Start();

                        SpinWait.SpinUntil(() => videoStream.started, new TimeSpan(0, 0, 5));

                        SpinWait.SpinUntil(() => audioStream.started, new TimeSpan(0, 0, 5));

                        //Console.WriteLine("players restarted");

                        commandSocket.Send(new byte[1] { 12 });
                        //Console.WriteLine("response sent");
                    }
                    
                }
            }
            else break;

            Thread.Sleep(100);
        }
    }

    
    

    private void abort(object? sender, EventArgs e) {
        disconnect();
    }
    
    private void skip(object? sender, EventArgs e) {
        if (!disconnected) {

            long d = audioStream.time - videoStream.time;

            if(d > 33000) {
                player.SetRate(3);
                
            } else {
                player.SetRate(1);
            }


            if(d < -33000 && !videoStream.wait) {
                
                int delay = (int) Math.Abs(d / 1000f);
                CancellationTokenSource cts = new CancellationTokenSource(delay);
                CancellationToken token = cts.Token;
                Task.Run(async () => {
                    long last = d;
                    videoStream.wait = true;
                    await Task.Delay(delay);
                    videoStream.wait = false;
                    videoStream.time += d;
                });
            }
        }
    }

    float aspectRatio;

    private void resize(object? sender, EventArgs e) {
        int nw = (int)((float)form.Height * aspectRatio);
        form.Size = new Size(nw, form.Height);
        view.Size = form.Size;
        lastSize = form.Size;
    }

    Size lastSize;
    Point lastPoint;
    bool fullscreened;

    private void fullscreen(object? sender, EventArgs e) {
        
        if(form.FormBorderStyle != FormBorderStyle.None) {
            lastPoint = form.Location;
            form.FormBorderStyle = FormBorderStyle.None;
            Screen screen = Screen.FromControl(form);
            Size max = new Size(screen.Bounds.Width, screen.Bounds.Height);
            form.Size = max;
            form.Location = new Point(screen.WorkingArea.Left, screen.WorkingArea.Top);
            view.Size = form.Size;
            fullscreened = true;
        } else {
            form.Size = lastSize;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.Location = lastPoint;
            view.Size = form.Size;
            fullscreened = false;
        }
    }
    bool has;
    private void log(object? sender, LogEventArgs e) {
        if(!e.Message.Contains("fifo overflow") && e.Message.Contains("early picture") && !has) {
            Console.WriteLine("VIDEO: " + e.Message);
            has = true;
        }
        //Console.WriteLine("VIDEO: " + e.Message);
    }

    private void alog(object? sender, LogEventArgs e) {
        //Console.WriteLine("AUDIO: " + e.Message);
        
    }
    
    LibVLC v;
    void audio() {
        //audioSocket.NoDelay = true;
        audioSocket.Blocking = true;
        bool started = false;
        Thread thread = new Thread(play);
        void play() {
            audioStream = new RawStream(audioSocket, "audio", this);

            if(v == null) {
                v = new LibVLC();
                v.Log += alog;
            }
            audioPlayer = new Player(v);
            audioPlayer.NetworkCaching = 100;
            audioPlayer.EnableHardwareDecoding = true;

            StreamMediaInput input = new StreamMediaInput(audioStream);
            Media media = new Media(v, input);
            //media.AddOption(":avcodec-hw=none");
            media.AddOption(":no-video");
            audioPlayer.Media = media;
            audioPlayer.Volume = volume;
            audioPlayer.Play();
            started = true;
        }
        thread.IsBackground = true;
        thread.Start();

        SpinWait.SpinUntil(() => started, Timeout.InfiniteTimeSpan);

    }

    RawStream audioStream;

    RawStream videoStream;
   

    Player player;
    LibVLC vlc;

    Player audioPlayer;

    public event EventHandler deviceDisconnected;

    private void close(object? sender, EventArgs e) {
        disconnect();
    }

    public bool disconnected;

    public void disconnect() {
        if(!disconnected ) {
            disconnected = true;

            deviceDisconnected?.Invoke(this, EventArgs.Empty);

            if(initialized)
                mouse.MouseWheel -= changeVolume;

            if(videoStream != null && audioStream != null && initialized) {
                videoStream.received -= skip;

                videoStream.stop = true;
                audioStream.stop = true;

                videoStream.Flush();
                audioStream.Flush();
            }
            
            device.Disconnect(false);
            audioSocket.Disconnect(false);
            commandSocket.Disconnect(false);

            audioSocket.Close();
            audioSocket.Dispose();

            device.Close();
            device.Dispose();

            commandSocket.Close();
            commandSocket.Dispose();

            
            if (player != null) {
                Task task = new Task(() => {
                    player.Stop();
                    player.Dispose();
                    audioPlayer.Stop();
                    audioPlayer.Dispose();
                    vlc.Dispose();
                    v.Dispose();
                });
                task.Start();
            }

            

            if(form != null && !form.IsDisposed && !closing)
                form.BeginInvoke((MethodInvoker)delegate {
                    form.Hide();
                    form.Close();
                });

            
            //Console.WriteLine(name + " disconnected");
            
        }
    }

}
public class TransparentPanel : Panel {
    protected override CreateParams CreateParams {            
        get {
            CreateParams cp =  base.CreateParams;
            cp.ExStyle |= 0x00000020;
            return cp;
            }
    }
    protected override void OnPaintBackground(PaintEventArgs e) {
        //base.OnPaintBackground(e);
    }
}