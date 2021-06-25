using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace _2021_1_file_p2p
{
    public partial class LoadingDialog_Host : Form
    {
        public string sInfo { get; set; }
        public string sText { get; set; }
        public string sPassword { get; set; }
        public int iPort { get; set; }

        private Thread tServer;
        private Thread loadingThread;
        private TcpListener tListener;
        private Encoding UTF8;
        private FileTransmitForm_Host Ow;

        private bool isConnected;
        
        public LoadingDialog_Host()
        {
            InitializeComponent();
        }

        private void LoadingDialog_Host_Load(object sender, EventArgs e)
        {
            this.Text = sText;

            tServer = new Thread(new ParameterizedThreadStart(server));
            tServer.IsBackground = true;
            tServer.Start();

            loadingThread = new Thread(new ParameterizedThreadStart(textLoading));
            loadingThread.Start();

            UTF8 = Encoding.UTF8;

            isConnected = false;

            Ow = (FileTransmitForm_Host)Owner;
        }

        private void textLoading(Object o)
        {
            label1.Text = sInfo;
            while (true)
            {
                label2.Text = sText;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(500);
                    label2.Text = label2.Text + ".";
                }
            }
        }

        private void server(Object o)
        {
            tListener = new TcpListener(IPAddress.Any, iPort);
            while (true)
            {
                tListener.Start();
                TcpClient serverClient = tListener.AcceptTcpClient();

                if (serverClient.Connected)
                {
                    NetworkStream ns = serverClient.GetStream();
                    byte[] bBuffer = new byte[1024];

                    int iBytes = ns.Read(bBuffer, 0, bBuffer.Length);

                    if (iBytes == 0)
                    {
                        Ow.setState(false);
                        CustomMessageBox.Error("연결이 끊겼습니다.");
                        ns.Write(bBuffer, 0, bBuffer.Length);
                        isConnected = false;
                        break;
                    }
                    else if (!isConnected)
                    {
                        string sByte = UTF8.GetString(bBuffer, 0, bBuffer.Length);

                        if (sByte.Substring(0, 5).Equals(sPassword))
                        {
                            ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length);
                            Ow.setState(true);
                            isConnected = true;
                            Ow.TCPClient = serverClient;
                            Close();
                        }
                        else
                        {
                            ns.Write(UTF8.GetBytes("NO"), 0, UTF8.GetBytes("NO").Length);
                        }
                    }
                }
            }
        }

        private void LoadingDialog_Host_FormClosed(object sender, FormClosedEventArgs e)
        {
            tListener.Stop();
            tServer.Abort();
            loadingThread.Abort();
        }

        private void LoadingDialog_Host_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isConnected)
            {
                DialogResult result = MessageBox.Show("정말로 종료하시겠습니까?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            } 
        }
    }
}
