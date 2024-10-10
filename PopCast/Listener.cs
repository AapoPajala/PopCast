using PopCast;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Listener {

    private NotifyIcon icon;
    public static int port = 6380;
    public static Dictionary<String, Device> devices;
    ContextMenuStrip menu;
    public Listener() {
        icon = new NotifyIcon();
        icon.Icon = new Icon(@".\popcast_icon.ico");

        icon.Text = "PopCast client";
        menu = new ContextMenuStrip();

        local = localAddress();

        menu.Items.Add(new ToolStripMenuItem("Open", null, open));
        menu.Items.Add(new ToolStripMenuItem("Start service", null, startService));
        menu.Items.Add(new ToolStripMenuItem("Stop service", null, stopService));
        menu.Items.Add(new ToolStripMenuItem("Disconnect all devices", null, disconnectAllDevices));
        menu.Items.Add(new ToolStripMenuItem("Exit", null, exitService));

        icon.ContextMenuStrip = menu;
        icon.Visible = true;

        pending = new List<string>();

        devices = new Dictionary<string, Device>();

        Form1.shutdown += shutdown;

        Thread taskThread = new Thread(disposeTasks);
        taskThread.Start();

        deviceAdded += deviceConnected;

        Thread updateThread = new Thread(update);
        updateThread.Start();
    }

    void update() {
        List<Device> trash = new List<Device>();
        List<string> trash2 = new List<string>();
        while(true) {

            foreach(Device d in devices.Values) {
                if(d.disconnected) {
                    trash.Add(d);

                    foreach(string s in pending) {
                        if(s.Contains(d.name)) trash2.Add(s);
                    }
                }

                
            }

            foreach(Device d in trash) {
                if(devices.ContainsKey(d.name)) {
                    devices.Remove(d.name);
                    deviceRemoved.Invoke(d, EventArgs.Empty);
                }

                foreach(string s in trash2) {
                    if(pending.Contains(s)) {
                        pending.Remove(s);
                    }
                }
            }
            trash.Clear();
            trash2.Clear();
            

            Thread.Sleep(100);
        }
    }

    private void deviceConnected(object? sender, EventArgs e) {
        foreach(Device device in devices.Values) {
            device.deviceDisconnected += trash;
        }
    }

    public event EventHandler deviceAdded, deviceRemoved, exiting;
    private void open(object? sender, EventArgs e) {
        openForm?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler openForm;
    
    void disposeTasks() {
        List<Task[]> trash = new List<Task[]>();

        while(true) {

            lock(tasks) {
                foreach(Task[] task in tasks) {

                    if(task[1].IsCompletedSuccessfully || task[1].IsCanceled) {
                        task[0].Dispose();
                        task[1].Dispose();
                        trash.Add(task);
                    }
                }
            }

            foreach(Task[] t in trash) {
                tasks.Remove(t);
            }

            trash.Clear();

            Thread.Sleep(100);
        }
    }

    private void shutdown(object? sender, EventArgs e) {
        exit();
    }

    
    
    UdpClient client;
    bool run;
    TcpListener listener;
    List<string> pending;
    public void listen() {
        if(!run) {
            run = true;

            client = new UdpClient();
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            client.EnableBroadcast = true;

            Console.WriteLine("local address: " + local);
            var from = new IPEndPoint(0, 0);
            IPEndPoint to = new IPEndPoint(IPAddress.Broadcast, port);


            Thread thread = new Thread(receive);
            thread.Start();

            void receive() {
                listener = new TcpListener(Listener.local, port);
                listener.Start();
                string send = Environment.MachineName;
                byte[] message = Encoding.ASCII.GetBytes(send);

                while(run) {
                    try {
                        
                        client.Send(message, message.Length, to);
                        
                        Task<UdpReceiveResult> result = client.ReceiveAsync();
                        bool success = result.Wait(1000);

                        if(success) {
                            byte[] recvBuffer = result.Result.Buffer;

                            string msg = Encoding.UTF8.GetString(recvBuffer);
                            
                            if(!msg.Equals(send) && !pending.Contains(msg)) {
                                addTask(msg);
                                //Console.WriteLine(msg);
                            }
                        }
                        Thread.Sleep(500);
                    } catch(SocketException e) {
                        run = false;
                    }
                }
            }

        }
    }

    void addTask(string msg) {
        Task task = new Task(async() => {

            if(!run) return;
                
                pending.Add(msg);

                
                foreach(Device d in devices.Values) {
                    if(msg.Contains(d.name)) return;
                }
                
                Socket client, audio, command;

                lock(listener)
                if(msg.Contains("PopCast")) {

                    try {

                        CancellationTokenSource cts = new CancellationTokenSource();
                        CancellationToken t = cts.Token;

                        Task<Socket> s1 = listener.AcceptSocketAsync();

                        SpinWait.SpinUntil(() => s1.Result != null, new TimeSpan(0, 0, 10));

                        client = s1.Result;

                        //Console.WriteLine("socket 1");

                        Task<Socket> s2 = listener.AcceptSocketAsync();

                        SpinWait.SpinUntil(() => s2 != null, new TimeSpan(0, 0, 10));

                        audio = s2.Result;

                        //Console.WriteLine("socket 2");

                        Task<Socket> s3 = listener.AcceptSocketAsync();

                        SpinWait.SpinUntil(() => s3.Result != null, new TimeSpan(0, 0, 10));

                        command = s3.Result;

                        //Console.WriteLine("socket 3");

                    } catch(SocketException e) {
                        return;
                    } catch(AggregateException e) {
                        return;
                    } catch(InvalidOperationException e) {
                        return;
                    }

                    if(command == null || audio == null || client == null) {
                        if(client != null)
                            client.Close();
                        if(audio != null)
                            audio.Close();
                        if(command != null)
                            command.Close();
                        return;
                    }

                    string ip = (client.RemoteEndPoint as IPEndPoint).Address.ToString();

                    if(!msg.Contains(ip))
                        foreach(string s in pending)
                            if(s.Contains(ip)) msg = s;

                    Device device = new Device(client, audio, command);
                    device.name = msg.Replace("PopCast ", "");
                    string ft = ""+ip[0] + ip[1] + ip[2];
                    
                    device.name = msg.Substring(0, msg.IndexOf(ft));
                    device.ipAddress = ip;
                        

                    deviceAdded?.Invoke(device, EventArgs.Empty);


                    pending.Remove(msg);

                    SpinWait.SpinUntil(() => device.width > 0, new TimeSpan(0, 0, 5));

                    if(!devices.ContainsKey(device.name))
                        devices.Add(device.name, device);

                

            }
            
        });
        task.Start();

        lock(tasks) {
            tasks.Add(new Task[] { task, task.WaitAsync(new TimeSpan(0, 0, 2)) });
        }
    }

    private void trash(object? sender, EventArgs e) {

        Device device = sender as Device;

        if(device != null)
            if(devices.ContainsKey(device.ipAddress)) {
                devices.Remove(device.ipAddress);
            }

        deviceRemoved?.Invoke(sender, EventArgs.Empty);
    }

    List<Task[]> tasks = new List<Task[]>();

    public static IPAddress local;

    public static IPAddress localAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    void startService(object? sender, EventArgs e) { start(); }
    void stopService(object? sender, EventArgs e) { stop(); }
    void disconnectAllDevices(object? sender, EventArgs e) { disconnectAll(); }
    void exitService(object? sender, EventArgs e) {
        exit(); 
        Environment.Exit(0);
    }


    public void start() {
        Console.WriteLine("listening for connections");
        listen();
    }

    

    public void stop() {

        run = false;

        if(client != null)
            client.Close();
        
        if(listener != null)
            listener.Stop();
    }

    

    public void disconnectAll() {
        
        //Console.WriteLine("disconnecting all " + devices.Count + " devices");

        foreach(Device d in devices.Values) {
            d.disconnect();
            Console.WriteLine(d.name + " ");
        }

    }



    public void exit() {

        if(listener != null)
        listener.Stop();

        run = false;

        if(client != null) {



            client.Dispose();
            client.Close();
            
        }

        if(devices.Count > 0)
            disconnectAll();
        
        icon.Visible = false;
        if(menu != null)
            menu.Dispose();
        icon.Dispose();
        menu = null;
        
        exiting?.Invoke(this, EventArgs.Empty);
    }

}