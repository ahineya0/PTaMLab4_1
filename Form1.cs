using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetworkApp
{
    public partial class Form1 : Form
    {
        private TcpListener _server;
        private TcpClient _client;
        private NetworkStream _clientStream;
        private Thread _serverThread;
        private bool _serverRunning = false;
        private bool _clientConnected = false;
        private bool _lastRequestWasFile = false;

        // Текущий путь на сервере, по которому навигация клиента.
        private string _currentServerPath = "";

        private const int PORT = 5000;

        public Form1()
        {
            InitializeComponent();
            // Список файлов пуст до подключения к серверу.
        }

        // Вспомогательные методы для потокобезопасного обновления UI.
        private void AppendClientLog(string text)
        {
            if (rtbClient.InvokeRequired)
            {
                rtbClient.Invoke(new Action<string>(AppendClientLog), text);
                return;
            }
            rtbClient.AppendText(text + "\r\n");
        }

        private void AppendServerLog(string text)
        {
            if (rtbServer.InvokeRequired)
            {
                rtbServer.Invoke(new Action<string>(AppendServerLog), text);
                return;
            }
            rtbServer.AppendText(text + "\r\n");
        }

        private void SetServerButtons(bool serverRunning)
        {
            if (btnToggleServer.InvokeRequired)
            {
                btnToggleServer.Invoke(new Action<bool>(SetServerButtons), serverRunning);
                return;
            }
            btnToggleServer.Text = serverRunning ? "Сервер отключить" : "Сервер включить";
        }

        private void SetClientButtons(bool connected)
        {
            if (btnConnect.InvokeRequired)
            {
                btnConnect.Invoke(new Action<bool>(SetClientButtons), connected);
                return;
            }
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            btnSendToServer.Enabled = connected;
        }

        // Заполнить cmbDrive списком дисков, пришедших от сервера.
        private void PopulateDrivesFromServer(string drivesLine)
        {
            if (cmbDrive.InvokeRequired)
            {
                cmbDrive.Invoke(new Action<string>(PopulateDrivesFromServer), drivesLine);
                return;
            }

            cmbDrive.Items.Clear();
            lstFiles.Items.Clear();

            string[] drives = drivesLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string drive in drives)
            {
                cmbDrive.Items.Add(drive.Trim());
            }

            if (cmbDrive.Items.Count > 0)
            {
                // Выбор диска вызовет cmbDrive_SelectedIndexChanged,
                // который запросит содержимое у сервера.
                cmbDrive.SelectedIndex = 0;
            }
        }

        // Заполнить lstFiles именами, пришедшими от сервера.
        private void PopulateListFromServer(string listing)
        {
            if (lstFiles.InvokeRequired)
            {
                lstFiles.Invoke(new Action<string>(PopulateListFromServer), listing);
                return;
            }

            lstFiles.Items.Clear();
            string[] lines = listing.Split(
                new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                lstFiles.Items.Add(line);
            }
        }

        // Навигация (левая панель).
        private void cmbDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_clientConnected || cmbDrive.SelectedItem == null)
            {
                return;
            }

            string path = cmbDrive.SelectedItem.ToString();
            RequestDirectoryListing(path);
        }

        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (!_clientConnected || lstFiles.SelectedItem == null)
            {
                return;
            }

            string selected = lstFiles.SelectedItem.ToString();

            // Каталоги помечены префиксом [DIR].
            if (!selected.StartsWith("[DIR] "))
            {
                return;
            }

            string dirName = selected.Substring(6); // убираем "[DIR] "
            string newPath = Path.Combine(_currentServerPath, dirName);
            RequestDirectoryListing(newPath);
        }

        // Отправить серверу запрос на листинг каталога.
        private void RequestDirectoryListing(string path)
        {
            if (!_clientConnected || _clientStream == null)
            {
                return;
            }

            try
            {
                _currentServerPath = path;
                cmbDrive.Text = path;
                SendMessage(_clientStream, path);
                _lastRequestWasFile = false;
                // Ответ придёт в фоновом потоке ReceiveLoop.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запроса каталога: " + ex.Message);
                DisconnectClient();
            }
        }

        // Сервер.
        private void btnToggleServer_Click(object sender, EventArgs e)
        {
            if (!_serverRunning)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
        }

        private void StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Parse(txtIpAddress.Text), PORT);
                _server.Start();
                _serverRunning = true;
                SetServerButtons(true);

                string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                AppendServerLog($"Сервер включён {timestamp} IP: {txtIpAddress.Text}");

                _serverThread = new Thread(ServerLoop);
                _serverThread.IsBackground = true;
                _serverThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запуска сервера: " + ex.Message);
            }
        }

        private void StopServer()
        {
            _serverRunning = false;
            try
            {
                _server?.Stop();
            }
            catch { }

            SetServerButtons(false);
            AppendServerLog("Сервер остановлен.");
        }

        private void ServerLoop()
        {
            while (_serverRunning)
            {
                try
                {
                    TcpClient clientConnection = _server.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(clientConnection));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch
                {
                    // Сервер остановлен, выходим из цикла.
                    break;
                }
            }
        }

        private void HandleClient(TcpClient clientConnection)
        {
            string clientAddress =
                ((IPEndPoint)clientConnection.Client.RemoteEndPoint).Address.ToString();
            string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            AppendServerLog($"Клиент соединился {timestamp} с адреса {clientAddress}");

            NetworkStream stream = clientConnection.GetStream();

            // Отправляем клиенту список логических дисков.
            string drives = string.Join(",", Directory.GetLogicalDrives());
            SendMessage(stream, drives);

            timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            AppendServerLog($"Сервер передал диски {timestamp}\r\n{drives}");

            try
            {
                while (clientConnection.Connected)
                {
                    string request = ReceiveMessage(stream);
                    if (request == null)
                    {
                        break;
                    }

                    timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    AppendServerLog($"Сервер получил {timestamp}\r\n{request}");

                    string response = ProcessRequest(request);
                    SendMessage(stream, response);
                }
            }
            catch { }
            finally
            {
                clientConnection.Close();
                AppendServerLog("Клиент отключился.");
            }
        }

        private string ProcessRequest(string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return "Пустой запрос.";
            }

            if (Directory.Exists(request))
            {
                StringBuilder sb = new StringBuilder();
                try
                {
                    string[] dirs = Directory.GetDirectories(request);
                    string[] files = Directory.GetFiles(request);
                    foreach (string d in dirs)
                    {
                        sb.AppendLine("[DIR] " + Path.GetFileName(d));
                    }
                    foreach (string f in files)
                    {
                        sb.AppendLine(Path.GetFileName(f));
                    }
                }
                catch (Exception ex)
                {
                    return "Ошибка чтения каталога: " + ex.Message;
                }
                return sb.ToString();
            }
            else if (File.Exists(request))
            {
                try
                {
                    return File.ReadAllText(request, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    return "Ошибка чтения файла: " + ex.Message;
                }
            }
            else
            {
                return "Путь не найден: " + request;
            }
        }

        // Клиент.
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIpAddress.Text.Trim();
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Введите IP-адрес.");
                return;
            }

            try
            {
                _client = new TcpClient();
                _client.Connect(ip, PORT);
                _clientStream = _client.GetStream();
                _clientConnected = true;
                SetClientButtons(true);

                // Запускаем фоновый поток приёма всех сообщений от сервера.
                Thread receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                AppendClientLog($"Клиент соединился {timestamp}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        // Фоновый поток: читает все входящие сообщения от сервера.
        // Первое сообщение список дисков, последующие ответы на запросы.
        private void ReceiveLoop()
        {
            bool firstMessage = true;

            try
            {
                while (_clientConnected)
                {
                    string message = ReceiveMessage(_clientStream);
                    if (message == null)
                    {
                        break;
                    }

                    string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                    if (firstMessage)
                    {
                        // Первое сообщение список дисков от сервера.
                        firstMessage = false;
                        AppendClientLog($"Клиент получил {timestamp}\r\n{message}");
                        PopulateDrivesFromServer(message);
                    }
                    else
                    {
                        // Ответ на запрос каталога или файла.
                        AppendClientLog($"Клиент получил {timestamp}\r\n{message}");

                        // Листинг каталога, обновляем lstFiles. Содержимое бинарного/текстового файла в список не кладём.
                        if (!_lastRequestWasFile)
                        {
                            PopulateListFromServer(message);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (_clientConnected)
                {
                    DisconnectClient();
                }
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            _clientConnected = false;
            try
            {
                _clientStream?.Close();
                _client?.Close();
            }
            catch { }

            // Очищаем дерево файлов при отключении.
            if (cmbDrive.InvokeRequired)
            {
                cmbDrive.Invoke(new Action(ClearFileTree));
            }
            else
            {
                ClearFileTree();
            }

            SetClientButtons(false);
            AppendClientLog("Отключено от сервера.");
        }

        private void ClearFileTree()
        {
            cmbDrive.Items.Clear();
            cmbDrive.Text = "";
            lstFiles.Items.Clear();
            _currentServerPath = "";
        }

        private void btnSendToServer_Click(object sender, EventArgs e)
        {
            if (!_clientConnected || _clientStream == null)
            {
                MessageBox.Show("Нет подключения к серверу.");
                return;
            }

            if (lstFiles.SelectedItem == null)
            {
                MessageBox.Show("Выберите файл или каталог в списке.");
                return;
            }

            string selected = lstFiles.SelectedItem.ToString();

            // Убираем префикс [DIR], если есть.
            string name = selected.StartsWith("[DIR] ") ? selected.Substring(6) : selected;
            string fullPath = Path.Combine(_currentServerPath, name);
            _lastRequestWasFile = !selected.StartsWith("[DIR] ");

            try
            {
                SendMessage(_clientStream, fullPath);
                // Ответ придёт в ReceiveLoop.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки: " + ex.Message);
                DisconnectClient();
            }
        }

        private void btnSendToClient_Click(object sender, EventArgs e)
        {
            AppendServerLog("Передача клиенту управляется автоматически (ответ на запрос).");
        }

        // Протокол обмена: длина (4 байта) + данные UTF-8.
        private void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private string ReceiveMessage(NetworkStream stream)
        {
            byte[] lengthBuffer = new byte[4];
            int bytesRead = 0;

            while (bytesRead < 4)
            {
                int n = stream.Read(lengthBuffer, bytesRead, 4 - bytesRead);
                if (n == 0)
                {
                    return null;
                }
                bytesRead += n;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] dataBuffer = new byte[messageLength];
            bytesRead = 0;

            while (bytesRead < messageLength)
            {
                int n = stream.Read(dataBuffer, bytesRead, messageLength - bytesRead);
                if (n == 0)
                {
                    return null;
                }
                bytesRead += n;
            }

            return Encoding.UTF8.GetString(dataBuffer);
        }

        // Закрытие формы.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_clientConnected)
            {
                DisconnectClient();
            }
            if (_serverRunning)
            {
                StopServer();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}