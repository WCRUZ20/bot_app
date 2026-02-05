namespace botapp.Interfaces.Secundarias
{
    partial class ConfServicioInterface
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelFrecuencia = new System.Windows.Forms.Label();
            this.nudFrecuencia = new System.Windows.Forms.NumericUpDown();
            this.chkServicioActivo = new System.Windows.Forms.CheckBox();
            this.chkCargaAutomatica = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelReporte = new System.Windows.Forms.Label();
            this.lblRutaReporte = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrecuencia)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblTitulo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDescripcion, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkServicioActivo, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.chkCargaAutomatica, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(24, 20, 24, 20);
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(675, 488);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTitulo.Location = new System.Drawing.Point(26, 20);
            this.lblTitulo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(623, 34);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Configuración Servicio";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblDescripcion.Location = new System.Drawing.Point(26, 54);
            this.lblDescripcion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(623, 52);
            this.lblDescripcion.TabIndex = 1;
            this.lblDescripcion.Text = "Configura cada cuántos minutos se ejecutan los procesos de descarga. Al finalizar" +
    " cada descarga, la carga se ejecutará de forma automática.";
            this.lblDescripcion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.labelFrecuencia, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.nudFrecuencia, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(26, 108);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(623, 53);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // labelFrecuencia
            // 
            this.labelFrecuencia.AutoSize = true;
            this.labelFrecuencia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelFrecuencia.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.labelFrecuencia.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.labelFrecuencia.Location = new System.Drawing.Point(2, 0);
            this.labelFrecuencia.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelFrecuencia.Name = "labelFrecuencia";
            this.labelFrecuencia.Size = new System.Drawing.Size(161, 53);
            this.labelFrecuencia.TabIndex = 0;
            this.labelFrecuencia.Text = "Frecuencia (minutos)";
            this.labelFrecuencia.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudFrecuencia
            // 
            this.nudFrecuencia.Dock = System.Windows.Forms.DockStyle.Left;
            this.nudFrecuencia.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.nudFrecuencia.Location = new System.Drawing.Point(168, 3);
            this.nudFrecuencia.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.nudFrecuencia.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudFrecuencia.Name = "nudFrecuencia";
            this.nudFrecuencia.Size = new System.Drawing.Size(77, 26);
            this.nudFrecuencia.TabIndex = 1;
            this.nudFrecuencia.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // chkServicioActivo
            // 
            this.chkServicioActivo.AutoSize = true;
            this.chkServicioActivo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkServicioActivo.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.chkServicioActivo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.chkServicioActivo.Location = new System.Drawing.Point(26, 165);
            this.chkServicioActivo.Margin = new System.Windows.Forms.Padding(2);
            this.chkServicioActivo.Name = "chkServicioActivo";
            this.chkServicioActivo.Size = new System.Drawing.Size(623, 32);
            this.chkServicioActivo.TabIndex = 3;
            this.chkServicioActivo.Text = "Servicio activo";
            this.chkServicioActivo.UseVisualStyleBackColor = true;
            // 
            // chkCargaAutomatica
            // 
            this.chkCargaAutomatica.AutoSize = true;
            this.chkCargaAutomatica.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCargaAutomatica.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.chkCargaAutomatica.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.chkCargaAutomatica.Location = new System.Drawing.Point(26, 201);
            this.chkCargaAutomatica.Margin = new System.Windows.Forms.Padding(2);
            this.chkCargaAutomatica.Name = "chkCargaAutomatica";
            this.chkCargaAutomatica.Size = new System.Drawing.Size(623, 37);
            this.chkCargaAutomatica.TabIndex = 4;
            this.chkCargaAutomatica.Text = "Ejecutar carga automáticamente al finalizar la descarga";
            this.chkCargaAutomatica.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.labelReporte, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblRutaReporte, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(26, 242);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(623, 224);
            this.tableLayoutPanel3.TabIndex = 4;
            // 
            // labelReporte
            // 
            this.labelReporte.AutoSize = true;
            this.labelReporte.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelReporte.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.labelReporte.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.labelReporte.Location = new System.Drawing.Point(2, 0);
            this.labelReporte.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelReporte.Name = "labelReporte";
            this.labelReporte.Size = new System.Drawing.Size(619, 29);
            this.labelReporte.TabIndex = 0;
            this.labelReporte.Text = "Reporte PDF por cliente";
            this.labelReporte.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRutaReporte
            // 
            this.lblRutaReporte.AutoSize = true;
            this.lblRutaReporte.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRutaReporte.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblRutaReporte.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblRutaReporte.Location = new System.Drawing.Point(2, 29);
            this.lblRutaReporte.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRutaReporte.Name = "lblRutaReporte";
            this.lblRutaReporte.Size = new System.Drawing.Size(619, 195);
            this.lblRutaReporte.TabIndex = 1;
            this.lblRutaReporte.Text = "Se generará un PDF con el estado de descarga (mes y tipo de documento) y el estad" +
    "o de carga con fecha y hora. Por ahora se guardará en la ruta raíz donde se crea" +
    "n los directorios de clientes y log.";
            // 
            // ConfServicioInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ConfServicioInterface";
            this.Size = new System.Drawing.Size(675, 488);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrecuencia)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelFrecuencia;
        private System.Windows.Forms.NumericUpDown nudFrecuencia;
        private System.Windows.Forms.CheckBox chkServicioActivo;
        private System.Windows.Forms.CheckBox chkCargaAutomatica;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label labelReporte;
        private System.Windows.Forms.Label lblRutaReporte;

    }
}
