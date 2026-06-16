namespace WaterMeterAPI.Models
{
    public class WaterReading
    {
        public int Id { get; set; }
        public string MeterId { get; set; } = "";
        public double FlowRate { get; set; }
        public double TotalLiters { get; set; }
        public double Bill { get; set; }
        public DateTime ReadingTime { get; set; }
    }
}
