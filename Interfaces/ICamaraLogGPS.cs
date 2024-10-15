using gpslogcamara.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpslogcamara.Interfaces
{
    internal interface ICamaraLogGPS
    {
        Task<IEnumerable<CamaraLogGPSModel>> getCamaraGPSLog();
        Task<IEnumerable<mensajesModel>> UpdateGPSCamaraData(Int64 idlogcam);
    }
}

