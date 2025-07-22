namespace SimuladorModbus
{
    partial class Principal
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_PortaCOM = new System.Windows.Forms.ComboBox();
            this.comboBox_BaudRate = new System.Windows.Forms.ComboBox();
            this.button_CriarCOM = new System.Windows.Forms.Button();
            this.textBox_StatusCOM = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "PortaCOM";
            // 
            // comboBox_PortaCOM
            // 
            this.comboBox_PortaCOM.FormattingEnabled = true;
            this.comboBox_PortaCOM.Location = new System.Drawing.Point(87, 11);
            this.comboBox_PortaCOM.Name = "comboBox_PortaCOM";
            this.comboBox_PortaCOM.Size = new System.Drawing.Size(81, 24);
            this.comboBox_PortaCOM.TabIndex = 1;
            this.comboBox_PortaCOM.SelectedIndexChanged += new System.EventHandler(this.comboBox_PortaCOM_SelectedIndexChanged);
            // 
            // comboBox_BaudRate
            // 
            this.comboBox_BaudRate.FormattingEnabled = true;
            this.comboBox_BaudRate.Location = new System.Drawing.Point(174, 11);
            this.comboBox_BaudRate.Name = "comboBox_BaudRate";
            this.comboBox_BaudRate.Size = new System.Drawing.Size(81, 24);
            this.comboBox_BaudRate.TabIndex = 2;
            // 
            // button_CriarCOM
            // 
            this.button_CriarCOM.Location = new System.Drawing.Point(261, 11);
            this.button_CriarCOM.Name = "button_CriarCOM";
            this.button_CriarCOM.Size = new System.Drawing.Size(75, 24);
            this.button_CriarCOM.TabIndex = 3;
            this.button_CriarCOM.Text = "Conectar";
            this.button_CriarCOM.UseVisualStyleBackColor = true;
            this.button_CriarCOM.Click += new System.EventHandler(this.button_Conectar_Click);
            // 
            // textBox_StatusCOM
            // 
            this.textBox_StatusCOM.Location = new System.Drawing.Point(15, 90);
            this.textBox_StatusCOM.Multiline = true;
            this.textBox_StatusCOM.Name = "textBox_StatusCOM";
            this.textBox_StatusCOM.Size = new System.Drawing.Size(321, 146);
            this.textBox_StatusCOM.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(66, 51);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(189, 24);
            this.comboBox1.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Dados";
            // 
            // Principal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 248);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox_StatusCOM);
            this.Controls.Add(this.button_CriarCOM);
            this.Controls.Add(this.comboBox_BaudRate);
            this.Controls.Add(this.comboBox_PortaCOM);
            this.Controls.Add(this.label1);
            this.Name = "Principal";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Principal_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_PortaCOM;
        private System.Windows.Forms.ComboBox comboBox_BaudRate;
        private System.Windows.Forms.Button button_CriarCOM;
        private System.Windows.Forms.TextBox textBox_StatusCOM;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
    }
}

