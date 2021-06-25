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
    public partial class LoadingDialog_Send : Form
    {
        public string sStatus { get; set; }
        public bool isFinished { get; set; }
        Thread loadingThread;
        Thread byteThread;
        bool downMode;
        FileTransmitForm_Host host;
        FileTransmitForm_Client client;
        public LoadingDialog_Send()
        {
            InitializeComponent();
        }

        private void LoadingDialog_Send_Load(object sender, EventArgs e)
        {
            loadingThread = new Thread(new ParameterizedThreadStart(textLoading));
            loadingThread.IsBackground = true;
            loadingThread.Start();

            byteThread = new Thread(new ParameterizedThreadStart(byteLoading));
            byteThread.IsBackground = true;
            byteThread.Start();

            if (sStatus == "전송 중")
            {
                downMode = true;
                host = (FileTransmitForm_Host)Owner;
            }
            else
            {
                downMode = false;
                client = (FileTransmitForm_Client)Owner;
            }
            isFinished = false;
        }


        private void textLoading(Object o)
        {
            try
            {
                while (true)
                {
                    label1.Text = sStatus;
                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(500);
                        label1.Text = label1.Text + ".";
                    }
                }
            }
            catch
            {

            }
        }

        private void byteLoading(Object o)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (downMode)
                    {
                        label2.Text = host.sNowFile + " (" + convertByte(host.sNowFileDownload) + " / " + convertByte(host.sNowFileLength) + ")";
                    }
                    else
                    {
                        label2.Text = client.sNowFile + " (" + convertByte(client.sNowFileDownload) + " / " + convertByte(client.sNowFileLength) + ")";
                    }
                }
            }
            catch
            {

            }
        }

        private string convertByte(int length)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (length > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(length, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        private void LoadingDialog_Send_FormClosed(object sender, FormClosedEventArgs e)
        {
            loadingThread.Abort();
            byteThread.Abort();
        }

        private void LoadingDialog_Send_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isFinished)
            {
                DialogResult result = MessageBox.Show(sStatus + "인 파일이 있습니다. 정말로 종료하시겠습니까?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    if (downMode)
                    {
                        host.isCancel = true;
                    }
                    else
                    {
                        client.isCancel = true;
                    }
                }
            }
            
        }
    }
}
