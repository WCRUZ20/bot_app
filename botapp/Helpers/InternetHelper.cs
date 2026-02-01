using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Helpers
{
    class InternetHelper
    {
        public static bool HayConexionInternet(string host = "8.8.8.8", int timeout = 3000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, timeout);
                    return reply != null && reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
