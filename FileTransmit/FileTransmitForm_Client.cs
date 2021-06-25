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
    public partial class FileTransmitForm_Client : Form
    {
        private TcpClient tClient;
        private LoadingDialog_Send loadingDialog;
        private Thread tReceiver;
        public bool isCancel { get; set; }

        public string sNowFile { get; set; }
        public int sNowFileLength { get; set; }
        public int sNowFileDownload { get; set; }

        public FileTransmitForm_Client(TcpClient tClient)
        {
            InitializeComponent();
            this.tClient = tClient;
        }

        private void FileTransmitForm_Client_Load(object sender, EventArgs e)
        {
            tReceiver = new Thread(new ParameterizedThreadStart(receiver));
            tReceiver.IsBackground = true;
            tReceiver.Start(tClient);

            folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            label1.Text = "현재 경로 : " + folderBrowserDialog1.SelectedPath;
            isCancel = false;
        }

        private void showDialog_file(Object o)
        {
            loadingDialog = new LoadingDialog_Send();
            loadingDialog.sStatus = "다운로드 중";
            loadingDialog.Owner = this;
            loadingDialog.ShowDialog();
            if (isCancel)
            {
                this.Close();
            }
        }

        private void receiver(Object o)
        {
            TcpClient client = (TcpClient)o;
            int fileDownLoad = 0;
            int downloaded = 0;
            byte[] recevbyte = new byte[2048];

            NetworkStream ns = client.GetStream();
            Encoding UTF8 = Encoding.UTF8;

            while (true)
            {
                try
                {
                    int bytes = ns.Read(recevbyte, 0, recevbyte.Length); //몇개의 리스트박스 아이템을 추가할지 Read해준다.
                    string strbyte = UTF8.GetString(recevbyte, 0, bytes);
                    ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //받았다고 신호를 Write 해준다.
                    if (bytes == 0)
                    {
                        ns.Write(recevbyte, 0, bytes);
                        break;
                    }
                    else
                    {
                        string[] sRead = strbyte.Split(new char[] { ';' });

                        if (sRead[0] == "listBox")
                        {
                            for (int i = 0; i < int.Parse(sRead[1]); i++)
                            {
                                ns.Flush();
                                recevbyte = new byte[2048];
                                bytes = ns.Read(recevbyte, 0, recevbyte.Length); //추가할 아이템의 이름을 Read해준다.
                                listBox1.Items.Add(UTF8.GetString(recevbyte)); //아이템을 추가해준다.
                                ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //추가했다고 신호를 Write 해준다.
                                label2.Text = listBox1.Items.Count + "개의 파일";
                                label4.Text = listBox2.Items.Count + "개의 파일";
                            }
                        }
                        else if (sRead[0] == "remove")
                        {
                            int index = int.Parse(sRead[1]);

                            try
                            {
                                listBox1.Items.RemoveAt(index);
                                label8.Text = listBox1.Items.Count + "개의 파일";
                                label7.Text = listBox2.Items.Count + "개의 파일";
                                ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //추가했다고 신호를 Write 해준다.
                            }
                            catch
                            {

                            }
                        }
                        else if (sRead[0] == "fileSend")
                        {
                            Thread fileDlg = new Thread(showDialog_file);
                            if (int.Parse(sRead[1]) > 0)
                            {
                                try
                                {
                                    fileDownLoad = int.Parse(sRead[1]);
                                    fileDlg.Start();
                                }
                                catch
                                {

                                }
                            }
                            listBox1.SelectedIndex = 0;
                            ns.Flush();
                            bytes = ns.Read(recevbyte, 0, recevbyte.Length); //파일 크기
                            ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //추가했다고 신호를 Write 해준다.
                            
                            if (bytes != 0)
                            {
                                int iFileSize = BitConverter.ToInt32(recevbyte, 0);
                                int iTotalSize = 0;
                                byte[] fileBuffer = new byte[2048];
                                recevbyte = new byte[2048];
                                string name = listBox1.SelectedItem.ToString();
                                string path = folderBrowserDialog1.SelectedPath + "\\" + name.Substring(0, name.LastIndexOf("(") - 1);
                                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                                BinaryWriter bw = new BinaryWriter(fs);
                                sNowFile = name.Substring(0, name.LastIndexOf("(") - 1);
                                sNowFileLength = iFileSize;
                                sNowFileDownload = 0;
                                while (iFileSize > iTotalSize)
                                {
                                    int receiveLength = ns.Read(fileBuffer, 0, fileBuffer.Length);
                                    sNowFileDownload += receiveLength;
                                    bw.Write(fileBuffer, 0, fileBuffer.Length);
                                    iTotalSize += receiveLength;
                                    ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다 끝났다고 말한다
                                }

                                if (iFileSize == iTotalSize)
                                {
                                    ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다 끝났다고 말한다
                                    bytes = ns.Read(recevbyte, 0, recevbyte.Length); //파일 크기
                                    strbyte = UTF8.GetString(recevbyte, 0, bytes);
                                    if (strbyte.Substring(0, 2) == "OK")
                                    {
                                        bw.Close();
                                        fs.Close();

                                        listBox2.Items.Add(listBox1.SelectedItem.ToString());
                                        listBox1.Items.RemoveAt(0);
                                        ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다 끝났다고 말한다
                                        bytes = ns.Read(recevbyte, 0, recevbyte.Length); //파일 크기
                                        strbyte = UTF8.GetString(recevbyte, 0, bytes);
                                        if (strbyte.Substring(0, 2) == "OK")
                                        {
                                            label2.Text = listBox1.Items.Count + "개의 파일";
                                            label4.Text = listBox2.Items.Count + "개의 파일";

                                            recevbyte = new byte[2048];
                                            ns.Flush();
                                            ns.Write(UTF8.GetBytes("NEXT"), 0, UTF8.GetBytes("NEXT").Length); //다음으로 넘겨버리자
                                            bytes = ns.Read(recevbyte, 0, recevbyte.Length); //파일 크기
                                            strbyte = UTF8.GetString(recevbyte, 0, bytes);
                                            downloaded++;
                                            if (fileDownLoad == downloaded)
                                            {
                                                loadingDialog.isFinished = true;
                                                loadingDialog.Close();
                                                CustomMessageBox.None(downloaded + "개의 파일 전송 완료");
                                            }
                                            if (strbyte.Substring(0, 2) != "OK")
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Error(ex.Message);
                    label3.Text = "상태 : 연결 안 됨";
                    break;
                }
            }
            client.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                label1.Text = "현재 경로 : " + folderBrowserDialog1.SelectedPath;
            }
        }

        private void FileTransmitForm_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isCancel)
            {
                DialogResult result = MessageBox.Show("정말로 종료하시겠습니까?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void FileTransmitForm_Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            tClient.Close();
            tReceiver.Abort();
        }
    }
}
