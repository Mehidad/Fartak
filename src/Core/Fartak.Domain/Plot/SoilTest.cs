using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Plot
{
    // نتیجه تست خاک — موجودیت فرزندِ Plot (جدول soil_tests)
    // فقط از طریق خود Aggregate ساخته می‌شود: Plot.RecordSoilTest
    public sealed class SoilTest : Entity<SoilTestId>
    {
        public PlotId PlotId { get; private set; }
        public DateOnly TestedAt { get; private set; }        // تاریخ تست (تاریخ میلادی، در DB ستون date)
        public string? LabName { get; private set; }          // نام آزمایشگاه
        public SoilAnalysis Analysis { get; private set; }    // پارامترهای ساختاریافته (jsonb)
        public string? Summary { get; private set; }          // جمع‌بندی آزمایش
        public UserId? RecordedBy { get; private set; }       // ثبت‌کننده (NULL = سیستم)
        public DateTime CreatedAt { get; private set; }

        private SoilTest() { } // برای EF Core

        private SoilTest(
            SoilTestId id,
            PlotId plotId,
            DateOnly testedAt,
            SoilAnalysis analysis,
            string? labName,
            string? summary,
            UserId? recordedBy)
        {
            Id = id;
            PlotId = plotId;
            TestedAt = testedAt;
            Analysis = analysis;
            LabName = labName;
            Summary = summary;
            RecordedBy = recordedBy;
            CreatedAt = DateTime.UtcNow;
        }

        internal static SoilTest Create(
            SoilTestId id,
            PlotId plotId,
            DateOnly testedAt,
            SoilAnalysis analysis,
            string? labName,
            string? summary,
            UserId? recordedBy)
        {
            if (analysis is null)
                throw new DomainValidationException(
                    "Plot.SoilAnalysisNull",
                    "پارامترهای تست خاک ارسال نشده است.");

            return new SoilTest(id, plotId, testedAt, analysis, labName, summary, recordedBy);
        }
    }
}
