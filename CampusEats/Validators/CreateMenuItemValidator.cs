using CampusEats.Features.Menu;
using FluentValidation;

namespace CampusEats.Validators;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}