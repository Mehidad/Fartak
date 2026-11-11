using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Common;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Case
{
    // یک رکورد از تخصیص پرونده به کارشناس — موجودیت فرزندِ Case (جدول case_assignments)
    // فقط از طریق خود Aggregate ساخته می‌شود: Case.AssignExpert
    // هر تخصیص یک مهلت پاسخ (SLA) دارد؛ پاسخ کارشناس با RecordResponse بسته می‌شود.
    public sealed class CaseAssignment : Entity<CaseAssignmentId>
    {
        public UserId ExpertId { get; private set; }
        public LookupId? SourceId { get; private set; }      // منبع تخصیص (lookup: assignment_source)
        public DateTime AssignedAt { get; private set; }
        public DateTime Deadline { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public LookupId? OutcomeId { get; private set; }     // نتیجه پاسخ (lookup: assignment_outcome)
        public string? Note { get; private set; }

        // تخصیص پاسخ‌داده‌نشده = تخصیص باز (در هر لحظه حداکثر یک تخصیص باز مجاز است)
        public bool IsOpen => RespondedAt is null;

        private CaseAssignment() { } // برای EF Core

        private CaseAssignment(
            CaseAssignmentId id,
            UserId expertId,
            DateTime assignedAt,
            DateTime deadline,
            LookupId? sourceId,
            string? note)
        {
            Id = id;
            ExpertId = expertId;
            SourceId = sourceId;
            AssignedAt = assignedAt;
            Deadline = deadline;
            Note = note;
        }

        internal static CaseAssignment Create(
            CaseAssignmentId id,
            UserId expertId,
            DateTime assignedAt,
            DateTime deadline,
            LookupId? sourceId,
            string? note)
        {
            if (expertId is null)
                throw new DomainValidationException(
                    "Case.AssignmentExpertNull",
                    "کارشناس برای تخصیص پرونده ارسال نشده است.");

            return new CaseAssignment(id, expertId, assignedAt, deadline, sourceId, note);
        }

        // ثبت پاسخ کارشناس به تخصیص (پذیرش/ردی/انقضای SLA — نتیجه به‌صورت LookupId می‌آید)
        internal Result RecordResponse(LookupId outcomeId, DateTime respondedAt, string? note = null)
        {
            if (outcomeId is null)
                throw new DomainValidationException(
                    "Case.AssignmentOutcomeNull",
                    "نتیجه پاسخ تخصیص ارسال نشده است.");

            if (!IsOpen)
                return Result.Failure(CaseErrors.AssignmentAlreadyResponded(Id.Value));

            if (respondedAt < AssignedAt)
                return Result.Failure(CaseErrors.InvalidRespondedAt(respondedAt, AssignedAt));

            RespondedAt = respondedAt;
            OutcomeId = outcomeId;
            if (string.IsNullOrWhiteSpace(Note) && !string.IsNullOrWhiteSpace(note))
                Note = note.Trim();

            return Result.Success();
        }
    }
}
