using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Fartak.Application.Role.Commands
{
    public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان نقش الزامی است.")
                .MaximumLength(100).WithMessage("عنوان نقش نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد نقش الزامی است.")
                .MaximumLength(40).WithMessage("کد نقش نمی‌تواند بیشتر از ۴۰ کاراکتر باشد.")
                // می‌تونیم ولیدیشن فرمت رو اینجا هم بذاریم تا زودتر ارور بده
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("کد نقش فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");
        }
    }
}
