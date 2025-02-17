using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WhatTheFuck.extensions;
using xemuh2stats.enums;

namespace xemuh2stats.objects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct s_variant_details
    {
        private static Dictionary<string, string> scenario_map = new Dictionary<string, string>()
        {
            {"beavercreek", "Beaver Creek"},
            {"burial_mounds", "Burial Mounds"},
            {"cyclotron", "Ivory Tower"},
            {"deltatap", "Sanctuary"},
            {"dune", "Relic"},
            {"street_sweeper", "District"},
            {"needle", "Uplift"},
        };

        public string name;
        public game_type game_type;
        public string scenario;
        public DateTime start_time;
        public DateTime end_time;
        public TimeSpan duration => ((end_time == DateTime.MinValue) ? DateTime.Now - start_time :  end_time - start_time);
        public string game_type_name => game_type.GetDisplayName();
        
        public string map_name
        {
            get
            {
                if (string.IsNullOrEmpty(scenario))
                    return "";

                if(scenario_map.TryGetValue(this.scenario, out var mapName))
                    return mapName;
                
                return char.ToUpper(this.scenario[0]) + this.scenario.Substring(1);
            }
        }
    }
    public static class variant_details
    {
        public static s_variant_details get()
        {
            var addr = Program.exec_resolver["variant_info"].address;
            var result = new s_variant_details();

            result.name = Program.memory.ReadStringUnicode(addr, 16);
            result.game_type = (game_type)Program.memory.ReadByte(addr + 0x40);
            result.scenario = Program.memory.ReadStringAscii(addr + 0x130, 256).Split('\\').Last();
            return result;
        }
    }
}
