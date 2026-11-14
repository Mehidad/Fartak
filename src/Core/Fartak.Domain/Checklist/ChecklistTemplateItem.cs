using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.Unit;

namespace Fartak.Domain.Checklist
{
    // آیتم چک‌لیست (جدول checklist_template_items) — موجودیت فرزندِ ChecklistTemplate؛
    // فقط از طریق خود Aggregate ساخته/ویرایش می‌شود.
    // template_id ستون صریح ندارد (مطابق الگوی CaseNote) و با navigation در EF نگاشت می‌شود.
    public sealed class ChecklistTemplateItem : Entity<ChecklistTemplateItemId>
    {
        public string Title { get; private set; }              // مثل «تراکم بوته» / «آفات و بیماری‌ها»
        public int SortOrder { get; private set; }             // ترتیب نمایش در فرم بازدید
        public LookupId AnswerTypeId { get; private set; }     // lookup: answer_type — rating/numeric/boolean/text
        public UnitId? UnitId { get; private set; }            // واحد پاسخ عددی (مثل m2) — اختیاری
        public bool IsRequired { get; private set; }           // پاسخ به این آیتم در فرم بازدید اجباری است؟

        private ChecklistTemplateItem() { } // برای EF Core

        private ChecklistTemplateItem(
            ChecklistTemplateItemId id,
            string title,
            LookupId answerTypeId,
            UnitId? unitId,
            bool isRequired,
            int sortOrder)
        {
            Id = id;
            Title = title;
            AnswerTypeId = answerTypeId;
            UnitId = unitId;
            IsRequired = isRequired;
            SortOrder = sortOrder;
        }

        // فقط ریشه تجمع صدا می‌زند؛ اعتبارسنجی عنوان/ترتیب/یکتایی پیش‌تر در ریشه انجام شده است
        internal static ChecklistTemplateItem Create(
            ChecklistTemplateItemId id,
            string title,
            LookupId answerTypeId,
            UnitId? unitId,
            bool isRequired,
            int sortOrder)
        {
            if (id is null || answerTypeId is null || string.IsNullOrWhiteSpace(title))
                throw new DomainValidationException(
                    "Checklist.ItemInvalid",
                    "شناسه آیتم، عنوان یا نوع پاسخ آیتم ارسال نشده است.");

            return new ChecklistTemplateItem(id, title.Trim(), answerTypeId, unitId, isRequired, sortOrder);
        }

        internal void SetTitle(string title)
            => Title = title;

        internal void SetSortOrder(int sortOrder)
            => SortOrder = sortOrder;

        internal void SetAnswerType(LookupId answerTypeId)
            => AnswerTypeId = answerTypeId;

        internal void SetUnit(UnitId? unitId)
            => UnitId = unitId;

        internal void SetRequirement(bool isRequired)
            => IsRequired = isRequired;
    }
}
