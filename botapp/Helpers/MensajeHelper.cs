using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Helpers
{
    public enum TipoMensaje
    {
        Exito,
        Error,
        Precaucion
    }

    public static class MensajeHelper
    {
        /// <summary>
        /// Muestra un mensaje en un label con color según el tipo y lo oculta después de 10 segundos.
        /// </summary>
        /// <param name="label">Label donde se mostrará el mensaje</param>
        /// <param name="mensaje">Texto del mensaje</param>
        /// <param name="tipo">Tipo de mensaje (Exito, Error, Precaucion)</param>
        public static async void Mostrar(Label label, string mensaje, TipoMensaje tipo)
        {
            if (label == null) return;
            label.ForeColor = Color.White;
            // Set color según tipo
            switch (tipo)
            {
                case TipoMensaje.Exito:
                    label.BackColor = Color.FromArgb(34, 197, 94);
                    break;
                case TipoMensaje.Error:
                    label.BackColor = Color.FromArgb(222, 4, 4);
                    break;
                case TipoMensaje.Precaucion:
                    label.BackColor = Color.FromArgb(239, 68, 68);
                    break;
            }

            // Mostrar mensaje
            label.Text = mensaje;
            label.Visible = true;

            // Esperar 10 segundos
            await Task.Delay(10000);

            // Ocultar mensaje
            label.Visible = false;
            label.Text = "";
        }
    }
}
