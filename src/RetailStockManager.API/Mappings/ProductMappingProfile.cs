using AutoMapper;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Domain.Entities;

namespace RetailStockManager.API.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Domain Entity ↔ DTO mappings
            CreateMap<Product, ProductDto>()
                .ReverseMap();

            CreateMap<CreateProductDto, Product>()
                .ConstructUsing(dto => new Product(dto.Name, dto.Description, dto.Price, dto.Category));

            CreateMap<UpdateProductDto, Product>()
                .ConstructUsing(dto => new Product(dto.Name, dto.Description, dto.Price, dto.Category))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<Product, ProductSummaryDto>();
        }
    }

    public class StockMappingProfile : Profile
    {
        public StockMappingProfile()
        {
            CreateMap<StockItem, StockItemDto>()
                .ReverseMap();

            CreateMap<CreateStockItemDto, StockItem>()
                .ConstructUsing(dto => new StockItem(dto.ProductId, dto.Quantity, dto.Location, dto.ReorderLevel));

            CreateMap<UpdateStockItemDto, StockItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => src.ReorderLevel))
                .ForMember(dest => dest.MaxLevel, opt => opt.MapFrom(src => src.MaxLevel))
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow)); 
        }
    }
}
