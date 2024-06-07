using ITI.Grpc.Protos;
using static ITI.Grpc.Protos.InventoryServiceProto;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Grpc.Server.Services
{
    public class InventroyService : InventoryServiceProtoBase
    {
        public static List<Product> Products { get; set; }

        public InventroyService()
        {
            Products = new List<Product>()
            {
                new Product{Id=1,Name="T-shirt",Price=250,Quantity=12 , Category= ProductCategory.Clothes },
                new Product{Id=2,Name="TV",Price=22200,Quantity=3,  Category=ProductCategory.Electronics },
                new Product{Id=3,Name="Skirt",Price=560,Quantity=16 ,  Category=ProductCategory.Clothes},
            };
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<IsExisted> GetProductById(Id request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(i => i.Id == request.Id_);
            if (product != null)
            {
                return await Task.FromResult(new IsExisted
                {
                    IsExisted_ = true
                });
            }
            return await Task.FromResult(new IsExisted
            {
                IsExisted_ = false
            });
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<Product> AddProduct(Product request, ServerCallContext context)
        {
            Products.Add(request);
            return await Task.FromResult(request);
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<Product> UpdateProduct(Product request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(i => i.Id == request.Id);
            if (product != null)
            {
                product.Name = request.Name;
                product.Price = request.Price;
                product.Quantity = request.Quantity;
            }

            return await Task.FromResult(product);
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task<productsNumber> AddBulkProducts(IAsyncStreamReader<Product> requestStream, ServerCallContext context)
        {
            int count = 0;
            await foreach (var request in requestStream.ReadAllAsync())
            {
                Products.Add(request);
                ++count;
            }

            return await Task.FromResult(new productsNumber { Count = count });
        }

        [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
        public override async Task GetProductReport(Empty request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            foreach (var item in Products)
            {
                await responseStream.WriteAsync(item);
            }

            await Task.CompletedTask;
        }
    }
}
