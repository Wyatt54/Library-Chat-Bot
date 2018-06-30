using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ISBNdbSearch
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            string results = tbResults.Text;

            if (results.Length == 0)
            {
                MessageBox.Show("Nothing to save", "Warning Message",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(saveFileDialog1.FileName,
                        FileMode.Create);
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] ascii = encoding.GetBytes(results);
                    fs.Write(ascii, 0, ascii.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Warning Message",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            SearchISBNdB search = new SearchISBNdB();
            String accessKey = tbAccessKey.Text;
            String results = search.GetXMLData(accessKey, tbISBN.Text);

            tbResults.Text = results;
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbResults.Text = string.Empty;
        }
    }
}
