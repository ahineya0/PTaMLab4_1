using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetworkApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // ─── Контролы ───
        private ComboBox cmbDrive;
        private ListBox lstFiles;
        private RichTextBox rtbClient;
        private RichTextBox rtbServer;
        private Label lblClient;
        private Label lblServer;
        private TextBox txtIpAddress;
        private Label lblIpAddress;
        private Button btnToggleServer;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnExit;
        private Button btnSendToServer;
        private Button btnSendToClient;
        private Panel panelBottom;
        private Panel panelMain;
        private Panel panelLeft;
        private Panel panelRight;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ─── Инициализация контролов ───
            this.cmbDrive        = new ComboBox();
            this.lstFiles        = new ListBox();
            this.rtbClient       = new RichTextBox();
            this.rtbServer       = new RichTextBox();
            this.lblClient       = new Label();
            this.lblServer       = new Label();
            this.txtIpAddress    = new TextBox();
            this.lblIpAddress    = new Label();
            this.btnToggleServer = new Button();
            this.btnConnect      = new Button();
            this.btnDisconnect   = new Button();
            this.btnExit         = new Button();
            this.btnSendToServer = new Button();
            this.btnSendToClient = new Button();
            this.panelBottom     = new Panel();
            this.panelMain       = new Panel();
            this.panelLeft       = new Panel();
            this.panelRight      = new Panel();

            this.SuspendLayout();

            // ─── Form ───
            this.Text            = "Программа для обмена данными между компьютерами";
            this.Size            = new Size(760, 500);
            this.MinimumSize     = new Size(760, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormClosing    += new FormClosingEventHandler(Form1_FormClosing);

            // ─── panelMain (основная рабочая область) ───
            this.panelMain.Dock    = DockStyle.Fill;
            this.panelMain.Padding = new Padding(5, 5, 5, 0);

            // ─── panelLeft (дерево файлов) ───
            this.panelLeft.Dock    = DockStyle.Left;
            this.panelLeft.Width   = 210;
            this.panelLeft.Padding = new Padding(0, 0, 4, 0);

            // cmbDrive
            this.cmbDrive.Dock          = DockStyle.Top;
            this.cmbDrive.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbDrive.Font          = new Font("Segoe UI", 9f);
            this.cmbDrive.SelectedIndexChanged += new EventHandler(cmbDrive_SelectedIndexChanged);

            // lstFiles
            this.lstFiles.Dock              = DockStyle.Fill;
            this.lstFiles.Font              = new Font("Segoe UI", 9f);
            this.lstFiles.ScrollAlwaysVisible = false;
            this.lstFiles.DoubleClick       += new EventHandler(lstFiles_DoubleClick);

            this.panelLeft.Controls.Add(this.lstFiles);
            this.panelLeft.Controls.Add(this.cmbDrive);

            // ─── panelRight (клиент + сервер) ───
            this.panelRight.Dock = DockStyle.Fill;

            // lblClient
            this.lblClient.Text      = "Клиентская сторона";
            this.lblClient.Font      = new Font("Segoe UI", 9f, FontStyle.Regular);
            this.lblClient.TextAlign = ContentAlignment.MiddleCenter;
            this.lblClient.Dock      = DockStyle.None;
            this.lblClient.Size      = new Size(260, 20);
            this.lblClient.Location  = new Point(0, 0);

            // lblServer
            this.lblServer.Text      = "Серверная сторона";
            this.lblServer.Font      = new Font("Segoe UI", 9f, FontStyle.Regular);
            this.lblServer.TextAlign = ContentAlignment.MiddleCenter;
            this.lblServer.Dock      = DockStyle.None;
            this.lblServer.Size      = new Size(260, 20);
            this.lblServer.Location  = new Point(268, 0);

            // rtbClient
            this.rtbClient.Location  = new Point(0, 22);
            this.rtbClient.Size      = new Size(260, 0); // высота задаётся при resize
            this.rtbClient.Font      = new Font("Segoe UI", 8f);
            this.rtbClient.ReadOnly  = true;
            this.rtbClient.BackColor = SystemColors.Window;
            this.rtbClient.Anchor    = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // rtbServer
            this.rtbServer.Location  = new Point(268, 22);
            this.rtbServer.Size      = new Size(260, 0);
            this.rtbServer.Font      = new Font("Segoe UI", 8f);
            this.rtbServer.ReadOnly  = true;
            this.rtbServer.BackColor = SystemColors.Window;
            this.rtbServer.Anchor    = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.panelRight.Controls.Add(this.lblClient);
            this.panelRight.Controls.Add(this.lblServer);
            this.panelRight.Controls.Add(this.rtbClient);
            this.panelRight.Controls.Add(this.rtbServer);
            this.panelRight.Resize += new EventHandler(PanelRight_Resize);

            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Controls.Add(this.panelLeft);

            // ─── panelBottom (нижняя панель управления) ───
            this.panelBottom.Dock      = DockStyle.Bottom;
            this.panelBottom.Height    = 80;
            this.panelBottom.Padding   = new Padding(5, 4, 5, 4);
            this.panelBottom.BorderStyle = BorderStyle.FixedSingle;

            // lblIpAddress
            this.lblIpAddress.Text      = "IP-адрес";
            this.lblIpAddress.Font      = new Font("Segoe UI", 9f);
            this.lblIpAddress.AutoSize  = true;
            this.lblIpAddress.Location  = new Point(8, 10);

            // txtIpAddress
            this.txtIpAddress.Text     = "127.0.0.1";
            this.txtIpAddress.Font     = new Font("Segoe UI", 9f);
            this.txtIpAddress.Size     = new Size(100, 22);
            this.txtIpAddress.Location = new Point(8, 28);

            // btnToggleServer
            this.btnToggleServer.Text     = "Сервер включить";
            this.btnToggleServer.Font     = new Font("Segoe UI", 9f);
            this.btnToggleServer.Size     = new Size(130, 26);
            this.btnToggleServer.Location = new Point(118, 10);
            this.btnToggleServer.Click   += new EventHandler(btnToggleServer_Click);

            // btnConnect
            this.btnConnect.Text     = "Соединиться";
            this.btnConnect.Font     = new Font("Segoe UI", 9f);
            this.btnConnect.Size     = new Size(100, 26);
            this.btnConnect.Location = new Point(8, 46);
            this.btnConnect.Click   += new EventHandler(btnConnect_Click);

            // btnDisconnect
            this.btnDisconnect.Text     = "Отключиться";
            this.btnDisconnect.Font     = new Font("Segoe UI", 9f);
            this.btnDisconnect.Size     = new Size(100, 26);
            this.btnDisconnect.Location = new Point(118, 46);
            this.btnDisconnect.Enabled  = false;
            this.btnDisconnect.Click   += new EventHandler(btnDisconnect_Click);

            // btnExit
            this.btnExit.Text     = "Выход";
            this.btnExit.Font     = new Font("Segoe UI", 9f);
            this.btnExit.Size     = new Size(80, 26);
            this.btnExit.Location = new Point(228, 46);
            this.btnExit.Click   += new EventHandler(btnExit_Click);

            // btnSendToServer
            this.btnSendToServer.Text     = "Передать серверу";
            this.btnSendToServer.Font     = new Font("Segoe UI", 9f);
            this.btnSendToServer.Size     = new Size(140, 26);
            this.btnSendToServer.Location = new Point(320, 46);
            this.btnSendToServer.Enabled  = false;
            this.btnSendToServer.Click   += new EventHandler(btnSendToServer_Click);

            // btnSendToClient
            this.btnSendToClient.Text     = "Передать клиенту";
            this.btnSendToClient.Font     = new Font("Segoe UI", 9f);
            this.btnSendToClient.Size     = new Size(140, 26);
            this.btnSendToClient.Location = new Point(470, 46);
            this.btnSendToClient.Click   += new EventHandler(btnSendToClient_Click);

            this.panelBottom.Controls.Add(this.lblIpAddress);
            this.panelBottom.Controls.Add(this.txtIpAddress);
            this.panelBottom.Controls.Add(this.btnToggleServer);
            this.panelBottom.Controls.Add(this.btnConnect);
            this.panelBottom.Controls.Add(this.btnDisconnect);
            this.panelBottom.Controls.Add(this.btnExit);
            this.panelBottom.Controls.Add(this.btnSendToServer);
            this.panelBottom.Controls.Add(this.btnSendToClient);

            // ─── Добавление панелей на форму ───
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelBottom);

            this.ResumeLayout(false);
        }

        // Динамическое выравнивание RTB при изменении размера панели
        private void PanelRight_Resize(object sender, EventArgs e)
        {
            int halfWidth   = (this.panelRight.Width - 10) / 2;
            int textHeight  = this.panelRight.Height - 24;

            this.rtbClient.Size     = new Size(halfWidth, textHeight);
            this.rtbClient.Location = new Point(0, 22);

            this.rtbServer.Size     = new Size(halfWidth, textHeight);
            this.rtbServer.Location = new Point(halfWidth + 8, 22);

            this.lblClient.Size     = new Size(halfWidth, 20);
            this.lblClient.Location = new Point(0, 0);

            this.lblServer.Size     = new Size(halfWidth, 20);
            this.lblServer.Location = new Point(halfWidth + 8, 0);
        }
    }
}
