using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Fartak.Application.Role.Queries.GetRolesQuery
{

    public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
    {
        public GetRolesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("شماره صفحه باید بزرگتر از صفر باشد.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("تعداد آیتم‌های صفحه باید بزرگتر از صفر باشد.")
                .LessThanOrEqualTo(100).WithMessage("حداکثر تعداد آیتم‌های هر صفحه ۱۰۰ عدد است.");
        }
    }
}
