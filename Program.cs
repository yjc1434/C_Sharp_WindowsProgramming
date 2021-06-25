using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2021_1_file_p2p
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {

            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            ClientLogin clientLogin = new ClientLogin();
            HostLogin hostLogin = new HostLogin();
            FileTransmitForm_Client client = new FileTransmitForm_Client(null);
            FileTransmitForm_Host host = new FileTransmitForm_Host();
        }
    }
}
