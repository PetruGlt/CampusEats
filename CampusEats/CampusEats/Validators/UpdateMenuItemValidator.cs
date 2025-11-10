using CampusEats.Features.Menu;
using CampusEats.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Validators;

public class UpdateMenuItemValidator : AbstractValidator<UpdateMenuItemRequest>
{
    public UpdateMenuItemValidator(CampusEatsContext context)
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty).WithMessage("Id must be a valid GUID.");
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x).MustAsync(async (req, ct) =>
        {
            return !await context.MenuItems.AnyAsync(m =>
                m.Name == req.Name && m.Id != req.Id, ct);
        }).WithMessage("Another item with the same name already exists.");
    }
}