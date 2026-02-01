using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Helpers
{
    public class ComicBubble : Panel
    {
        public string BubbleText { get; set; } = "Hola, soy una nube!";

        public ComicBubble()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Size = new Size(200, 100);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 20);

            using (GraphicsPath path = new GraphicsPath())
            {
                // Dibujar rectángulo redondeado
                int radio = 20;
                path.AddArc(rect.X, rect.Y, radio, radio, 180, 90);
                path.AddArc(rect.Right - radio, rect.Y, radio, radio, 270, 90);
                path.AddArc(rect.Right - radio, rect.Bottom - radio, radio, radio, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radio, radio, radio, 90, 90);
                path.CloseAllFigures();

                // Dibujar "pico" de la nube (triangulito abajo)
                Point[] triangulo = {
                new Point(this.Width/2 - 10, rect.Bottom),
                new Point(this.Width/2 + 10, rect.Bottom),
                new Point(this.Width/2, this.Height)
            };
                path.AddPolygon(triangulo);

                using (SolidBrush brush = new SolidBrush(Color.White))
                    g.FillPath(brush, path);

                using (Pen pen = new Pen(Color.Black, 2))
                    g.DrawPath(pen, path);
            }

            // Dibujar texto centrado
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(BubbleText, this.Font, Brushes.Black, rect, sf);
            }
        }
    }

}
