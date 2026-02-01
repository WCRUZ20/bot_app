
namespace botapp.Interfaces.Secundarias
{
    partial class CargaBotInterface
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCargar = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnAbrirLog = new System.Windows.Forms.Button();
            this.lblableWrite = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblEstado = new System.Windows.Forms.Label();
            this.numIntervalo = new System.Windows.Forms.NumericUpDown();
            this.chkToggleAutoCarga = new System.Windows.Forms.CheckBox();
            this.grdClientes = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIntervalo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdClientes)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.grdClientes, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(40, 20, 40, 20);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1198, 620);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblableWrite, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(43, 23);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1112, 110);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(490, 55);
            this.label1.TabIndex = 0;
            this.label1.Text = "PROCESO CARGA CLAVES DE ACCESO A EDOC";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(559, 58);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(550, 49);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.btnCargar);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(278, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(20);
            this.panel1.Size = new System.Drawing.Size(269, 43);
            this.panel1.TabIndex = 0;
            // 
            // btnCargar
            // 
            this.btnCargar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.btnCargar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCargar.FlatAppearance.BorderSize = 0;
            this.btnCargar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnCargar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCargar.ForeColor = System.Drawing.Color.White;
            this.btnCargar.Location = new System.Drawing.Point(20, 20);
            this.btnCargar.Name = "btnCargar";
            this.btnCargar.Size = new System.Drawing.Size(229, 3);
            this.btnCargar.TabIndex = 0;
            this.btnCargar.Text = "CARGAR 🔼";
            this.btnCargar.UseVisualStyleBackColor = false;
            this.btnCargar.Click += new System.EventHandler(this.btnCargar_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnAbrirLog);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(20);
            this.panel2.Size = new System.Drawing.Size(269, 43);
            this.panel2.TabIndex = 1;
            // 
            // btnAbrirLog
            // 
            this.btnAbrirLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnAbrirLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAbrirLog.FlatAppearance.BorderSize = 0;
            this.btnAbrirLog.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnAbrirLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrirLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbrirLog.ForeColor = System.Drawing.Color.White;
            this.btnAbrirLog.Location = new System.Drawing.Point(20, 20);
            this.btnAbrirLog.Name = "btnAbrirLog";
            this.btnAbrirLog.Size = new System.Drawing.Size(229, 3);
            this.btnAbrirLog.TabIndex = 0;
            this.btnAbrirLog.Text = "ABRIR LOG 🧾";
            this.btnAbrirLog.UseVisualStyleBackColor = false;
            this.btnAbrirLog.Click += new System.EventHandler(this.btnAbrirLog_Click);
            // 
            // lblableWrite
            // 
            this.lblableWrite.AutoSize = true;
            this.lblableWrite.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblableWrite.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblableWrite.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblableWrite.Location = new System.Drawing.Point(1002, 0);
            this.lblableWrite.Name = "lblableWrite";
            this.lblableWrite.Size = new System.Drawing.Size(107, 55);
            this.lblableWrite.TabIndex = 2;
            this.lblableWrite.Text = "Sin conexión";
            this.lblableWrite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 58);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(550, 49);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblEstado);
            this.panel3.Controls.Add(this.numIntervalo);
            this.panel3.Controls.Add(this.chkToggleAutoCarga);
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(243, 43);
            this.panel3.TabIndex = 2;
            // 
            // lblEstado
            // 
            this.lblEstado.AutoSize = true;
            this.lblEstado.Location = new System.Drawing.Point(-3, 0);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(52, 17);
            this.lblEstado.TabIndex = 2;
            this.lblEstado.Text = "Estado";
            // 
            // numIntervalo
            // 
            this.numIntervalo.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numIntervalo.Location = new System.Drawing.Point(166, 15);
            this.numIntervalo.Name = "numIntervalo";
            this.numIntervalo.Size = new System.Drawing.Size(57, 25);
            this.numIntervalo.TabIndex = 1;
            // 
            // chkToggleAutoCarga
            // 
            this.chkToggleAutoCarga.AutoSize = true;
            this.chkToggleAutoCarga.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkToggleAutoCarga.Location = new System.Drawing.Point(0, 17);
            this.chkToggleAutoCarga.Name = "chkToggleAutoCarga";
            this.chkToggleAutoCarga.Size = new System.Drawing.Size(139, 23);
            this.chkToggleAutoCarga.TabIndex = 0;
            this.chkToggleAutoCarga.Text = "Carga automática";
            this.chkToggleAutoCarga.UseVisualStyleBackColor = true;
            this.chkToggleAutoCarga.CheckedChanged += new System.EventHandler(this.chkToggleAutoCarga_CheckedChanged);
            // 
            // grdClientes
            // 
            this.grdClientes.BackgroundColor = System.Drawing.Color.White;
            this.grdClientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdClientes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdClientes.Location = new System.Drawing.Point(43, 139);
            this.grdClientes.Name = "grdClientes";
            this.grdClientes.RowHeadersWidth = 51;
            this.grdClientes.RowTemplate.Height = 24;
            this.grdClientes.Size = new System.Drawing.Size(1112, 458);
            this.grdClientes.TabIndex = 1;
            // 
            // CargaBotInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CargaBotInterface";
            this.Size = new System.Drawing.Size(1198, 620);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIntervalo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdClientes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView grdClientes;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCargar;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnAbrirLog;
        private System.Windows.Forms.Label lblableWrite;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox chkToggleAutoCarga;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.NumericUpDown numIntervalo;
    }
}
