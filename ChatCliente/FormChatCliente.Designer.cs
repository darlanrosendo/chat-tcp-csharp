namespace ChatCliente
{
    partial class FormChatCliente
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
            this.textBoxServidorIP = new System.Windows.Forms.TextBox();
            this.numericUpDownPortaHost = new System.Windows.Forms.NumericUpDown();
            this.textBoxUsuario = new System.Windows.Forms.TextBox();
            this.buttonConectar = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.textBoxMensagem = new System.Windows.Forms.TextBox();
            this.buttonEnviar = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortaHost)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxServidorIP
            // 
            this.textBoxServidorIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxServidorIP.Location = new System.Drawing.Point(12, 12);
            this.textBoxServidorIP.Name = "textBoxServidorIP";
            this.textBoxServidorIP.Size = new System.Drawing.Size(428, 38);
            this.textBoxServidorIP.TabIndex = 0;
            this.textBoxServidorIP.Text = "127.0.0.1";
            // 
            // numericUpDownPortaHost
            // 
            this.numericUpDownPortaHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.numericUpDownPortaHost.Location = new System.Drawing.Point(446, 13);
            this.numericUpDownPortaHost.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownPortaHost.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownPortaHost.Name = "numericUpDownPortaHost";
            this.numericUpDownPortaHost.Size = new System.Drawing.Size(152, 38);
            this.numericUpDownPortaHost.TabIndex = 1;
            this.numericUpDownPortaHost.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // textBoxUsuario
            // 
            this.textBoxUsuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxUsuario.Location = new System.Drawing.Point(12, 66);
            this.textBoxUsuario.Name = "textBoxUsuario";
            this.textBoxUsuario.Size = new System.Drawing.Size(428, 38);
            this.textBoxUsuario.TabIndex = 2;
            this.textBoxUsuario.Text = "Visitante";
            // 
            // buttonConectar
            // 
            this.buttonConectar.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
            this.buttonConectar.ForeColor = System.Drawing.Color.Green;
            this.buttonConectar.Location = new System.Drawing.Point(446, 66);
            this.buttonConectar.Name = "buttonConectar";
            this.buttonConectar.Size = new System.Drawing.Size(152, 38);
            this.buttonConectar.TabIndex = 3;
            this.buttonConectar.Text = "Conectar";
            this.buttonConectar.UseVisualStyleBackColor = true;
            this.buttonConectar.Click += new System.EventHandler(this.buttonConectar_Click_1);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxLog.Location = new System.Drawing.Point(12, 120);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(586, 390);
            this.textBoxLog.TabIndex = 4;
            // 
            // textBoxMensagem
            // 
            this.textBoxMensagem.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold);
            this.textBoxMensagem.Location = new System.Drawing.Point(12, 527);
            this.textBoxMensagem.Name = "textBoxMensagem";
            this.textBoxMensagem.Size = new System.Drawing.Size(428, 38);
            this.textBoxMensagem.TabIndex = 5;
            this.textBoxMensagem.Text = "Visitante";
            // 
            // buttonEnviar
            // 
            this.buttonEnviar.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
            this.buttonEnviar.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.buttonEnviar.Location = new System.Drawing.Point(446, 527);
            this.buttonEnviar.Name = "buttonEnviar";
            this.buttonEnviar.Size = new System.Drawing.Size(152, 38);
            this.buttonEnviar.TabIndex = 6;
            this.buttonEnviar.Text = "Enviar";
            this.buttonEnviar.UseVisualStyleBackColor = true;
            // 
            // labelStatus
            // 
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelStatus.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelStatus.Location = new System.Drawing.Point(79, 572);
            this.labelStatus.MaximumSize = new System.Drawing.Size(464, 70);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(464, 70);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "Não Conectado";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormChatCliente
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 651);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonEnviar);
            this.Controls.Add(this.textBoxMensagem);
            this.Controls.Add(this.buttonConectar);
            this.Controls.Add(this.textBoxUsuario);
            this.Controls.Add(this.numericUpDownPortaHost);
            this.Controls.Add(this.textBoxServidorIP);
            this.Controls.Add(this.textBoxLog);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormChatCliente";
            this.ShowIcon = false;
            this.Text = "Chat Cliente";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPortaHost)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxServidorIP;
        private System.Windows.Forms.NumericUpDown numericUpDownPortaHost;
        private System.Windows.Forms.TextBox textBoxUsuario;
        private System.Windows.Forms.Button buttonConectar;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TextBox textBoxMensagem;
        private System.Windows.Forms.Button buttonEnviar;
        private System.Windows.Forms.Label labelStatus;
    }
}

