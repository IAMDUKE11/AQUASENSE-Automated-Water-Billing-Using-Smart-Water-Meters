namespace WaterMeterAPI.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public string MeterId { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}