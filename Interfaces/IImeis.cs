using gpslogcamara.Models;


namespace gpslogcamara.Interfaces
{
    internal interface IImeis
    {
        Task<IEnumerable<ImeisModel>> ObtenerImeis();
    }
}
