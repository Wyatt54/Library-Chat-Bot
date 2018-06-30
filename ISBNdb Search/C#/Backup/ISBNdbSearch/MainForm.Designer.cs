namespace ISBNdbSearch
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbResults = new System.Windows.Forms.TextBox();
            this.btSave = new System.Windows.Forms.Button();
            this.btSearch = new System.Windows.Forms.Button();
            this.tbISBN = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbAccessKey = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btClear = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(23, 158);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ReadOnly = true;
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(400, 252);
            this.tbResults.TabIndex = 17;
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(185, 107);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(75, 23);
            this.btSave.TabIndex = 16;
            this.btSave.Text = "S&ave";
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btSearch
            // 
            this.btSearch.Location = new System.Drawing.Point(23, 107);
            this.btSearch.Name = "btSearch";
            this.btSearch.Size = new System.Drawing.Size(75, 23);
            this.btSearch.TabIndex = 15;
            this.btSearch.Text = "&Search";
            this.btSearch.Click += new System.EventHandler(this.btSearch_Click);
            // 
            // tbISBN
            // 
            this.tbISBN.Location = new System.Drawing.Point(326, 66);
            this.tbISBN.Name = "tbISBN";
            this.tbISBN.Size = new System.Drawing.Size(100, 20);
            this.tbISBN.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(244, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "ISBN";
            // 
            // tbAccessKey
            // 
            this.tbAccessKey.Location = new System.Drawing.Point(326, 18);
            this.tbAccessKey.Name = "tbAccessKey";
            this.tbAccessKey.PasswordChar = '*';
            this.tbAccessKey.Size = new System.Drawing.Size(100, 20);
            this.tbAccessKey.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(244, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 23);
            this.label1.TabIndex = 9;
            this.label1.Text = "Access Key";
            // 
            // btClear
            // 
            this.btClear.Location = new System.Drawing.Point(348, 107);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(75, 23);
            this.btClear.TabIndex = 18;
            this.btClear.Text = "&Clear";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Text Files (*.txt) | *.txt";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 429);
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.btSearch);
            this.Controls.Add(this.tbISBN);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbAccessKey);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm - SearchISBNdb";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbResults;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btSearch;
        private System.Windows.Forms.TextBox tbISBN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbAccessKey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

