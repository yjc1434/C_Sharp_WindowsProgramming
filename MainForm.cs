using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2021_1_file_p2p
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            HostLogin dlg = new HostLogin();
            if (dlg.ShowDialog() != DialogResult.Retry)
            {
                this.Close();
            }
            else
            {
                dlg.Close();
                this.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            ClientLogin dlg = new ClientLogin();
            if (dlg.ShowDialog() != DialogResult.Retry)
            {
                this.Close();
            }
            else
            {
                dlg.Close();
                this.Show();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
