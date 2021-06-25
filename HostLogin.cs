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
    public partial class HostLogin : Form
    {
        public HostLogin()
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
                CustomMessageBox.Error("포트번호를 입력해주세요.");
            }
            else
            {
                int iPort = 0;
                try
                {
                    iPort = int.Parse(textBox1.Text);
                }
                catch
                {
                    CustomMessageBox.Error("정확한 포트번호를 입력해주세요.");
                }

                string sChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                char[] cArray = sChars.ToCharArray();
                int iPWlength = 5;
                string sPassword = "";
                int iSeed = Environment.TickCount;

                Random rd = new Random(iSeed);
                int iTemp = 0;

                for (int j = 0; j < iPWlength; j++)
                {
                    iTemp = rd.Next(0, cArray.Length - 1);
                    sPassword += cArray[iTemp];
                }

                this.Hide();
                FileTransmitForm_Host form = new FileTransmitForm_Host();
                form.sPassword = sPassword;
                form.iPort = iPort;
                form.sIP = textBox2.Text;
                form.ShowDialog();
                this.Close();
            }
        }

        public static string getIP()
        {
           
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            if (addr[2].ToString().StartsWith("192.168.")) //내부 아이피일 경우
            {
                
                return new WebClient().DownloadString("http://ipinfo.io/ip").Trim(); //IP를 띄어주는 웹에서 받아옴
            }
            return addr[2].ToString();
        }


        private void Form3_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = true;

            textBox2.Text = getIP();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Enabled = false;
                textBox1.Text = "9000";
            }
            else
            {
                textBox1.Enabled = true;
                textBox1.Text = "";
            }
        }
    }
}
