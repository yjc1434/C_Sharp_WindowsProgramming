using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2021_1_file_p2p
{
    static class CustomMessageBox
    {
        static public void Error(string text)
        {
            MessageBox.Show(text, "파일 전송 프로그램", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        static public void None(string text)
        {
            MessageBox.Show(text, "파일 전송 프로그램", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
}
