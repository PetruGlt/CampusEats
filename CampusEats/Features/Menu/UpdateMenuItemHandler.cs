using AutoMapper;
using CampusEats.Persistence;
using FluentValidation;

namespace CampusEats.Features.Menu;

public class UpdateMenuItemHandler(CampusEatsContext context, IMapper mapper, IValidator<UpdateMenuItemRequest> validator)
{
    public async Task<IResult> Handle(UpdateMenuItemRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
           throw new ValidationException(validationResult.Errors);       
        }
/*var existingMenuItem = await context.MenuItems.FindAsync(request.Id);

        mapper.Map(request, existingMenuItem);*/
        var updatedMenuItem = mapper.Map<MenuItem>(request);
        
        context.MenuItems.Update(updatedMenuItem);
        await context.SaveChangesAsync();

        return Results.Ok(updatedMenuItem);
    }
}
