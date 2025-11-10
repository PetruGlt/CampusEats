using CampusEats.Features.Kitchen;
using CampusEats.Features.Orders;
using FluentValidation;

namespace CampusEats.Validators;

public class UpdateOrderStatusRequestBodyValidator : AbstractValidator<UpdateOrderStatusRequestBody>
{
    public UpdateOrderStatusRequestBodyValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: Pending, Preparing, Ready, Completed, Cancelled");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, true, out _);
    }
}
