using InfluxDB.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetCoreInfluxDB
{
    [Measurement("weather")]
    public class WeatherData
    {
        [Column("temperature", IsTag = false)]
        public double Temperature { get; set; }

        [Column("humidity", IsTag = false)]
        public double Humidity { get; set; }

        [Column("location", IsTag = true)]
        public string? Location { get; set; }

        [Column("device_id", IsTag = true)]
        public string? DeviceId { get; set; }

        [Column(IsTimestamp = true)]
        public DateTime Time { get; set; }
    }
}
