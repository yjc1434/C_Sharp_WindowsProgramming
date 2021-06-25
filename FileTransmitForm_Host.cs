using System;
using System.Collections;
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
    public partial class FileTransmitForm_Host : Form
    {
        private TcpClient tClient;
        private Encoding UTF8;
        private LoadingDialog_Send loadingDialog;
        private bool isConnected;
        public bool isCancel { get; set; }

        public string sPassword { get; set; }
        public string sIP { get; set; }
        public int iPort { get; set; }
        public TcpClient TCPClient { get { return tClient; } set { tClient = value; } }
        public string sNowFile { get; set; }
        public int sNowFileLength { get; set; }
        public int sNowFileDownload { get; set; }

        public FileTransmitForm_Host()
        {
            InitializeComponent();
        }

        public void setState(bool state)
        {
            if (state)
            {
                label3.Text = "연결 됨";
                isConnected = true;
            }
            else
            {
                label3.Text = "연결 안 됨";
                isConnected = false;
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
        private void button1_Click(object sender, EventArgs e)
        {
            int iCount = 0;
            string sWrite = "listBox;";
            if (isConnected)
            {
                NetworkStream ns = tClient.GetStream();
                try
                {
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        byte[] recevbyte = new byte[2048];
                        iCount = openFileDialog1.FileNames.Length;
                        sWrite += iCount;
                        ns.Write(UTF8.GetBytes(sWrite), 0, UTF8.GetBytes(sWrite).Length); //몇개의 아이템을 추가할지 Write 해준다.
                        ns.Read(recevbyte, 0, recevbyte.Length); //추가했다고 신호를 받는다. 그 전까진 Block
                        foreach (string s in openFileDialog1.FileNames)
                        {
                            string fileName = s.Substring(s.LastIndexOf('\\') + 1);
                            FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read);
                            int fLength = (int)fs.Length;
                            sWrite = fileName + " (" + convertByte(fLength) + ")";
                            listBox1.Items.Add(s);
                            ns.Flush();
                            ns.Write(UTF8.GetBytes(sWrite), 0, UTF8.GetBytes(sWrite).Length);//파일 이름 보냄
                            ns.Read(recevbyte, 0, recevbyte.Length); //체크 신호 받음.
                            label8.Text = listBox1.Items.Count + "개의 파일";
                            label7.Text = listBox2.Items.Count + "개의 파일";
                        }
                        CustomMessageBox.None(iCount + "개의 파일이 추가되었습니다.");
                    }
                    else
                    {
                        CustomMessageBox.Error("상대방과 연결된 후 사용 가능합니다.");
                    }
                }
                catch
                {
                    label3.Text = "상태 : 연결 안 됨";
                    CustomMessageBox.Error("연결이 끊겼습니다.");
                    ns.Write(new byte[1], 0, 1);
                    isConnected = false;
                }
            }
        }

        private void FileTransmitForm_Load(object sender, EventArgs e)
        {
            label1.Text = " * IP주소 : " + sIP + ":" + iPort + "\n\n* 비밀번호 : " + sPassword;

            isConnected = false;

            Thread tDialog = new Thread(new ParameterizedThreadStart(showDialog));
            tDialog.Start();

            UTF8 = Encoding.UTF8;
        }


        private void showDialog(Object o)
        {
            LoadingDialog_Host hostLoading = new LoadingDialog_Host();
            hostLoading.sInfo = "IP주소 : " + sIP + ":" + iPort + " / 비밀번호 : " + sPassword;
            hostLoading.sText = "상대방 연결 대기 중";
            hostLoading.sPassword = sPassword;
            hostLoading.iPort = iPort;
            hostLoading.Owner = this;
            hostLoading.ShowDialog();
            if (isConnected)
            {
                
            }
            else
            {
                Close();
            }
        }
        private void showDialog_file(Object o)
        {
            loadingDialog = new LoadingDialog_Send();
            loadingDialog.Owner = this;
            loadingDialog.sStatus = "전송 중";
            loadingDialog.ShowDialog();
            if (isCancel)
            {
                this.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                if (isConnected)
                {
                    NetworkStream ns = tClient.GetStream();
                    try
                    {
                        DialogResult result = MessageBox.Show("선택된 파일을 삭제하시겠습니까?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            string write = "remove;" + listBox1.SelectedIndex + ";";
                            ns.Write(UTF8.GetBytes(write), 0, UTF8.GetBytes(write).Length);
                            byte[] buffer = new byte[2048];
                            int bytes = ns.Read(buffer, 0, buffer.Length);
                            string sBuffer = UTF8.GetString(buffer, 0, bytes);
                            if (sBuffer.Substring(0, 2) == "OK")
                            {
                                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                                label8.Text = listBox1.Items.Count + "개의 파일";
                                label7.Text = listBox2.Items.Count + "개의 파일";
                                CustomMessageBox.None("선택된 파일이 삭제되었습니다.");
                            }
                        }
                    }
                    catch
                    {
                        label3.Text = "상태 : 연결 안 됨";
                        CustomMessageBox.Error("연결이 끊겼습니다.");
                        ns.Write(new byte[1], 0, 1);
                        isConnected = false;
                    }
                }
            }
            else
            {
                CustomMessageBox.Error("선택된 파일이 없습니다.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                Thread t2 = new Thread(new ParameterizedThreadStart(file_send));
                t2.IsBackground = true;
                t2.Start(tClient);

            }
            else
            {
                CustomMessageBox.Error("상대방과 연결된 후 사용 가능합니다.");
            }
        }

        private void file_send(Object o)
        {
            TcpClient c = (TcpClient)o;
            NetworkStream ns = c.GetStream();
            int length = listBox1.Items.Count;
            Thread fileDlg = new Thread(showDialog_file);
            try
            {
                fileDlg.Start();
            }
            catch
            {

            }
            for (int i = 0; i < length; i++)
            {
                byte[] getBuffer = new byte[2048];
                string sWrite = "fileSend;";
                if (i == 0)
                {
                    sWrite += listBox1.Items.Count + ";";
                }
                else
                {
                    sWrite += 0 + ";";
                }

                ns.Write(UTF8.GetBytes(sWrite), 0, UTF8.GetBytes(sWrite).Length); //fileSend;
                int bytes = ns.Read(getBuffer, 0, getBuffer.Length); //받았다는 신호 read해줌.

                listBox1.SelectedIndex = 0;
                string sFileName = listBox1.SelectedItem.ToString();

                FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
                BinaryReader bReader = new BinaryReader(fs);

                sNowFile = sFileName.Substring(sFileName.LastIndexOf("\\") + 1);
                sNowFileLength = (int)fs.Length;
                sNowFileDownload = 0;
                int fLength = (int)fs.Length;
                byte[] buffer = BitConverter.GetBytes(fLength);
                getBuffer = new byte[2048];

                ns.Write(buffer, 0, buffer.Length); //파일 크기 전송
                bytes = ns.Read(getBuffer, 0, getBuffer.Length); //받았다는 신호 read해줌.
                string strbyte = UTF8.GetString(getBuffer, 0, bytes);
                if (strbyte.Substring(0, 2) == "OK")
                {
                    int iCount = fLength / 2048;
                    for (int j = 0; j < iCount + 1; j++)
                    {
                        buffer = bReader.ReadBytes(2048);
                        ns.Write(buffer, 0, buffer.Length);
                        sNowFileDownload += buffer.Length;
                        bytes = ns.Read(getBuffer, 0, getBuffer.Length); //받았다는 신호 read해줌.
                        strbyte = UTF8.GetString(getBuffer, 0, bytes);
                        if (strbyte.Substring(0, 2) != "OK")
                        {
                            MessageBox.Show(strbyte.Substring(0, 2));
                            break;
                        }
                    }

                    getBuffer = new byte[2048];
                    bytes = ns.Read(getBuffer, 0, getBuffer.Length); //다 끝났다는 신호 받음.
                    strbyte = UTF8.GetString(getBuffer, 0, bytes);
                    if (strbyte.Substring(0, 2) == "OK")
                    {
                        ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다음으로 넘겨버리자
                        bReader.Close();
                        fs.Close();

                        listBox2.Items.Add(listBox1.SelectedItem.ToString());

                        listBox1.Items.RemoveAt(0);
                        getBuffer = new byte[2048];
                        bytes = ns.Read(getBuffer, 0, getBuffer.Length); //다 끝났다는 신호 받음.
                        strbyte = UTF8.GetString(getBuffer, 0, bytes);

                        if (strbyte.Substring(0, 2) == "OK")
                        {
                            ns.Flush();
                            ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다음으로 넘겨버리자
                            label8.Text = listBox1.Items.Count + "개의 파일";
                            label7.Text = listBox2.Items.Count + "개의 파일";

                            getBuffer = new byte[2048];
                            ns.Flush();
                            bytes = ns.Read(getBuffer, 0, getBuffer.Length); //파일 크기
                            strbyte = UTF8.GetString(getBuffer, 0, bytes);
                            if (strbyte.Substring(0, 4) == "NEXT")
                            {
                                ns.Write(UTF8.GetBytes("OK"), 0, UTF8.GetBytes("OK").Length); //다음으로 넘겨버리자
                            }
                        }
                    }
                }
            }
            loadingDialog.isFinished = true;
            loadingDialog.Close();
            CustomMessageBox.None(length + "개의 파일 전송 완료");
        }


        private void FileTransmitForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy | DragDropEffects.Scroll;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                int iCount = 0;
                string sWrite = "listBox;";
                if (isConnected)
                {
                    NetworkStream ns = tClient.GetStream();
                    try
                    {
                            byte[] recevbyte = new byte[2048];
                            iCount = file.Length;
                            sWrite += iCount;
                            ns.Write(UTF8.GetBytes(sWrite), 0, UTF8.GetBytes(sWrite).Length); //몇개의 아이템을 추가할지 Write 해준다.
                            ns.Read(recevbyte, 0, recevbyte.Length); //추가했다고 신호를 받는다. 그 전까진 Block
                            foreach (string s in file)
                            {
                                string fileName = s.Substring(s.LastIndexOf('\\') + 1);
                                FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read);
                                int fLength = (int)fs.Length;
                                sWrite = fileName + " (" + convertByte(fLength) + ")";
                                listBox1.Items.Add(s);
                                ns.Flush();
                                ns.Write(UTF8.GetBytes(sWrite), 0, UTF8.GetBytes(sWrite).Length);//파일 이름 보냄
                                ns.Read(recevbyte, 0, recevbyte.Length); //체크 신호 받음.
                                label8.Text = listBox1.Items.Count + "개의 파일";
                                label7.Text = listBox2.Items.Count + "개의 파일";
                            }
                            CustomMessageBox.None(iCount + "개의 파일이 추가되었습니다.");
                    }
                    catch
                    {
                        label3.Text = "상태 : 연결 안 됨";
                        CustomMessageBox.Error("연결이 끊겼습니다.");
                        ns.Write(new byte[1], 0, 1);
                        isConnected = false;
                    }
                }
            }
        }
    }
}
