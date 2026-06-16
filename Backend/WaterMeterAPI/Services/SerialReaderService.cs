using System.IO.Ports;
using Microsoft.Extensions.Hosting;
using WaterMeterAPI.Data;
using WaterMeterAPI.Models;

namespace WaterMeterAPI.Services
{
    public class SerialReaderService : IHostedService
    {
        private SerialPort? _port;
        private readonly FirebirdHelper _db;
        private const string MeterId = "WM-001-2024";

        private double _lastSavedLiters = -1;
        private DateTime _lastSavedTime = DateTime.MinValue;
        private const int MinSecondsBetweenSaves = 30;

        public SerialReaderService(FirebirdHelper db)
        {
            _db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _port = new SerialPort("COM9", 115200);
                _port.DataReceived += OnDataReceived;
                _port.Open();
                Console.WriteLine("Serial port opened.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Serial error: " + ex.Message);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_port?.IsOpen == true)
            {
                _port.Close();
                Console.WriteLine("Serial port closed.");
            }
            return Task.CompletedTask;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _port!.ReadLine().Trim();
                Console.WriteLine("Received: " + line);

                // Final session record
                if (line.StartsWith("SESSION_END|"))
                {
                    string[] p = line.Split('|');
                    if (p.Length < 4) return;

                    double liters = Convert.ToDouble(p[2]);
                    double bill = Convert.ToDouble(p[3]);

                    if (liters <= 0) return;

                    _db.AddWaterReading(new WaterReading
                    {
                        MeterId = MeterId,
                        FlowRate = 0,
                        TotalLiters = liters,
                        Bill = bill,
                        ReadingTime = DateTime.Now
                    });

                    _lastSavedLiters = -1;
                    _lastSavedTime = DateTime.MinValue;
                    Console.WriteLine($"Session saved — {liters:F6} L | ${bill:F6}");
                    return;
                }

                // Live data while flowing
                if (!line.StartsWith("DATA|")) return;

                // Format: DATA|flowRate|totalLiters|bill|END
                string[] parts = line.Split('|');
                if (parts.Length < 4) return;

                double flow = Convert.ToDouble(parts[1]);
                double liveL = Convert.ToDouble(parts[2]);
                double liveBill = Convert.ToDouble(parts[3]);

                if (liveL <= 0) return;

                bool enoughTime = (DateTime.Now - _lastSavedTime).TotalSeconds >= MinSecondsBetweenSaves;

                if (!enoughTime)
                {
                    Console.WriteLine($"Skipped — {liveL:F6} L");
                    return;
                }

                _db.AddWaterReading(new WaterReading
                {
                    MeterId = MeterId,
                    FlowRate = flow,
                    TotalLiters = liveL,
                    Bill = liveBill,
                    ReadingTime = DateTime.Now
                });

                _lastSavedLiters = liveL;
                _lastSavedTime = DateTime.Now;
                Console.WriteLine($"Saved — {flow:F4} L/min | {liveL:F6} L | ${liveBill:F6}");

                if (liveL > 50)
                {
                    _db.AddAlert(new Alert
                    {
                        MeterId = MeterId,
                        AlertType = "High Usage",
                        Message = $"High usage: {liveL:F1} liters",
                        CreatedAt = DateTime.Now
                    });
                    Console.WriteLine("Alert saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parse error: " + ex.Message);
            }
        }
    }
}