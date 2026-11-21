using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.PlanVersion
{
    // فاز زمان‌بندی برنامه (جدول plan_phases) — ظرف ساده‌سازی خطوط («مرحله کشت و استقرار»،
    // «مراحل تغذیه و مدیریت» و...). فقط عنوان و ترتیب دارد؛ اعتبارسنجی در والد انجام می‌شود
    // و Setterها برای همین internal هستند.
    public class PlanPhase : Entity<PlanPhaseId>
    {
        public string Title { get; private set; }
        public int SortOrder { get; private set; }

        private PlanPhase() { } // برای EF Core

        internal PlanPhase(PlanPhaseId id, string title, int sortOrder)
        {
            Id = id;
            Title = title;
            SortOrder = sortOrder;
        }

        // ساخت فاز جدید — فقط از طریق PlanVersion.AddPhase
        internal static PlanPhase Create(PlanPhaseId id, string title, int sortOrder)
            => new PlanPhase(id, title, sortOrder);

        internal void SetTitle(string newTitle) => Title = newTitle;

        internal void SetSortOrder(int newSortOrder) => SortOrder = newSortOrder;
    }
}
