using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xemuh2stats.objects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct s_game_stats
    {
        public ushort kills;
        public ushort assists;
        public ushort deaths;
        public ushort betrayals;
        public ushort suicides;
        public ushort best_spree;
        public ushort total_tile_alive;
        public ushort ctf_scores;
        public ushort ctf_flag_steals;
        public ushort ctf_flag_saves;
        public ushort ctf_unkown;
        public ushort assault_suicide;
        public ushort assault_score;
        public ushort assault_bomber_kills;
        public ushort assault_bomb_grabbed;
        public ushort assault_bomb_unk;
        public uint oddball_score;
        public ushort oddball_ball_kills;
        public ushort oddball_carried_kills;
        public ushort koth_kills_as_king;
        public ushort koth_kings_killed;
        public ushort juggernauts_killed;
        public ushort kills_as_juggernaut;
        public ushort juggernaut_time;
        public ushort territories_taken;
        public ushort territories_lost;
    }
    public static class game_stats
    {
        public static s_game_stats get(int player_index)
        {
            var addr = Program.exec_resolver["game_stats"].address + player_index * 0x36A;

            if (player_index > 4)
                addr = Program.exec_resolver["game_results_globals_extra"].address + (player_index - 5) * 0x36A;

            return Program.memory.ReadStruct<s_game_stats>(addr);
        }

        public static long get_addr(int player_index)
        {
            return Program.exec_resolver["game_stats"].address + player_index * 0x36A;
        }
    }
}
