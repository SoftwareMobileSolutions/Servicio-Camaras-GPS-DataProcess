using gpslogcamara.Data;
using gpslogcamara.Interfaces;
using gpslogcamara.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })

    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Leer valores de tiempo de appsettings.json
        int tiempoudp_request = configuration.GetValue<int>("tiempoudp_request");
        int tiempoapigps_request = configuration.GetValue<int>("tiempoapigps_request");

        services.AddScoped<SqlCnConfigMain>(provider =>
        {
            string connectionString = configuration.GetConnectionString("conexion")!;
            return new SqlCnConfigMain(connectionString);
        });
        //Para manejar la API de JIMI IOT
        services.AddScoped<_IModuloCamara, _ModuloCamaraService>();
        services.AddScoped<IImeis, ImeisService>();
        services.AddScoped<IDatosGPSCamara, DatosGPSCamaraService>();
        services.AddScoped<CameraApiManagerServices>();

        //Para enviar las tramas en UDP
        services.AddScoped<ICamaraLogGPS, CamaraLogGPSService>();
        services.AddScoped<CameraUDPCommManagerService>();

        // Registrar los tiempos leídos
        services.AddSingleton(new TimersConfig 
        { 
            TiempoUDPRequest = tiempoudp_request, 
            TiempoApiGpsRequest = tiempoapigps_request 
        });

    })
    .Build();

// Obtener la configuración de tiempos
var timersConfig = host.Services.GetRequiredService<TimersConfig>();

// Obtener la instancia de CameraApiManagerServices
var cameraApiManagerService = host.Services.GetRequiredService<CameraApiManagerServices>();
var CameraUDPCommManagerService = host.Services.GetRequiredService<CameraUDPCommManagerService>();

//1. Obtiene datos de la API
_=Task.Run(async () =>
{
    while (true)
    {
        try
        {
            await cameraApiManagerService.getGpsData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" +  $"Error en getGpsData: {ex.Message}");
        }
        await Task.Delay(TimeSpan.FromMinutes(timersConfig.TiempoApiGpsRequest));
    }
});

//2. Procesa datos para enviarlos a UDP
_=Task.Run(async () =>
{
    while (true)
    {
        try
        {
            await CameraUDPCommManagerService.procesarTrama();
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" +  $"Error en procesarTrama: {ex.Message}");
        }
        await Task.Delay(TimeSpan.FromMinutes(timersConfig.TiempoUDPRequest));
    }
});

//Mantener el programa en ejecución
await Task.Delay(Timeout.Infinite);

public class TimersConfig
{
    public int TiempoUDPRequest { get; set; }
    public int TiempoApiGpsRequest { get; set; }
}