using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Region
{
    // سطح منطقه در سلسله‌مراتب جغرافیایی — معادل ستون level در جدول regions
    // نگاشت به رشته‌های 'province' / 'city' / 'village' در Infrastructure (کلاس Convertor) انجام می‌شود
    public enum RegionLevel
    {
        Province = 1, // استان — ریشه‌ی درخت؛ والد ندارد
        City = 2,     // شهرستان — فقط زیر استان
        Village = 3   // روستا — فقط زیر شهرستان
    }
}
