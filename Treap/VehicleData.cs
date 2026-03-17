using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    public class VehicleData
    {
        public string LicensePlate { get; set; }
        public string OwnerName { get; set; }

        public VehicleData(string licensePlate, string ownerName)
        {
            LicensePlate = licensePlate;
            OwnerName = ownerName;
        }

        public override string ToString()
        {
            return $"SPZ: {LicensePlate}, Majitel: {OwnerName}";
        }
    }
}
