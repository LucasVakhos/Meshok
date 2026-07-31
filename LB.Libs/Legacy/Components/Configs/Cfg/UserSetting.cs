using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LB.Libs;

public static class UserSetting
{
    public static bool IsAdmin { get; set; }

    public static string UserName { get; set; } = string.Empty;
    public static int UserId { get; set; }
}
