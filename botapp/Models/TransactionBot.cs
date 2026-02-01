using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Models
{
    public class TransactionBot
    {
        public string id_transaction { get; set; }
        public string transaction_order { get; set; }
        public string resolution_provider { get; set; }
        public string id_cliente { get; set; }
        public string tipodocumento { get; set; }
        public string mes_consulta { get; set; }
        public string estado { get; set; }
        public string observacion { get; set; }
        public string saldo { get; set; }
        public string trans_type { get; set; }
    }
}
