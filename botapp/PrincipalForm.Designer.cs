
namespace botapp
{
    partial class PrincipalForm
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

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrincipalForm));
            this.pnlprincipal = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlprincipal
            // 
            this.pnlprincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlprincipal.Location = new System.Drawing.Point(0, 0);
            this.pnlprincipal.Name = "pnlprincipal";
            this.pnlprincipal.Size = new System.Drawing.Size(1900, 981);
            this.pnlprincipal.TabIndex = 0;
            // 
            // PrincipalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1900, 981);
            this.Controls.Add(this.pnlprincipal);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrincipalForm";
            this.Text = "DOCSY";
            this.Load += new System.EventHandler(this.PrincipalForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlprincipal;
    }
}

