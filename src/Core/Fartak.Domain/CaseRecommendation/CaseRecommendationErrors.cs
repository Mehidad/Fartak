using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation
{
    public static class CaseRecommendationErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "CaseRecommendation.NullReference",
                message: "مقادیر اجباری توصیه پرونده ارسال نشده است.");

        public static Error InvalidBody()
            => Error.Create(
                code: "CaseRecommendation.InvalidBody",
                message: "متن توصیه خالی است.");

        public static Error BodyTooLong(int length)
            => Error.Create(
                code: "CaseRecommendation.BodyTooLong",
                message: "متن توصیه نباید بیش از ۴٬۰۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Length"] = length
                });

        public static Error BodyUnchanged()
            => Error.Create(
                code: "CaseRecommendation.BodyUnchanged",
                message: "متن توصیه از قبل همین است.");

        public static Error AudiencesUnchanged()
            => Error.Create(
                code: "CaseRecommendation.AudiencesUnchanged",
                message: "مخاطبان اعلان از قبل همین است.");

        public static Error VisitUnchanged(long? visitId)
            => Error.Create(
                code: "CaseRecommendation.VisitUnchanged",
                message: "بازدید متصل به این توصیه از قبل همین است.",
                details: new Dictionary<string, object>
                {
                    ["VisitId"] = visitId ?? 0
                });
    }
}
