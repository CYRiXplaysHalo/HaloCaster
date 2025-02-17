using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using WhatTheFuck.extensions;
using xemuh2stats;
using xemuh2stats.objects;
using static xemuh2stats.objects.weapon_stat;

namespace WhatTheFuck.objects
{
    public enum e_game_results_event_type : byte
    {
        _game_results_event_type_none,
        _game_results_event_type_kill,
        _game_results_event_type_carry,
        _game_results_event_type_score,
        k_game_results_event_type_count
    };

    public enum e_game_results_carry_type : int
    {
        _game_results_carry_type_none,
        [Display(Name = "Flag")]
        _game_results_carry_type_flag,
        [Display(Name = "Bomb")]
        _game_results_carry_type_bomb,
        [Display(Name = "Oddball")]
        _game_results_carry_type_ball,
        k_game_results_carry_type_count
    }

    public enum e_game_results_score_type : int
    {
        _game_results_carry_type_none,
        _game_results_carry_type_flag,
        _game_results_carry_type_bomb,
        _game_results_carry_type_ball,
        k_game_results_carry_type_count
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct s_game_result_event_player_kill
    {
        [FieldOffset(0)]
        public Vector3 source_player_position;

        [FieldOffset(12)]
        public Vector3 effected_player_position;

        [FieldOffset(24)]
        public e_weapon_statistic_type statistic_index;
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct s_game_result_event_player_score
    {
        [FieldOffset(0)]
        public Vector3 scoring_player_position;

        [FieldOffset(12)]
        public e_game_team team_index;

        [FieldOffset(16)]
        public int score_type;

        [FieldOffset(20)]
        public long pad; // 8 bytes of padding
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct s_game_result_event_player_carry
    {
        [FieldOffset(0)]
        public Vector3 source_player_position;

        [FieldOffset(12)]
        public e_game_team team_index;

        [FieldOffset(16)]
        public e_game_results_carry_type carry_type;

        [FieldOffset(20)]
        public long pad; // 8 bytes of padding
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct s_game_result_event_data
    {
        [FieldOffset(0)]
        public s_game_result_event_player_kill kill_event;

        [FieldOffset(0)]
        public s_game_result_event_player_score score_event;

        [FieldOffset(0)]
        public s_game_result_event_player_carry carry_event;
    }

    [StructLayout(LayoutKind.Explicit, Size = 36)]
    public struct s_game_result_event
    {
        [FieldOffset(0)]
        public e_game_results_event_type type;

        [FieldOffset(1)]
        public byte source_player_index;

        [FieldOffset(2)]
        public byte effected_player_index;

        [FieldOffset(3)]
        public sbyte pad_3;

        [FieldOffset(4)]
        public s_game_result_event_data data;

        [FieldOffset(32)]
        public int time_stamp;
    }

    public static class game_event_monitor
    {
        private static Dictionary<uint, long> address_cache = new Dictionary<uint, long>();
        private static List<Action<string>> event_callbacks = new List<Action<string>>();
        private static uint last_event_record_id = 0;
        private static uint start_time = 0;
        private static DateTime start;
        
        public static Dictionary<uint, s_game_result_event> event_log = new Dictionary<uint, s_game_result_event>();

        public static void reset()
        {
            address_cache.Clear();
            event_log.Clear();
            last_event_record_id = 0;
            start_time = 0;
        }

        public static void read_events()
        {
            uint memEventCount = Program.memory.ReadUInt(Program.exec_resolver["game_results_globals"].address - 0xC);

            if (start_time == 0)
            {
                start_time = Program.memory.ReadUInt(Program.exec_resolver["game_results_globals"].address + 0x374);
                start = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                start = start.AddSeconds(start_time).ToLocalTime();
            }

            ulong eventReadCount;

            if (memEventCount >= last_event_record_id)
            {
                eventReadCount = memEventCount - last_event_record_id;
            }
            else
            {
                // Handle overflow
                eventReadCount = ((ulong)memEventCount + (ulong)UInt32.MaxValue + 1) - last_event_record_id;
            }

            if (eventReadCount > 0)
            {
                // Limit to 1000 to match buffer size
                if (eventReadCount > 1000)
                {
                    eventReadCount = 1000;
                    last_event_record_id = memEventCount - 1000;
                }

                for (uint i = 0; i < eventReadCount; i++)
                {
                    uint eventIndex = (last_event_record_id + i) % UInt32.MaxValue;
                    uint bufferIndex = (eventIndex % 1000u);

                    long eventAddress = (Program.exec_resolver["game_results_globals"].address + 0x24F84) + (bufferIndex * 36);
                    s_game_result_event gameEvent = Program.memory.ReadStruct<s_game_result_event>(eventAddress);

                    event_log.Add(eventIndex, gameEvent);
                    foreach (var eventCallback in event_callbacks)
                    {
                        eventCallback.Invoke(format_event(gameEvent));
                    }
                }

                last_event_record_id = memEventCount;
            }
        }

        public static void add_event_callbaack(Action<string> callback)
        {
            event_callbacks.Add(callback);
        }

        public static string format_event(uint event_id)
        {
            if (event_id > last_event_record_id)
                return "";

            s_game_result_event game_event = event_log[event_id];

            return format_event(game_event);
        }

        public static string format_event(s_game_result_event game_event)
        {
            string message_base = $"[{start.AddSeconds(game_event.time_stamp):h:mm:ss}]\t";
            game_event.

            switch (game_event.type)
            {
                case e_game_results_event_type._game_results_event_type_kill:
                    return $"{message_base} {real_time_player_stats.GetPlayerNameExplicit(game_event.source_player_index)} Killed {real_time_player_stats.GetPlayerNameExplicit(game_event.effected_player_index)} with {game_event.data.kill_event.statistic_index.GetDisplayName()}";
                    break;
                case e_game_results_event_type._game_results_event_type_score:
                    return $"{message_base} {real_time_player_stats.GetPlayerNameExplicit(game_event.source_player_index)} scored the {game_event.data.score_event.score_type}";
                    break;
                case e_game_results_event_type._game_results_event_type_carry:
                    return $"{message_base} {real_time_player_stats.GetPlayerNameExplicit(game_event.source_player_index)} is carrying {game_event.data.carry_event.carry_type.GetDisplayName()}";
                    break;
            }

            return "";
        }

        public static IEnumerable<s_game_result_event> get_new_events(uint last_event_id, out uint event_id)
        {
            var results =  event_log.Where(x => x.Key > last_event_id).Select(x => x.Value).AsEnumerable();
            event_id = last_event_record_id;
            return results;
        }
    }
}
