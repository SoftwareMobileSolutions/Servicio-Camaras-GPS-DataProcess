using gpslogcamara.Interfaces;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace gpslogcamara.Services
{
    internal class CameraUDPCommManagerService
    {
        private readonly ICamaraLogGPS _ICamaraLogGPS;
        public CameraUDPCommManagerService(ICamaraLogGPS ICamaraLogGPS_)
        {
            _ICamaraLogGPS = ICamaraLogGPS_;
        }

        public async Task procesarTrama()
        {
            var d = await _ICamaraLogGPS.getCamaraGPSLog();
            string strTrama = "";
            if (d == null) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" +  "NO HAY NUEVOS DATOS PARA PROCESAR EN LA TABLA gps_log_camara");
            } else {
                var mensajes = d.FirstOrDefault()!;

                if (mensajes == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "NO HAY NUEVOS DATOS PARA PROCESAR EN LA TABLA gps_log_camara"); 
                } else
                {
                    if (mensajes.code != null && d.Count() == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "ERROR: CÓDIGO: " + mensajes.code + "|" + mensajes.msg);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "CÓDIGO: 0|Enviando datos [" + d.Count().ToString() +"] a UDP[56020]...");
                        int i = 0;
                        foreach (var datos in d)
                        {
                            i++;
                            strTrama = CrearStrTrama(i, datos.imei, datos.dategps, datos.lat, datos.lng, datos.speed, (int)Math.Round(datos.heading), datos.evento, datos.numsat, datos.accStatus, datos.gpsSignal, datos.distance, datos.batteryPowerVal);
                            if (strTrama == "-1")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "ERROR: Mientras se convertía la data al string de la trama");
                            }
                            else
                            {
                                await EnvioBKTrama(strTrama, "www.kontrol-gps.com", 56020);
                                await _ICamaraLogGPS.UpdateGPSCamaraData(datos.idlogcam);
                            }
                        }
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "CÓDIGO: 0|Datos Enviados por UDP exitosamente");
                    }
                }
                
            }
            Console.ResetColor();
        }

        public async Task EnvioBKTrama(string strTrama, string strURLDestino, int iPuerto)
        {
            try
            {
                using (UdpClient socketUDP = new UdpClient())
                {
                    byte[] frame = Encoding.ASCII.GetBytes(strTrama);
                    await socketUDP.SendAsync(frame, frame.Length, strURLDestino, iPuerto);
                }
            }
            catch(Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "ERROR:" + ex.Message);
            }
            
        }

        public string CrearStrTrama(long numeroTrama, string strIMEI, string strFechaGPS, double dLatitud, double dLongitud, double dVelocidad, int iHeading, int iEvento, int numberSatellite, bool accstatus/* IO*/, int smssignal /* gpsSignal CF 0 - 4 */, int odometro /* distance VO */, double batterypowerval  /*BL capacidad voltaje bateria */ )
        {
            try
            {
                if (string.IsNullOrEmpty(strFechaGPS))
                {
                    strFechaGPS = "0000-00-00 00:00:00";
                }

                //string strTrama = "$FNC=" + numeroTrama + ">REV";
                string strTrama = ">REV";
                string strEvento = iEvento.ToString();
                string strLongitud = "";
                string strLatitud = "";
                string strVelocidad = ((int)(dVelocidad / 1.6)).ToString(); // Km/h a Millas/h
                string strHeading = iHeading.ToString();
                string strSemanasTotal = "";
                  

                if (strEvento.Length < 2)
                {
                    strEvento = "0" + strEvento;
                }
                strTrama += strEvento;

                // Obteniendo Fecha
                DateTime fechaGPS = DateTime.ParseExact(strFechaGPS, "yyyy-MM-dd HH:mm:ss", null);
                DateTime fechaBase = new DateTime(1980, 1, 6, 0, 0, 0);
                DateTime horaGPS = DateTime.ParseExact("1980-01-06 " + strFechaGPS.Substring(11), "yyyy-MM-dd HH:mm:ss", null);

                long lSegundosTotal = (long)(fechaGPS - fechaBase).TotalSeconds;
                long lSemanasTotal = lSegundosTotal / (86400 * 7);
                long lSegundosTotalDia = (long)(horaGPS - fechaBase).TotalSeconds;
                string strSegundosTotalDia = lSegundosTotalDia.ToString();
                strSemanasTotal = lSemanasTotal.ToString("D4");

                strTrama += strSemanasTotal;
                strTrama += ((int)fechaGPS.DayOfWeek).ToString();

                strSegundosTotalDia = strSegundosTotalDia.PadLeft(5, '0');
                strTrama += strSegundosTotalDia;

                // Longitud
                string[] strALongitud = Math.Abs(dLongitud).ToString("F6", CultureInfo.InvariantCulture).Split('.');
                strALongitud[0] = strALongitud[0].PadLeft(3, '0'); // Asegurarse que la parte entera tenga 3 dígitos
                strALongitud[1] = strALongitud[1].PadRight(6, '0'); // Asegurarse que la parte decimal tenga 6 dígitos
                strLongitud = strALongitud[0] + strALongitud[1]; // Concatenar parte entera y decimal

                if (dLongitud > 0) strLongitud = "+" + strLongitud;
                else strLongitud = "-" + strLongitud;
                strLongitud = strLongitud.Substring(0, Math.Min(9, strLongitud.Length));


                // Latitud
                strLatitud = Math.Abs(dLatitud).ToString("F5", CultureInfo.InvariantCulture).Replace(".", "");
                if (dLatitud > 0) strLatitud = "+" + strLatitud;
                else strLatitud = "-" + strLatitud;
                strLatitud = strLatitud.Substring(0, Math.Min(8, strLatitud.Length));

                // Agregando a la trama
                strTrama += strLatitud;
                strTrama += strLongitud;

                strVelocidad = strVelocidad.PadLeft(3, '0');
                strTrama += strVelocidad.Substring(0, 3);

                strHeading = strHeading.PadLeft(3, '0');
                strTrama += strHeading.Substring(0, 3) + "12;";

                //Datos ****
                //IO
                int io_ = 0;
                int powerstatus = 1; //Cambiar cuando se conozca el poweron de la batería
                if (accstatus) {
                    io_+=100;
                }
                if (powerstatus == 1) {
                    io_+=200;
                }

                strTrama += "IO=" + (io_).ToString() + ";";

                // Número de satélites
                string numberSatelliteS = numberSatellite.ToString().PadLeft(2, '0');
                strTrama += "SV=" + numberSatelliteS + ";";

                // Batería
                string bl_ = (4000 * batterypowerval).ToString();
                strTrama += "BL=" + bl_ + ";";

                //intensidad de la señal
                string cf_ = ((int)(Math.Pow(2, smssignal + 1) - 1)).ToString();
                strTrama += "CF=000,0000," + cf_ + ";";

                //odometro
                strTrama += "VO=" + odometro.ToString() + ";";

                // Estado de batería y uso de datos (simulación)


                /* string levelBattery = "00000"; // Simulación, cambiar por el valor real
                 string dataUsage = "00000"; // Simulación, cambiar por el valor real
                 strTrama += "CV90=" + levelBattery + ";";
                 strTrama += "CV91=" + dataUsage + ";";
                 */
                // IMEI
                strTrama += "ID=" + strIMEI + "<\r\n";

                return strTrama;
                
            }
            catch (Exception e)
            {
                // Manejo de errores
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "/" + "Error: " + e.Message);
                return "-1";
            }
        }
    }
}
