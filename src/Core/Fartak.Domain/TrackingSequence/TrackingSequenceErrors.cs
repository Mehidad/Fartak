using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.TrackingSequence
{
    public static class TrackingSequenceErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "TrackingSequence.NullReference",
                message: "مقادیر اجباری دنباله رهگیری ارسال نشده است.");

        public static Error InvalidLastSeq(long lastSeq)
            => Error.Create(
                code: "TrackingSequence.InvalidLastSeq",
                message: "شمارنده سال باید عددی بین ۰ تا ۹۹۹۹۹۹ باشد.",
                details: new Dictionary<string, object>
                {
                    ["LastSeq"] = lastSeq
                });

        public static Error SequenceExhausted(long maxSequenceNumber)
            => Error.Create(
                code: "TrackingSequence.SequenceExhausted",
                message: "ظرفیت تولید کد رهگیری این سال پر شده است.",
                details: new Dictionary<string, object>
                {
                    ["MaxSequenceNumber"] = maxSequenceNumber
                });
    }
}
