using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xemuh2stats.enums
{
    public enum game_type
    {
        invalid = -1,
        none = 0,
        [Display(Name = "Capture the Flag")]
        capture_the_flag = 1,
        [Display(Name = "Slayer")]
        slayer = 2,
        [Display(Name = "Oddball")]
        oddball = 3,
        [Display(Name = "King of the Hill")]
        king_of_the_hill = 4,
        unused_5 = 5,
        unused_6 = 6,
        [Display(Name = "Juggernaut")]
        juggernaut = 7,
        [Display(Name = "Territories")]
        territories = 8,
        [Display(Name = "Assault")]
        assault = 9,
        unused_10 = 10,
    }
}
