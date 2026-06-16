namespace WaterMeterAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string MeterId { get; set; } = "";
    }
}