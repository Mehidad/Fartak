using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.User
{
    public enum UserRole
    {
        Farmer = 1,   // کشاورز
        Expert = 2,   // کارشناس کشاورزی
        Mentor = 3,   // منتور / مشاور ارشد
        Admin = 4,
        NotSet = 5 // برای زمانی که کاربر هنوز ثبت‌نام نهایی را انجام نداده و نقش آن هنوز مشخص نیست
    }
}
