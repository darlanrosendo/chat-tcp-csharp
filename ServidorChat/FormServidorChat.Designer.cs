namespace ServidorChat
{
    partial class FormServidorChat
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.numericUpDownNumPorta = new System.Windows.Forms.NumericUpDown();
            this.buttonStartServer = new System.Windows.Forms.Button();
            this.listBoxListaLog = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumPorta)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxIP
            // 
            this.textBoxIP.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxIP.Location = new System.Drawing.Point(12, 12);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(317, 38);
            this.textBoxIP.TabIndex = 0;
            this.textBoxIP.Text = "127.0.0.1";
            // 
            // numericUpDownNumPorta
            // 
            this.numericUpDownNumPorta.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.numericUpDownNumPorta.Location = new System.Drawing.Point(340, 13);
            this.numericUpDownNumPorta.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownNumPorta.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownNumPorta.Name = "numericUpDownNumPorta";
            this.numericUpDownNumPorta.Size = new System.Drawing.Size(120, 38);
            this.numericUpDownNumPorta.TabIndex = 1;
            this.numericUpDownNumPorta.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
            this.buttonStartServer.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.buttonStartServer.Location = new System.Drawing.Point(470, 13);
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.Size = new System.Drawing.Size(318, 41);
            this.buttonStartServer.TabIndex = 2;
            this.buttonStartServer.Text = "Start Server";
            this.buttonStartServer.UseVisualStyleBackColor = true;
            this.buttonStartServer.Click += new System.EventHandler(this.buttonStartServer_Click);
            // 
            // listBoxListaLog
            // 
            this.listBoxListaLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
            this.listBoxListaLog.FormattingEnabled = true;
            this.listBoxListaLog.ItemHeight = 25;
            this.listBoxListaLog.Location = new System.Drawing.Point(12, 66);
            this.listBoxListaLog.Name = "listBoxListaLog";
            this.listBoxListaLog.Size = new System.Drawing.Size(776, 354);
            this.listBoxListaLog.TabIndex = 3;
            // 
            // FormServidorChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listBoxListaLog);
            this.Controls.Add(this.buttonStartServer);
            this.Controls.Add(this.numericUpDownNumPorta);
            this.Controls.Add(this.textBoxIP);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "FormServidorChat";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SERVIDOR DE CHAT";
            this.Load += new System.EventHandler(this.FormServidorChat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumPorta)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.NumericUpDown numericUpDownNumPorta;
        private System.Windows.Forms.Button buttonStartServer;
        private System.Windows.Forms.ListBox listBoxListaLog;
    }
}

