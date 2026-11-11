using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Application.Framework.Commands;
using Fartak.Domain.Framework;
using Fartak.Domain.Role;
using Fartak.Domain.Role.Repository;

namespace Fartak.Application.Role.Commands
{

    public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Result<long>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator<long> _idGenerator;

        public CreateRoleCommandHandler(
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IIdGenerator<long> idGenerator)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
        }

        public async Task<Result<long>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. بررسی یکتایی کد نقش
            var isCodeUnique = await _roleRepository.IsCodeUniqueAsync(request.Code, cancellationToken);
            if (!isCodeUnique)
            {
                // استفاده از Result.Failure<long> بجای Result<long>.Failure
                return Result.Failure<long>(RoleErrors.CodeAlreadyExists(request.Code));
            }

            // 2. تولید شناسه
            var roleId = RoleId.From(_idGenerator.GetId());

            // 3. ساخت اگریگیت (اینجا از مسیر کامل استفاده کردم تا با Namespace فعلی تداخل نداشته باشد)
            var createResult = Fartak.Domain.Role.Role.Create(
                roleId,
                request.Title,
                request.Code,
                request.Description,
                request.IsSystem);

            if (createResult.IsFailure)
            {
                return Result.Failure<long>(createResult.Error);
            }

            // 4. ذخیره
            await _roleRepository.AddAsync(createResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // برگرداندن موفقیت
            return Result.Success(createResult.Value.Id.Value);
        }
    }
}
