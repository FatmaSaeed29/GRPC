namespace ITI.Grpc.Server.Services
{
    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public object Category { get; set; }
    }
}