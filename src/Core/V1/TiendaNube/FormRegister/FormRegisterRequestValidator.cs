using FluentValidation;

namespace Core.V1.TiendaNube.FormRegister
{
    public class FormRegisterRequestValidator : AbstractValidator<FormRegisterRequest>
    {
		public FormRegisterRequestValidator()
		{
			RuleFor(m => m.FirstName)
				.NotEmpty()
				.When(c => c.DocumentType == "DNI");

            RuleFor(m => m.LastName)
                .NotEmpty()
                .When(c => c.DocumentType == "DNI");

            RuleFor(m => m.CompanyName)
                .NotEmpty()
                .When(c => c.DocumentType != "DNI");

            RuleFor(m => m.Email)
				.NotEmpty();

			RuleFor(m => m.Password)
				.NotEmpty();
				

			RuleFor(m => m.DocumentType)
				.NotEmpty();

			RuleFor(m => m.DocumentNumber)
				.NotEmpty();

			RuleFor(m => m.CellPhone)
				.NotEmpty();

			RuleFor(m => m.Street)
				.NotEmpty();

			RuleFor(m => m.StreetNumber)
				.NotEmpty();

			RuleFor(m => m.Location)
				.NotEmpty();

			RuleFor(m => m.City)
				.NotEmpty();

			RuleFor(m => m.CPA)
				.NotEmpty();

		}
    }
}