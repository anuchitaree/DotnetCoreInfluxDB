using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Text.Json;


namespace DotnetCoreInfluxDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static readonly string url = "http://localhost:8086"; // Change to your InfluxDB URL
        private static readonly string token = "NvFCvRuTTjFK31vTAhBNR-3uU05mj3DGb4BdOa1ii8XqlwNICwOGHXxU6ninOYTdUJNqeou6OCqBQ1gOFP-KkA==";
        private static readonly string org = "organization";
        private static readonly string bucket = "result";

        private void button1_Click(object sender, EventArgs e)
        {
            using var client = new InfluxDBClient(url, token);

            WriteData(client);

        }
        private async void WriteData(InfluxDBClient client)
        {
            try
            {
                var now = DateTime.Now;
                long unixTimestampSecond = ((DateTimeOffset)now).ToUnixTimeSeconds();
                long unixTimestampMilliseconds = ((DateTimeOffset)now).ToUnixTimeMilliseconds();

                var option = new InfluxDBClientOptions.Builder()
               .Url(url)
               .AuthenticateToken(token)
               .Org(org)
               .Build();

                var influxDBClient = new InfluxDBClient(option);

                Random random = new Random();

                int temperature = random.Next(25, 35);

                int humidity = random.Next(50, 100);

                var point = PointData.Measurement("weather")
                   .Tag("location", "Phuket")
                   .Tag("device_id", "D05")
                   .Tag("sensor_type", "thermometer")
                   .Field("temperature", temperature)
                   .Field("humidity", humidity)
                   .Timestamp(unixTimestampMilliseconds, WritePrecision.Ns);

                await influxDBClient.GetWriteApiAsync().WritePointAsync(point, bucket, org);

                influxDBClient.Dispose();
                Console.WriteLine("✅ Data Written to InfluxDB");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
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
            await ReadAllData();
        }


        private static async Task ListMeasurements(InfluxDBClient client)
        {
            try
            {
                var option = new InfluxDBClientOptions.Builder()
                  .Url(url)
                  .AuthenticateToken(token)
                  .Org(org)
                  .Build();

                var influxDBClient = new InfluxDBClient(option);

                string fluxQuery = $"from(bucket: \"{bucket}\") |> range(start: -1h)";
                var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(fluxQuery, org);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
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
        private static async Task ReadAllData2()
        {

            //using var client = new InfluxDBClient(url, token);

            //// สร้าง Queryable
            //var queryable = client.GetQueryApi().QueryAsync<WeatherData>(org);

            //// Query ข้อมูลทั้งหมด
            //var allData = await queryable.ToListAsync();
            //Console.WriteLine("== ข้อมูลทั้งหมด ==");
            //allData.ForEach(data =>
            //    Console.WriteLine($"{data.Time}: {data.Location} - Temp: {data.Temperature}, Humidity: {data.Humidity}")
            //);

            //// Query ข้อมูลเฉพาะ Location "Bangkok" ที่มีอุณหภูมิ > 30 องศา
            //var filteredData = await queryable
            //    .Where(w => w.Location == "Bangkok" && w.Temperature > 30)
            //    .ToListAsync();

            //Console.WriteLine("== ข้อมูลที่ Filtered ==");
            //filteredData.ForEach(data =>
            //    Console.WriteLine($"{data.Time}: {data.Location} - Temp: {data.Temperature}, Humidity: {data.Humidity}")
            //);

        }


        private static async Task ReadAllData()
        {

            try
            {
                var option = new InfluxDBClientOptions.Builder()
                  .Url(url)
                  .AuthenticateToken(token)
                  .Org(org)
                  .Build();

                var influxDBClient = new InfluxDBClient(option);

                string fluxQuery = $@"
                                   from(bucket: ""result"")  
                                   |> range(start: -inf,stop:now())
                                   |> filter(fn: (r) => r._measurement == ""weather"")
                                   |> filter(fn: (r) => r.sensor_type == thermometer)
                ";

                var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(fluxQuery, org);


                Console.WriteLine("📌 Retrieved Data:");
                string records = "";

                foreach (var table in fluxTables)
                {

                    foreach (var record in table.Records)
                    {
                        //Console.WriteLine($"Time: {record.GetTime()}, Measurement: {record.GetMeasurement}, Field: {record.GetField}, Value: {record.GetValue()}");
                        records += $"Time:{record.GetTime()}, Measurement: {record.GetMeasurement}, Field: {record.GetField}, Value:{record.GetValue()}";

                        records += JsonSerializer.Serialize(record.Values);
                        records += "\n";
                    }
                }

                influxDBClient.Dispose();

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            using var client = new InfluxDBClient(url, token);


            await ListMeasurements(client);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            await healthCheck();

        }

        private async Task healthCheck()
        {
            var option = new InfluxDBClientOptions.Builder()
                .Url(url)
                .AuthenticateToken(token)
                .Org(org)
                .Build();
            //using var influxDBClient = new InfluxDBClient(url, token);

            var influxDBClient = new InfluxDBClient(option);
            try

            {
                var health = await influxDBClient.ReadyAsync();

                if (health.Status == Ready.StatusEnum.Ready)
                {
                    Console.WriteLine("Successfully connected to InfluxDB!");
                }
                else
                {
                    Console.WriteLine($"InfluxDB health check failed: {health}");
                }
                influxDBClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to InfluxDB: {ex.Message}");
            }

        }

    }

}
