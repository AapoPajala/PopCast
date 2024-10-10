namespace PopCast {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            startService = new Button();
            hideToTray = new Button();
            passwordBox = new TextBox();
            label1 = new Label();
            connectedList = new ListView();
            label2 = new Label();
            disconnectSelected = new Button();
            disconnectAll = new Button();
            stopService = new Button();
            hideWhenLocked = new CheckBox();
            setPassword = new Label();
            showPwd = new CheckBox();
            alwaysOnTop = new CheckBox();
            SuspendLayout();
            // 
            // startService
            // 
            startService.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            startService.Location = new Point(40, 59);
            startService.Name = "startService";
            startService.Size = new Size(120, 38);
            startService.TabIndex = 0;
            startService.Text = "start service";
            startService.UseVisualStyleBackColor = true;
            startService.Click += startService_Click;
            // 
            // hideToTray
            // 
            hideToTray.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            hideToTray.Location = new Point(40, 197);
            hideToTray.Name = "hideToTray";
            hideToTray.Size = new Size(120, 40);
            hideToTray.TabIndex = 1;
            hideToTray.Text = "hide to tray";
            hideToTray.UseVisualStyleBackColor = true;
            hideToTray.Click += hideToTray_Click;
            // 
            // passwordBox
            // 
            passwordBox.Location = new Point(40, 307);
            passwordBox.MaxLength = 16;
            passwordBox.Name = "passwordBox";
            passwordBox.PasswordChar = '*';
            passwordBox.Size = new Size(120, 23);
            passwordBox.TabIndex = 3;
            passwordBox.TextChanged += passwordBox_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(40, 289);
            label1.Name = "label1";
            label1.Size = new Size(74, 17);
            label1.TabIndex = 4;
            label1.Text = "password *";
            // 
            // connectedList
            // 
            connectedList.Location = new Point(239, 107);
            connectedList.MultiSelect = false;
            connectedList.Name = "connectedList";
            connectedList.Size = new Size(162, 155);
            connectedList.TabIndex = 5;
            connectedList.UseCompatibleStateImageBehavior = false;
            connectedList.View = View.List;
            connectedList.SelectedIndexChanged += connectedList_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(239, 83);
            label2.Name = "label2";
            label2.Size = new Size(151, 21);
            label2.TabIndex = 6;
            label2.Text = "connected devices";
            // 
            // disconnectSelected
            // 
            disconnectSelected.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            disconnectSelected.Location = new Point(239, 268);
            disconnectSelected.Name = "disconnectSelected";
            disconnectSelected.Size = new Size(162, 34);
            disconnectSelected.TabIndex = 7;
            disconnectSelected.Text = "disconnect selected";
            disconnectSelected.UseVisualStyleBackColor = true;
            disconnectSelected.Click += disconnectSelected_Click;
            // 
            // disconnectAll
            // 
            disconnectAll.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            disconnectAll.Location = new Point(407, 177);
            disconnectAll.Name = "disconnectAll";
            disconnectAll.Size = new Size(135, 37);
            disconnectAll.TabIndex = 8;
            disconnectAll.Text = "disconnect all";
            disconnectAll.UseVisualStyleBackColor = true;
            disconnectAll.Click += disconnectAll_Click;
            // 
            // stopService
            // 
            stopService.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            stopService.Location = new Point(40, 107);
            stopService.Name = "stopService";
            stopService.Size = new Size(120, 37);
            stopService.TabIndex = 9;
            stopService.Text = "stop service";
            stopService.UseVisualStyleBackColor = true;
            stopService.Click += stopService_Click;
            // 
            // hideWhenLocked
            // 
            hideWhenLocked.AutoSize = true;
            hideWhenLocked.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            hideWhenLocked.Location = new Point(40, 368);
            hideWhenLocked.Name = "hideWhenLocked";
            hideWhenLocked.Size = new Size(282, 21);
            hideWhenLocked.TabIndex = 10;
            hideWhenLocked.Text = "hide window when Android screen is locked";
            hideWhenLocked.UseVisualStyleBackColor = true;
            hideWhenLocked.CheckedChanged += hideWhenLocked_CheckedChanged;
            // 
            // setPassword
            // 
            setPassword.AutoSize = true;
            setPassword.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            setPassword.ForeColor = Color.Crimson;
            setPassword.Location = new Point(40, 21);
            setPassword.Name = "setPassword";
            setPassword.Size = new Size(202, 25);
            setPassword.TabIndex = 11;
            setPassword.Text = "please set a password";
            setPassword.Visible = false;
            // 
            // showPwd
            // 
            showPwd.AutoSize = true;
            showPwd.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            showPwd.Location = new Point(40, 336);
            showPwd.Name = "showPwd";
            showPwd.Size = new Size(118, 21);
            showPwd.TabIndex = 12;
            showPwd.Text = "show password";
            showPwd.UseVisualStyleBackColor = true;
            showPwd.CheckedChanged += showPwd_CheckedChanged;
            // 
            // alwaysOnTop
            // 
            alwaysOnTop.AutoSize = true;
            alwaysOnTop.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            alwaysOnTop.Location = new Point(40, 395);
            alwaysOnTop.Name = "alwaysOnTop";
            alwaysOnTop.Size = new Size(108, 21);
            alwaysOnTop.TabIndex = 13;
            alwaysOnTop.Text = "always on top";
            alwaysOnTop.UseVisualStyleBackColor = true;
            alwaysOnTop.CheckedChanged += alwaysOnTop_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(579, 450);
            Controls.Add(alwaysOnTop);
            Controls.Add(showPwd);
            Controls.Add(setPassword);
            Controls.Add(hideWhenLocked);
            Controls.Add(stopService);
            Controls.Add(disconnectAll);
            Controls.Add(disconnectSelected);
            Controls.Add(label2);
            Controls.Add(connectedList);
            Controls.Add(label1);
            Controls.Add(passwordBox);
            Controls.Add(hideToTray);
            Controls.Add(startService);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "PopCast";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button startService;
        private Button hideToTray;
        private TextBox passwordBox;
        private Label label1;
        private ListView connectedList;
        private Label label2;
        private Button disconnectSelected;
        private Button disconnectAll;
        private Button stopService;
        private CheckBox hideWhenLocked;
        private Label setPassword;
        private CheckBox showPwd;
        private CheckBox alwaysOnTop;
    }
}