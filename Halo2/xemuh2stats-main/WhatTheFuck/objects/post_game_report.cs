using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using xemuh2stats.enums;

namespace xemuh2stats.objects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct s_post_game_player
    {
        public string name;
        public string score;
        public int kills;
        public int deaths;
        public int assists;
        public int suicides;
        public unit_team team;
        public bool observer;
        public short rank;
        public short rank_verified;
        public int medal_flags;
        public int medal_flags_2;
        public int shots_fired;
        public int shots_hit;
        public int head_shots;
        public int killed_player_1;
        public int killed_player_2;
        public int killed_player_3;
        public int killed_player_4;
        public int killed_player_5;
        public int killed_player_6;
        public int killed_player_7;
        public int killed_player_8;
        public int killed_player_9;
        public int killed_player_10;
        public int killed_player_11;
        public int killed_player_12;
        public int killed_player_13;
        public int killed_player_14;
        public int killed_player_15;
        public int killed_player_16;
        public string place;
    }
    public static class post_game_report
    {
        public static s_post_game_player get(int player_index)
        {
            var addr = Program.exec_resolver["post_game_report"].address + 0x114 * player_index;
            var result = new s_post_game_player();

            result.name = Program.memory.ReadStringUnicode(addr, 16);
            result.score = Program.memory.ReadStringUnicode(addr + 0x40, 16);
            result.kills = Program.memory.ReadInt(addr + 0x60);
            result.deaths = Program.memory.ReadInt(addr + 0x64);
            result.assists = Program.memory.ReadInt(addr + 0x68);
            result.suicides = Program.memory.ReadInt(addr + 0x6C);
            result.team = (unit_team)Program.memory.ReadShort(addr + 0x72); 
            result.observer = Program.memory.ReadBool(addr + 0x74);
            result.rank = Program.memory.ReadShort(addr + 0x78); 
            result.rank_verified = Program.memory.ReadShort(addr + 0x7c); 
            result.medal_flags = Program.memory.ReadInt(addr + 0x7E); 
            result.medal_flags_2 = Program.memory.ReadInt(addr + 0x80); 
            result.shots_fired = Program.memory.ReadInt(addr + 0x84); 
            result.shots_hit = Program.memory.ReadInt(addr + 0x88); 
            result.head_shots = Program.memory.ReadInt(addr + 0x8c); 
            result.killed_player_1 = Program.memory.ReadInt(addr + 0x90); 
            result.killed_player_2 = Program.memory.ReadInt(addr + 0x94); 
            result.killed_player_3 = Program.memory.ReadInt(addr + 0x98); 
            result.killed_player_4 = Program.memory.ReadInt(addr + 0x9C); 
            result.killed_player_5 = Program.memory.ReadInt(addr + 0xA0); 
            result.killed_player_6 = Program.memory.ReadInt(addr + 0xA4); 
            result.killed_player_7 = Program.memory.ReadInt(addr + 0xA8); 
            result.killed_player_8 = Program.memory.ReadInt(addr + 0xAC); 
            result.killed_player_9 = Program.memory.ReadInt(addr + 0xB0); 
            result.killed_player_10 = Program.memory.ReadInt(addr + 0xB4); 
            result.killed_player_11 = Program.memory.ReadInt(addr + 0xB8); 
            result.killed_player_12 = Program.memory.ReadInt(addr + 0xBC); 
            result.killed_player_13 = Program.memory.ReadInt(addr + 0xC0); 
            result.killed_player_14 = Program.memory.ReadInt(addr + 0xC4); 
            result.killed_player_15 = Program.memory.ReadInt(addr + 0xC8); 
            result.killed_player_16 = Program.memory.ReadInt(addr + 0xCC); 
            result.place = Program.memory.ReadStringUnicode(addr + 0xE0, 16);

            return result;
        }
    }
}
