using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace WindowsFormsSample
{
    public partial class ChatForm : Form
    {
        private HubConnection _connection;

        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            addressTextBox.Focus();
        }

        private void addressTextBox_Enter(object sender, EventArgs e)
        {
            AcceptButton = connectButton;
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            UpdateState(connected: false);

            //_connection = new HubConnectionBuilder()
            //    .WithUrl(addressTextBox.Text)
            //    .Build();

            _connection = new HubConnectionBuilder()
                    .WithUrl( addressTextBox.Text,
                              options=> {
                                  options.AccessTokenProvider = getAccessToken;
                              })
                    .WithAutomaticReconnect()
                    .Build();

            //_connection.On<string, string>("broadcastMessage", (s1, s2) => OnSend(s1, s2));

            _connection.On<string,string>("SetWork", (type,work) => CurrentWork(work));
            _connection.On<string,string>("Recive", (type, data) => Recive(data));
            _connection.On("StopWork", () => StopWork());

            Log(Color.Gray, "Starting connection...");
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Log(Color.Red, ex.ToString());
                return;
            }

            Log(Color.Gray, "Connection established.");

            UpdateState(connected: true);

            messageTextBox.Focus();
        }
        async Task<string> getAccessToken() 
        {
            await Task.Yield();
            return "Authorization:bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiIyNTI4ODUyNjMwMDM3MjEiLCJBY2NvdW50IjoiYWRtaW4iLCJOYW1lIjoi566h55CG5ZGYIiwiU3VwZXJBZG1pbiI6MSwiVGVuYW50SWQiOiIxNDIzMDcwNzA5MTg3ODAiLCJUb2tlbklEIjozMTg4MzgyNjg3NTU5NzQsImlhdCI6MTY2MDAwNjU3NiwibmJmIjoxNjYwMDA2NTc2LCJleHAiOjE2NjA2MTEzNzYsImlzcyI6IklUQyIsImF1ZCI6IklUQyJ9.WCDsVMLLNolNUXdd7-oYAPX18iY3ADKUAFZg9PVossg ";  
        }

    private async void disconnectButton_Click(object sender, EventArgs e)
        {
            Log(Color.Gray, "Stopping connection...");
            try
            {
                await _connection.StopAsync();
            }
            catch (Exception ex)
            {
                Log(Color.Red, ex.ToString());
            }

            Log(Color.Gray, "Connection terminated.");

            UpdateState(connected: false);
        }

        private void messageTextBox_Enter(object sender, EventArgs e)
        {
            AcceptButton = sendButton;
        }
        public Task CurrentWork(string work)
        {
            return Task.Run(() => Log(Color.Gray, work));
        }
        public Task Recive(string data)
        {
            return Task.Run(() => Log(Color.Gray, data));

        }
        public Task StopWork()
        {
            return Task.Run(() => Log(Color.Gray, "StopWork"));
        }



        private async void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _connection.InvokeAsync("Start", "worktype", "workdate");
                await _connection.InvokeAsync("Send","type",messageTextBox.Text);
                await _connection.InvokeAsync("FinishWork");
            }
            catch (Exception ex)
            {
                Log(Color.Red, ex.ToString());
            }
        }

        private void UpdateState(bool connected)
        {
            disconnectButton.Enabled = connected;
            connectButton.Enabled = !connected;
            addressTextBox.Enabled = !connected;

            messageTextBox.Enabled = connected;
            sendButton.Enabled = connected;
        }

        private void OnSend(string name, string message)
        {
            Log(Color.Black, name + ": " + message);
        }

        private void Log(Color color, string message)
        {
            Action callback = () =>
            {
                messagesList.Items.Add(new LogMessage(color, message));
            };

            Invoke(callback);
        }

        private class LogMessage
        {
            public Color MessageColor { get; }

            public string Content { get; }

            public LogMessage(Color messageColor, string content)
            {
                MessageColor = messageColor;
                Content = content;
            }
        }

        private void messagesList_DrawItem(object sender, DrawItemEventArgs e)
        {
            var message = (LogMessage)messagesList.Items[e.Index];
            e.Graphics.DrawString(
                message.Content,
                messagesList.Font,
                new SolidBrush(message.MessageColor),
                e.Bounds);
        }
    }


    public  class SynergyData
    {
        public string Id { get; set; }
        public  string Type { get; set; }
    }
    public abstract class SynergyWork
    {
        public string Id { get; set; }

        /// <summary>
        /// 工作所属的会议
        /// </summary>
        public string MeetingId { get; set; }

        /// <summary>
        /// 工作类型
        /// 1全屏批注 2文档批注 3白板 4分屏白板
        /// </summary>
        public int WorkType { get; set; }
        /// <summary>
        /// 工作参数（）
        /// </summary>
        public string WorkParameter { get; set; }
    }
}
