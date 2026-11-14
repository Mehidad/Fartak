using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Case;
using Fartak.Domain.Framework;

namespace Fartak.Domain.TrackingSequence
{
    // شمارنده سالانه تولید کد رهگیری پرونده (جدول tracking_sequences) —
    // قالب کد مطابق TrackingCode: «FT-{سال جلالی چهاررقمی}-{شماره ترتیبی ۵رقمی صفرپرشده}» مثل FT-1403-02867
    //
    // تقسیم کار با لایه Application: این تجمع عملیات رزرو شماره را مدل می‌کند؛ اتمیک‌بودن افزایش
    // last_seq در تراکنش Application با قفل ردیف (ROWLOCK / FOR UPDATE) تضمین می‌شود —
    // معادل function make_tracking_code در ERD که برای مسیر SQL خام نگه داشته شده است.
    // هر نمونه تجمع فقط در یک تراکنش استفاده می‌شود و مسابقه هم‌زمانی در سطح Application حل می‌شود.
    //
    // ردیف هر سال با اولین پرونده همان سال ساخته می‌شود (last_seq = 0) یا هنگام درون‌ریزی
    // داده تاریخی با Create(lastSeq) مقدار اولیه می‌گیرد.
    //
    // جدول فقط دو ستون دارد و بدون timestamp و is_active است — بنابراین این تجمع عمداً
    // بدون CreatedAt/UpdatedAt و بدون متد وضعیت است.
    public class TrackingSequence : AggregateRoot<TrackingSequenceId>
    {
        // سقف شماره ترتیبی سالانه — مطابق سقف ۶ رقمی شماره در قواعد ساختاری TrackingCode
        public const long MaxSequenceNumber = 999_999;

        public long LastSeq { get; private set; }   // آخرین شماره مصرف‌شده در این سال (۰ = هیچ کدی صادر نشده)

        private TrackingSequence() { } // برای EF Core

        private TrackingSequence(TrackingSequenceId id, long lastSeq)
        {
            Id = id;
            LastSeq = lastSeq;
        }

        public static Result<TrackingSequence> Create(TrackingSequenceId id, long lastSeq = 0)
        {
            if (id is null)
                return Result.Failure<TrackingSequence>(TrackingSequenceErrors.NullReferenceError());

            if (lastSeq < 0 || lastSeq > MaxSequenceNumber)
                return Result.Failure<TrackingSequence>(TrackingSequenceErrors.InvalidLastSeq(lastSeq));

            var sequence = new TrackingSequence(id, lastSeq);
            sequence.AddDomainEvent(new TrackingSequenceCreatedEvent(id.Value, lastSeq));
            return Result<TrackingSequence>.Success(sequence);
        }

        // رزرو شماره بعدی برای پرونده جدید — شمارنده را یک واحد جلو می‌برد و کد قالب‌بندی‌شده
        // را با TrackingCode اعتبارسنجی و برمی‌گرداند؛ کد خروجی مستقیماً به Case.Create داده می‌شود.
        // قالب: FT-{سال چهاررقمی}-{شماره ۵رقمی صفرپرشده} — مطابق lpad(seq,5,'0') در function ERD
        public Result<TrackingCode> ReserveNext()
        {
            var nextSeq = LastSeq + 1;
            if (nextSeq > MaxSequenceNumber)
                return Result.Failure<TrackingCode>(TrackingSequenceErrors.SequenceExhausted(MaxSequenceNumber));

            LastSeq = nextSeq;

            var code = TrackingCode.Create($"FT-{Id.Value:D4}-{nextSeq:D5}");
            AddDomainEvent(new TrackingCodeReservedEvent(Id.Value, nextSeq, code.Value));
            return Result<TrackingCode>.Success(code);
        }
    }
}
