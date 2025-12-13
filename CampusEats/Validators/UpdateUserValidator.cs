using CampusEats.Features.Users;
using FluentValidation;

namespace CampusEats.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email must be valid");
        RuleFor(x => x.PlainPassword).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long");
    }
}