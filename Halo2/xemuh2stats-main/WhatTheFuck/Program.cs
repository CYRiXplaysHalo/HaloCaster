using DocumentFormat.OpenXml.ExtendedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhatTheFuck.classes;
using xemuh2stats.classes;
using xemuh2stats.objects;
using Application = System.Windows.Forms.Application;

namespace xemuh2stats
{
    internal static class Program
    {
        public static MemoryHandler memory;
        public static long game_state_address;
        public static offset_resolver game_state_resolver = new offset_resolver();
        public static offset_resolver exec_resolver = new offset_resolver();
        public static configuration_collection configurations = new configuration_collection();
        public static QmpProxy qmp;
        public static s_variant_details variant_details_cache;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //object_formatter.register_type("weapon_stats", typeof(weapon_stat.s_weapon_stat));
            //object_formatter.register_type("game_stats", typeof(s_game_stats));
            //object_formatter.register_type("medal_stats", typeof(s_medal_stats));
            //object_formatter.register_type("post_report", typeof(s_post_game_player));
            Application.Run(new Form1());
        }
    }
}
