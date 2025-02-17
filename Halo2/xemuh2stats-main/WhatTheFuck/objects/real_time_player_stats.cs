using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace xemuh2stats.objects
{
    public enum e_game_team : byte
    {
        [Display(Name = "Red")]
        _game_team_red = 0,
        [Display(Name = "Blue")]
        _game_team_blue = 1,
        [Display(Name = "Yellow")]
        _game_team_yellow = 2,
        [Display(Name = "Green")]
        _game_team_green = 3,
        [Display(Name = "Purple")]
        _game_team_purple = 4,
        [Display(Name = "Orange")]
        _game_team_orange = 5,
        [Display(Name = "Brown")]
        _game_team_brown = 6,
        [Display(Name = "Pink")]
        _game_team_pink = 7,
        [Display(Name = "Neutral")]
        _game_team_neutral = 8,
    }

    public enum e_player_color : byte
    {
        _player_color_white,
        _player_color_steel,
        _player_color_red,
        _player_color_orange,
        _player_color_gold,
        _player_color_olive,
        _player_color_green,
        _player_color_sage,
        _player_color_cyan,
        _player_color_teal,
        _player_color_colbat,
        _player_color_blue,
        _player_color_violet,
        _player_color_purple,
        _player_color_pink,
        _player_color_crimson,
        _player_color_brown,
        _player_color_tan,
        k_player_color_count
    };

    public enum e_character_type : byte
    {
        _character_type_masterchief = 0,
        _character_type_dervish = 1,
        _character_type_spartan = 2,
        _character_type_elite = 3,
        k_player_character_type_count
    };

    public enum e_handicap : byte
    {
        _handicap_none = 0,
        _handicap_minor = 1,
        _handicap_moderate = 2,
        _handicap_severe = 3,

        k_handicap_count
    };

    public enum e_emblem_foreground : byte
    {
        _emblem_foreground_seventh_column = 0,
        _emblem_foreground_bullseye = 1,
        _emblem_foreground_vortex = 2,
        _emblem_foreground_halt = 3,
        _emblem_foreground_spartan = 4,
        _emblem_foreground_da_bomb = 5,
        _emblem_foreground_trinity = 6,
        _emblem_foreground_delta = 7,
        _emblem_foreground_rampancy = 8,
        _emblem_foreground_sergeant = 9,
        _emblem_foreground_phenoix = 10,
        _emblem_foreground_champion = 11,
        _emblem_foreground_jolly_roger = 12,
        _emblem_foreground_marathon = 13,
        _emblem_foreground_cube = 14,
        _emblem_foreground_radioactive = 15,
        _emblem_foreground_smiley = 16,
        _emblem_foreground_frowney = 17,
        _emblem_foreground_spearhead = 18,
        _emblem_foreground_sol = 19,
        _emblem_foreground_waypoint = 20,
        _emblem_foreground_ying_yang = 21,
        _emblem_foreground_helmet = 22,
        _emblem_foreground_triad = 23,
        _emblem_foreground_grunt_symbol = 24,
        _emblem_foreground_cleave = 25,
        _emblem_foreground_thor = 26,
        _emblem_foreground_skull_king = 27,
        _emblem_foreground_triplicate = 28,
        _emblem_foreground_subnova = 29,
        _emblem_foreground_flaming_ninja = 30,
        _emblem_foreground_doubleCresent = 31,
        _emblem_foreground_spades = 32,
        _emblem_foreground_clubs = 33,
        _emblem_foreground_diamonds = 34,
        _emblem_foreground_hearts = 35,
        _emblem_foreground_wasp = 36,
        _emblem_foreground_mark_of_shame = 37,
        _emblem_foreground_snake = 38,
        _emblem_foreground_hawk = 39,
        _emblem_foreground_lips = 40,
        _emblem_foreground_capsule = 41,
        _emblem_foreground_cancel = 42,
        _emblem_foreground_gas_mask = 43,
        _emblem_foreground_grenade = 44,
        _emblem_foreground_tsanta = 45,
        _emblem_foreground_race = 46,
        _emblem_foreground_valkyire = 47,
        _emblem_foreground_drone = 48,
        _emblem_foreground_grunt = 49,
        _emblem_foreground_grunt_head = 50,
        _emblem_foreground_brute_head = 51,
        _emblem_foreground_runes = 52,
        _emblem_foreground_trident = 53,
        _emblem_foreground_number0 = 54,
        _emblem_foreground_number1 = 55,
        _emblem_foreground_number2 = 56,
        _emblem_foreground_number3 = 57,
        _emblem_foreground_number4 = 58,
        _emblem_foreground_number5 = 59,
        _emblem_foreground_number6 = 60,
        _emblem_foreground_number7 = 61,
        _emblem_foreground_number8 = 62,
        _emblem_foreground_number9 = 63
    };

    public enum e_emblem_background : byte
    {
        _emblem_background_solid = 0,
        _emblem_background_vertical_split = 1,
        _emblem_background_horizontal_split1 = 2,
        _emblem_background_horizontal_split2 = 3,
        _emblem_background_vertical_gradient = 4,
        _emblem_background_horizontal_gradient = 5,
        _emblem_background_triple_column = 6,
        _emblem_background_triple_row = 7,
        _emblem_background_quadrants1 = 8,
        _emblem_background_quadrants2 = 9,
        _emblem_background_diagonal_slice = 10,
        _emblem_background_cleft = 11,
        _emblem_background_x1 = 12,
        _emblem_background_x2 = 13,
        _emblem_background_dircle = 14,
        _emblem_background_diamond = 15,
        _emblem_background_cross = 16,
        _emblem_background_square = 17,
        _emblem_background_dual_half_circle = 18,
        _emblem_background_triangle = 19,
        _emblem_background_diagonal_quadrant = 20,
        _emblem_background_three_quaters = 21,
        _emblem_background_quarter = 22,
        _emblem_background_four_rows1 = 23,
        _emblem_background_four_rows2 = 24,
        _emblem_background_split_circle = 25,
        _emblem_background_one_third = 26,
        _emblem_background_two_thirds = 27,
        _emblem_background_upper_field = 28,
        _emblem_background_top_and_bottom = 29,
        _emblem_background_center_stripe = 30,
        _emblem_background_left_and_right = 31
    };

    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
    public struct s_emblem_info
    {
        public e_emblem_foreground foreground_emblem;
        public e_emblem_background background_emblem;
        byte emblem_flags;
    };

    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
    public struct s_player_profile
    {
        public e_player_color primary_color;
        public e_player_color secondary_color;
        public e_player_color tertiary_color;
        public e_player_color quaternary_color;
        public e_character_type player_character_type;
        public s_emblem_info emblem_info;
    };

    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
    public struct s_player_profile_traits
    {
        public s_player_profile profile;
        int gap_48;
        int gap_4C;
    };

    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
    public struct s_clan_identifiers
    {
        uint ID_1;
        uint ID_2;
        uint ID_3;
    };

    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public unsafe struct s_player_properties
    {
        [JsonIgnore]
        public fixed char player_name[16];
        [JsonIgnore]
        public int spawn_protection_time;
        [JsonIgnore]
        public fixed byte gap_24[28];
        public s_player_profile_traits profile_traits;
        [JsonIgnore] 
        public fixed char clan_name[16];
        [JsonIgnore] 
        public s_clan_identifiers clan_identifiers;
        public e_game_team team_index;
        public e_handicap player_handicap_level;
        public byte player_displayed_skill;
        public byte player_overall_skill;
        public byte player_is_griefer;
        public byte bungie_user_role;
        public byte achievement_flags;
        public byte unk2;
    }

    public class real_time_player_stats
    {
        public s_player_properties player;
        public s_game_stats game_stats;
        public s_medal_stats medal_stats;
        public Dictionary<string, weapon_stat.s_weapon_stat> weapon_stats;
        [JsonIgnore]
        public long game_addr;
        [JsonIgnore]
        public long medal_addr;
        [JsonIgnore]
        public long weapon_addr;
        public int player_index;

        public static real_time_player_stats get(int player_index)
        {
            real_time_player_stats result = new real_time_player_stats();

            result.player_index = player_index;

            //result.name =
            //    Program.memory.ReadStringUnicode(Program.exec_resolver["session_players"].address + player_index * 0xA4,
            //        16);
            
            result.player = Program.memory.ReadStruct<s_player_properties>(Program.exec_resolver["session_players"].address + player_index * 0xA4);
            //result.name = game_state_player.name(player_index);
            Dictionary<string, weapon_stat.s_weapon_stat> t_stats = new Dictionary<string, weapon_stat.s_weapon_stat>();
            
            for (int i = 0; i < weapon_stat.weapon_list.Count; i++)
                t_stats.Add(weapon_stat.weapon_list[i], weapon_stat.get_weapon_stats(player_index, i));
            
            result.weapon_stats = t_stats;
            result.weapon_addr = weapon_stat.get_addr(player_index);

            result.game_stats = objects.game_stats.get(player_index);
            result.game_addr = objects.game_stats.get_addr(player_index);

            result.medal_stats = objects.medal_stats.get(player_index);
            result.medal_addr = objects.medal_stats.get_addr(player_index);

            return result;
        }

        public static string GetPlayerNameExplicit(int player_index) => 
            Program.memory.ReadStringUnicode(Program.exec_resolver["session_players"].address + player_index * 0xA4, 16);

        public unsafe string GetPlayerName()
        {
            fixed (char* namePtr = this.player.player_name)
            {
                return new string(namePtr);
            }
        }
    }
}
