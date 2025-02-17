using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheFuck.extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            var a = enumValue.GetType().GetMember(enumValue.ToString()).First();
            return (a.GetCustomAttribute<DisplayAttribute>() != null ? a.GetCustomAttribute<DisplayAttribute>().GetName() : enumValue.ToString()) ?? string.Empty;
        }
    }
}
