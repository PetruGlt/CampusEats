using AutoMapper;
using CampusEats.Features.Menu;

namespace CampusEats.Mappings;

public class MenuItemMappingProfile : Profile
{
    public MenuItemMappingProfile()
    {
        CreateMap<CreateMenuItemRequest, MenuItem>()
            .ConstructUsing(src => 
                new MenuItem(Guid.NewGuid(), src.Name, src.Price)
            );
        CreateMap<UpdateMenuItemRequest, MenuItem>()
            .ConstructUsing(src => 
                new MenuItem(src.Id, src.Name, src.Price)
            );
    }
}