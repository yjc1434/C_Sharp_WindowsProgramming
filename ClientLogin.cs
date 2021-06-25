using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace _2021_1_file_p2p
{
    public partial class ClientLogin : Form
    {
        private TcpClient tClient;
        public ClientLogin()
        {
            InitializeComponent();
        }
        private bool checkIP(string target)
        {
            return new Regex(@"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$").IsMatch(target);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                CustomMessageBox.Error("IP 주소를 입력해주세요.");
            }
            else
            {
                string sIP = "";
                int iPort = 9000;

                if (textBox1.Text.Contains(':'))
                {
                    sIP = textBox1.Text.Substring(0, textBox1.Text.IndexOf(':'));
                    try
                    {
                        iPort = int.Parse(textBox1.Text.Substring(textBox1.Text.IndexOf(':') + 1));
                        MessageBox.Show(iPort.ToString());
                    }
                    catch
                    {
                        CustomMessageBox.Error("정확한 포트번호를 입력해주세요.");
                    }
                }
                else
                {
                    sIP = textBox1.Text;
                }

                if (!checkIP(sIP))
                {
                    CustomMessageBox.Error("정확한 IP 주소를 입력해주세요.");
                }
                else
                {
                    if (textBox2.Text == "")
                    {
                         CustomMessageBox.Error("비밀번호를 입력해주세요.");
                    }
                    else
                    {
                        try
                        {
                            tClient = new TcpClient(sIP, iPort);
                            if (tClient.Connected)
                            {
                                NetworkStream ns = tClient.GetStream();

                                Encoding UTF8 = Encoding.UTF8;
                                byte[] bWrite = UTF8.GetBytes(textBox2.Text);
                                byte[] bRead = new byte[1025];
                                if (tClient != null)
                                {
                                    ns.Write(bWrite, 0, bWrite.Length);
                                    ns.Read(bRead, 0, bRead.Length);
                                    if (UTF8.GetString(bRead).Substring(0, 2) == "OK")
                                    {
                                        FileTransmitForm_Client form = new FileTransmitForm_Client(tClient);
                                        this.Hide();
                                        form.ShowDialog();
                                        this.Close();
                                    }
                                    else
                                    {
                                        CustomMessageBox.Error("비밀번호가 틀렸습니다.");
                                    }
                                }
                            }
                            tClient.Close();
                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Error(ex.Message);
                        }
                    }
                }
            }
        }

        private void ClientLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
