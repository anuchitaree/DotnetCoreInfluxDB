using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;


namespace DotnetCoreInfluxDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static readonly string url = "http://localhost:8086"; // Change to your InfluxDB URL
        private static readonly string token = "your-influxdb-token";
        private static readonly string org = "your-org";
        private static readonly string bucket = "your-bucket";

        private async void button1_Click(object sender, EventArgs e)
        {
            using var client = new InfluxDBClient(url, token);

            await WriteData(client);
            //await ReadData(client);
        }
        private static async Task WriteData(InfluxDBClient client)
        {
            var writeApi = client.GetWriteApiAsync();

            var point = PointData.Measurement("temperature")
                .Tag("location", "office")
                .Field("value", 25.5)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(point, bucket, org);

            Console.WriteLine("✅ Data Written to InfluxDB");
        }

        // ✅ Read data from InfluxDB
        private static async Task ReadData(InfluxDBClient client)
        {
            var queryApi = client.GetQueryApi();
            string fluxQuery = $"from(bucket: \"{bucket}\") |> range(start: -1h) |> filter(fn: (r) => r._measurement == \"temperature\")";

            var tables = await queryApi.QueryAsync(fluxQuery, org);

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    Console.WriteLine($"📌 Time: {record.GetTime()}; Value: {record.GetValueByKey("_value")}");
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            using var client = new InfluxDBClient(url, token);


            await ReadData(client);
        }


        private static async Task ListMeasurements(InfluxDBClient client)
        {
            var queryApi = client.GetQueryApi();
            string fluxQuery = @"import ""influxdata/influxdb/schema""
                         schema.measurements(bucket: ""your-bucket"")";

            var tables = await queryApi.QueryAsync(fluxQuery, "your-org");

            Console.WriteLine("📌 Available Measurements:");
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    Console.WriteLine($"- {record.GetValue()}");
                }
            }
        }
        private static async Task ListFields(InfluxDBClient client, string measurement)
        {
            var queryApi = client.GetQueryApi();
            string fluxQuery = $@"import ""influxdata/influxdb/schema""
                          schema.fields(bucket: ""your-bucket"", measurement: ""{measurement}"")";

            var tables = await queryApi.QueryAsync(fluxQuery, "your-org");

            Console.WriteLine($"📌 Fields in {measurement}:");
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    Console.WriteLine($"- {record.GetValue()}");
                }
            }
        }
        private static async Task ReadAllData(InfluxDBClient client)
        {
            var queryApi = client.GetQueryApi();
            string fluxQuery = @"from(bucket: ""your-bucket"") 
                         |> range(start: -1h)"; // Adjust range as needed

            var tables = await queryApi.QueryAsync(fluxQuery, "your-org");

            Console.WriteLine("📌 Retrieved Data:");
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    Console.WriteLine($"Time: {record.GetTime()}, Measurement: {record.GetMeasurement}, Field: {record.GetField}, Value: {record.GetValue()}");
                }
            }
        }






    }
    
}
