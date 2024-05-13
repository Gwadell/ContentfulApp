using ContentfulApp.Models.DTO.ExportDto;
using ContentfulApp.Models.DTO;

namespace ContentfulApp.Services
{
    public class DtoMappingService : IDtoMappingService
    {
        /// <summary>
        /// Maps the given DTO object to the specified export DTO type.
        /// </summary>
        /// <param name="dto">The DTO object to map.</param>
        /// <param name="exportDtoType">The type of the export DTO.</param>
        /// <returns>The mapped export DTO object.</returns>
        public object MapToExportDto(object dto, Type exportDtoType)
        {
            switch (exportDtoType.Name)
            {
                case nameof(FullExport) when dto is FullEntryDto fullEntryDto:
                    return new FullExport
                    {
                        // Mapping properties from FullEntryDto to FullExport
                        Id = fullEntryDto.Sys.Id,
                        InternalName = fullEntryDto.InternalName,
                        Name = fullEntryDto.Name,
                        IsPrimaryCategory = fullEntryDto.IsPrimaryCategory,
                        CategoryRank = fullEntryDto.CategoryRank,
                        ShortDescription = fullEntryDto.ShortDescription,
                        Filter = fullEntryDto.Filter?._rawFilterData,
                        //SubPageData = fullEntryDto.ConvertSubPageDataToString(),
                        AdditionalContentDescription = fullEntryDto.ConvertAdditionalContentDescriptionToHtml(), 
                        Active = fullEntryDto.IsActiveLocale(),
                        CreateLinksOnProductPages = fullEntryDto.CreateLinksOnProductPages,
                        UseAsFacet = fullEntryDto.UseAsFacet,
                        Tags = fullEntryDto.GetTagsIdsAsString(), 
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
                        // Mapping properties from RegularEntryDto to RegularExport
                        Id = regularEntryDto.Sys.Id,
                        InternalName = regularEntryDto.InternalName,
                        Name = regularEntryDto.Name,
                        Slug = regularEntryDto.Slug,
                        Urls = regularEntryDto.GetLastUrl(),
                        CreatedAt = regularEntryDto.Sys.CreatedAt,
                        Tags = regularEntryDto.GetTagsIdsAsString(),
                        //archived
                        
                    };
                default:
                    return null;
            }
        }

        public Type GetDtoType(string contentType)
        {
            return contentType switch
            {
                "productListingPage" => typeof(FullEntryDto),
                "brand" => typeof(FullEntryDto),
                "collection" => typeof(FullEntryDto),
                "designer" => typeof(FullEntryDto),
                _ => typeof(RegularEntryDto)
            };
        }

        public Type GetExportDtoType(string contentType)
        {
            if (!ContentTypeToExportDtoTypeMap.TryGetValue(contentType, out var exportDtoType))
            {
                exportDtoType = ContentTypeToExportDtoTypeMap["_default"];
            }

            return exportDtoType;
        }

        public static Dictionary<string, Type> ContentTypeToExportDtoTypeMap = new Dictionary<string, Type>
        {
            { "productListingPage", typeof(FullExport) },
            { "brand", typeof(FullExport) },
            { "collection", typeof(FullExport) },
            { "designer", typeof(FullExport) },
            { "_default", typeof(RegularExport) }
        };
    }

    public interface IDtoMappingService
    {
        object MapToExportDto(object dto, Type exportDtoType);
        Type GetDtoType(string contentType);
        Type GetExportDtoType(string contentType);

    }
}
