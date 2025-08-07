using FluentValidation;
using RetailStockManager.Application.DTOs;

namespace RetailStockManager.API.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 100).WithMessage("Product name must be between 2 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed 999,999.99");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid product category");
        }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 100).WithMessage("Product name must be between 2 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed 999,999.99");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid product category");
        }
    }
}
