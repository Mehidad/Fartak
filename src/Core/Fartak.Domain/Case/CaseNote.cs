using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;
using Fartak.Domain.User.Fartak.Domain.User;

namespace Fartak.Domain.Case
{
    // یادداشت آزاد روی پرونده — موجودیت فرزندِ Case (جدول case_notes)
    // فقط از طریق خود Aggregate ساخته می‌شود: Case.AddNote
    public sealed class CaseNote : Entity<CaseNoteId>
    {
        // سقف عملیاتی متن یادداشت (ستون body از نوع text است و در DB سقف ندارد)
        public const int MaxBodyLength = 4000;

        public UserId AuthorId { get; private set; }
        public string Body { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private CaseNote() { } // برای EF Core

        private CaseNote(CaseNoteId id, UserId authorId, string body)
        {
            Id = id;
            AuthorId = authorId;
            Body = body;
            CreatedAt = DateTime.UtcNow;
        }

        internal static CaseNote Create(CaseNoteId id, UserId authorId, string body)
        {
            if (authorId is null || string.IsNullOrWhiteSpace(body))
                throw new DomainValidationException(
                    "Case.NoteInvalid",
                    "متن یادداشت یا نویسنده یادداشت ارسال نشده است.");

            return new CaseNote(id, authorId, body.Trim());
        }
    }
}
