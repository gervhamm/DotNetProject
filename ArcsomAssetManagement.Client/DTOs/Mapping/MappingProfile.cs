using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;

namespace ArcsomAssetManagement.Client.DTOs.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Manufacturer, ManufacturerDto>()
            .ForMember(dest => dest.ProductDtos, opt => opt.MapFrom(src => src.Products));
        CreateMap<ManufacturerDto, Manufacturer>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.ProductDtos));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ManufacturerDto, opt => opt.MapFrom(src => src.Manufacturer));
        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.ManufacturerDto));

        CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.ProductDto, opt => opt.MapFrom(src => src.Product));
        CreateMap<AssetDto, Asset>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.ProductDto));
    }
}
