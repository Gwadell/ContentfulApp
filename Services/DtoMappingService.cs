using ContentfulApp.Models.DTO.ExportDto;
using ContentfulApp.Models.DTO;

namespace ContentfulApp.Services
{
    public class DtoMappingService
    {
        public object MapToExportDto(object dto, Type exportDtoType)
        {
            switch (exportDtoType.Name)
            {
                case nameof(FullExport) when dto is FullEntryDto fullEntryDto:
                    return new FullExport
                    {
                        Id = fullEntryDto.Sys.Id,
                        InternalName = fullEntryDto.InternalName,
                        Name = fullEntryDto.Name,
                        IsPrimaryCategory = fullEntryDto.IsPrimaryCategory,
                        CategoryRank = fullEntryDto.CategoryRank,
                        ShortDescription = fullEntryDto.ShortDescription,
                        Filter = fullEntryDto.Filter?._rawFilterData,
                        //SubPageData = fullEntryDto.SubPageData.Routes,
                        AdditionalContentDescription = fullEntryDto.GetAdditionalContentDescriptionAsString(), //funkar ej 
                        Active = fullEntryDto.IsActiveLocale(),
                        CreateLinksOnProductPages = fullEntryDto.CreateLinksOnProductPages,
                        UseAsFacet = fullEntryDto.UseAsFacet,
                        Tags = fullEntryDto.GetTagsAsString(), //funkar ej 
                        Facets = fullEntryDto.GetFacetsAsString(),
                        SeoTitle = fullEntryDto.SeoInfo?.Title,
                        SeoDescription = fullEntryDto.SeoInfo?.Description,
                        Slug = fullEntryDto.Slug,
                        CreatedAt = fullEntryDto.Sys.CreatedAt,
                        Urls = fullEntryDto.GetLastUrl(),
                        Archived = fullEntryDto.IsArchived(),
                        H1Title = fullEntryDto.H1Title,
                        //archived
                    };
                case nameof(RegularExport) when dto is RegularEntryDto regularEntryDto:
                    return new RegularExport
                    {
                        Id = regularEntryDto.Sys.Id,
                        InternalName = regularEntryDto.InternalName,
                        Name = regularEntryDto.Name,
                        Slug = regularEntryDto.Slug,
                        Urls = regularEntryDto.GetLastUrl(),
                        CreatedAt = regularEntryDto.Sys.CreatedAt,
                        //archived
                        //tags
                    };
                default:
                    return null;

            }
        }
    }
}
