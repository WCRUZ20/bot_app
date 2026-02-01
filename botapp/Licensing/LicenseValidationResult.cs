using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Licensing
{
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
        public string AbleWrite { get; set; }

        public static LicenseValidationResult Invalid(string message)
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                IsActive = false,
                Message = message ?? "Licencia inválida.",
                AbleWrite = "N"
            };
        }
    }
}
