using gpslogcamara.Models;

namespace gpslogcamara.Interfaces
{
    internal interface IDatosGPSCamara
    {
        Task<IEnumerable<mensajesModel>> SetGPSCamaraData(string data);
    }
}
