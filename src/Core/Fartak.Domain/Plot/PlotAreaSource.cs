using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Plot
{
    // منبع مساحت قطعه — معادل ستون area_source در جدول plots
    // نگاشت به رشته‌های 'computed' / 'manual' در Infrastructure (کلاس Convertor) انجام می‌شود
    public enum PlotAreaSource
    {
        Computed = 1, // محاسبه‌شده از پلی‌گان GeoJSON
        Manual = 2    // ورود دستی توسط کاربر/کارشناس
    }
}
