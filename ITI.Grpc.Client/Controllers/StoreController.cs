using Grpc.Core;
using Grpc.Net.Client;
using ITI.Grpc.Protos;
using Microsoft.AspNetCore.Mvc;

namespace ITI.Grpc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _grpcAddress;

        public StoreController(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["ApiKey"];
            _grpcAddress = "https://localhost:7214";
        }

        private GrpcChannel CreateGrpcChannel()
        {
            return GrpcChannel.ForAddress(_grpcAddress, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), CreateCallCredentials())
            });
        }

        private CallCredentials CreateCallCredentials()
        {
            return CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("x-api-key", _apiKey);
                return Task.CompletedTask;
            });
        }

        private InventoryServiceProto.InventoryServiceProtoClient CreateGrpcClient(GrpcChannel channel)
        {
            return new InventoryServiceProto.InventoryServiceProtoClient(channel);
        }

       
        [HttpPost]
        public async Task<ActionResult> AddProduct(Product product)
        {
            using var channel = CreateGrpcChannel();
            var client = CreateGrpcClient(channel);

            try
            {
                var productExists = await client.GetProductByIdAsync(new Id { Id_ = product.Id }, new CallOptions(credentials: CreateCallCredentials()));

                if (!productExists.IsExisted_)
                {
                    var addedProduct = await client.AddProductAsync(product, new CallOptions(credentials: CreateCallCredentials()));
                    return Created("Product Created", addedProduct);
                }

                var updatedProduct = await client.UpdateProductAsync(product, new CallOptions(credentials: CreateCallCredentials()));
                return Created("Product Updated", updatedProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding/updating product (ID: {product.Id}): {ex.Message}");
                return StatusCode(500, $"Error adding/updating product: {ex.Message}");
            }
        }

        [HttpPost("addproducts")]
        public async Task<ActionResult> AddBulkProducts(List<Product> products)
        {
            using var channel = CreateGrpcChannel();
            var client = CreateGrpcClient(channel);

            try
            {
                var call = client.AddBulkProducts(new CallOptions(credentials: CreateCallCredentials()));

                foreach (var product in products)
                {
                    await call.RequestStream.WriteAsync(product);
                    await Task.Delay(100);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding bulk products: {ex.Message}");
                return StatusCode(500, $"Error adding bulk products: {ex.Message}");
            }
        }

        [HttpGet("GetReport")]
        public async Task<ActionResult> GetReport()
        {
            using var channel = CreateGrpcChannel();
            var client = CreateGrpcClient(channel);

            var addedProducts = new List<Product>();

            try
            {
                var call = client.GetProductReport(new Google.Protobuf.WellKnownTypes.Empty(), new CallOptions(credentials: CreateCallCredentials()));

                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    addedProducts.Add(call.ResponseStream.Current);
                }

                return Ok(addedProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product report: {ex.Message}");
                return StatusCode(500, $"Error fetching product report: {ex.Message}");
            }
        }
    }
}
