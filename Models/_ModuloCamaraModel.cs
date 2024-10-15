using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpslogcamara.Models
{
    internal class _ModuloCamaraModel
    {
        public int? code { get; set; } = null;
        public string? message { get; set; } = null;
        public resultCamaraModel result { get; set; } = new resultCamaraModel();
        public object? data { get; set; } = null;

    }

    public class _ModuloCamaraModelArr
    {
        public int? code { get; set; } = null;
        public string? message { get; set; } = null;
        public List<resultDeviceLocationGetModel> result { get; set; } = new List<resultDeviceLocationGetModel>();
        public object? data { get; set; } = null;
    }

    public class resultCamaraModel
    {
        //AcessToken y RefreshToken
        public string? appKey { get; set; } = null;
        public string? account { get; set; } = null;
        public string? accessToken { get; set; } = null;
        public string? refreshToken { get; set; } = null;
        public int? expiresIn { get; set; } = null;
        public string? time { get; set; } = null;
    }

    public class resultDeviceLocationGetModel
    {
        public string? imei { get; set; } = null;
        public string? deviceName { get; set; } = null;
        public string? icon { get; set; } = null;
        public string? status { get; set; } = null;
        public string? posType { get; set; } = null;
        public double? lat { get; set; } = null;
        public double? lng { get; set; } = null;
        public string? hbTime { get; set; } = null;
        public string? accStatus { get; set; } = null;
        public string? gpsSignal { get; set; } = null;
        public string? powerValue { get; set; } = null;
        public string? batteryPowerVal { get; set; } = null;
        public string? speed { get; set; } = null;
        public string? gpsNum { get; set; } = null;
        public string? gpsTime { get; set; } = null;
        public string? direction { get; set; } = null;
        public string? activationFlag { get; set; } = null;
        public string? expireFlag { get; set; } = null;
        public string? electQuantity { get; set; } = null;
        public string? locDesc { get; set; } = null;
        public string? distance { get; set; } = null;
        public string? temperature { get; set; } = null;
        public string? trackerOil { get; set; } = null;
        public string? currentMileage { get; set; } = null;
    }
}
