using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule
{
    // VisitScheduleErrors.cs
  
    public static class VisitScheduleErrors
    {
        public static readonly Error CaseNotSpecified = Error.Create(
            "VisitSchedule.CaseNotSpecified",
            "برای ثبت برنامه بازدید باید پرونده مشخص باشد.",
            new Dictionary<string, object>());

        public static readonly Error VisitTypeNotSpecified = Error.Create(
            "VisitSchedule.VisitTypeNotSpecified",
            "نوع بازدید باید مشخص باشد.",
            new Dictionary<string, object>());

        public static readonly Error StatusNotSpecified = Error.Create(
            "VisitSchedule.StatusNotSpecified",
            "وضعیت برنامه بازدید باید مشخص باشد.",
            new Dictionary<string, object>());

        public static readonly Error ScheduledForInPast = Error.Create(
            "VisitSchedule.ScheduledForInPast",
            "زمان بازدید نمی‌تواند در گذشته باشد.",
            new Dictionary<string, object>());

        public static readonly Error NewScheduledForInPast = Error.Create(
            "VisitSchedule.NewScheduledForInPast",
            "زمان جدید بازدید نمی‌تواند در گذشته باشد.",
            new Dictionary<string, object>());

        public static readonly Error SameVisitType = Error.Create(
            "VisitSchedule.SameVisitType",
            "نوع بازدید جدید با نوع فعلی یکسان است.",
            new Dictionary<string, object>());

        public static readonly Error SameStatus = Error.Create(
            "VisitSchedule.SameStatus",
            "وضعیت جدید با وضعیت فعلی یکسان است.",
            new Dictionary<string, object>());

        public static readonly Error SameScheduledFor = Error.Create(
            "VisitSchedule.SameScheduledFor",
            "زمان جدید بازدید با زمان فعلی یکسان است.",
            new Dictionary<string, object>());

        public static readonly Error NoteTooLong = Error.Create(
            "VisitSchedule.NoteTooLong",
            "یادداشت برنامه بازدید نمی‌تواند بیش از ۵۰۰ نویسه باشد.",
            new Dictionary<string, object> { ["MaxLength"] = VisitSchedule.NoteMaxLength });
    }
}
