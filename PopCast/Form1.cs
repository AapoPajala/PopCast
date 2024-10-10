using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PopCast {
    public partial class Form1 : System.Windows.Forms.Form {
        public Form1() {
            InitializeComponent();
            listener = new Listener();
            syncContext = SynchronizationContext.Current;
            listener.openForm += open;
            listener.deviceAdded += add;
            listener.deviceRemoved += remove;
            listener.exiting += closeFromOutside;

            Thread thread = new Thread(update);
            thread.Start();

            FormClosing += closing;

            key = "";
        }

        SynchronizationContext syncContext;

        private void remove(object? sender, EventArgs e) {

            Device s = sender as Device;

            if(s != null)
                syncContext.Post(new SendOrPostCallback(delegate (object state) {
                    connectedList.Items.RemoveByKey(s.name);
                    if(connectedList.Items.Count == 0) passwordBox.Enabled = true;
                }), null);
        }

        private void add(object? sender, EventArgs e) {

            Device s = sender as Device;


            if(s != null) {
                syncContext.Post(new SendOrPostCallback(delegate (object state) {
                    if(!connectedList.Items.ContainsKey(s.name))
                        connectedList.Items.Add(s.name, s.name, 0);
                }), null);
            }
        }

        private void open(object? sender, EventArgs e) {
            Show();
        }

        int devices;
        Listener listener;



        public void update() {
            /*MethodInvoker mi;
            while(true) {
                mi = delegate () {

                    

                };
                try {
                    Invoke(mi);
                } catch(Exception) {

                };

                Thread.Sleep(100);
            }*/
        }

        private void startService_Click(object sender, EventArgs e) {
            if(key.Length > 0) {
                listener.start();
                setPassword.Visible = false;
                passwordBox.Enabled = false;
                startService.ForeColor = Color.Green;
                Text = "PopCast  |  listening for connections";
            } else {
                setPassword.Visible = true;
                passwordBox.Select();
            }


        }

        private void stopService_Click(object sender, EventArgs e) {
            listener.stop();
            startService.ForeColor = Color.Black;
            Text = "PopCast";
            if(connectedList.Items.Count == 0) passwordBox.Enabled = true;
        }

        private void hideToTray_Click(object sender, EventArgs e) {
            Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e) {
            //AllocConsole();
        }

        public static event EventHandler shutdown;
        private void closing(object? sender, FormClosingEventArgs e) {
            if(e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                DialogResult result = MessageBox.Show("Exit PopCast?", "PopCast", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes) {
                    if(listener != null) {
                        listener.exit();
                    }
                    shutdown?.Invoke(this, EventArgs.Empty);
                    Dispose();
                    Application.Exit();
                    Environment.Exit(0);
                }
            }
        }

        void closeFromOutside(object? sender, EventArgs e) {
            Dispose();
            Application.Exit();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void disconnectAll_Click(object sender, EventArgs e) {
            listener.disconnectAll();
            connectedList.Clear();
        }

        private void connectedList_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void disconnectSelected_Click(object sender, EventArgs e) {
            foreach(Device d in Listener.devices.Values) {
                if(connectedList.SelectedItems.Count > 0) {
                    ListViewItem i = connectedList.SelectedItems[0];
                    string t = i.Text;
                    if(d.name.Equals(t)) {
                        d.disconnect();
                        Listener.devices.Remove(d.name);
                        connectedList.Items.RemoveAt(connectedList.Items.IndexOf(i));
                    }
                }
            }
        }
        public static bool hideOnLock;
        private void hideWhenLocked_CheckedChanged(object sender, EventArgs e) {
            hideOnLock = hideWhenLocked.Checked;
        }
        public static string key;
        private void passwordBox_TextChanged(object sender, EventArgs e) {
            key = passwordBox.Text;

            if(key.Length >= 16) passwordBox.BackColor = Color.LightGreen;
            else if(key.Length >= 8) passwordBox.BackColor = Color.Orange;
            else if(key.Length >= 4) passwordBox.BackColor = Color.Yellow;
            else if(key.Length > 0) passwordBox.BackColor = Color.PaleVioletRed;
            else passwordBox.BackColor = Color.White;

            if(passwordBox.Text.Length > 0) {

                if(key.Length < 16) {
                    int d = 16 - key.Length;
                    for(int i = 0; i < d; i++)
                        key += key[(int)(key.Length * (i / (float)d))];
                }
            }
        }


        private void showPwd_CheckedChanged(object sender, EventArgs e) {
            if(showPwd.Checked) passwordBox.PasswordChar = '\0';
            else passwordBox.PasswordChar = '*';
        }

        public static event EventHandler onTopChanged;
        public static bool onTop;
        private void alwaysOnTop_CheckedChanged(object sender, EventArgs e) {
            onTop = alwaysOnTop.Checked;
            onTopChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

