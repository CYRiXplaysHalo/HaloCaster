using DocumentFormat.OpenXml.VariantTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WhatTheFuck.objects;

namespace xemuh2stats.objects
{
    public unsafe struct s_game_state_player
    {
        public ushort datum_salt;
        public ushort flags;
        public ulong identifier;
        public int player_creation_tick;
        public fixed byte machine_identifier[6];
        public short machine_index;
        public int machine_user_index;
        public int machine_controller_index;
        public int controller_index;
        public short user_index;
        public short player_bsp_location_index;
        public uint unit_index;
        public uint dead_unit_index;
        public uint possibly_datum;
        public int InputFlags;
        public int InputFlags2;
        public fixed byte field_3C[4];
        public s_player_properties properties_1;
        public fixed byte pad_1[12];
        public s_player_properties properties_2;
        public int field_148;
        public int respawn_penalty;
        public int teleporter_blocked_tick_count;
        public int respawn_time;
        public int unk_12;
        public fixed byte gap_15C[4];
        public int field_160;
        public short field_164;
        public fixed byte gap_166[14];
        public int betrayal_encountered_tick;
        public int spawn_protection_time;
        public fixed short field_17C[2];
        public float unit_speed;
        public int field_184;
        public fixed byte gap_188[2];
        public short field_18A;
        public fixed byte gap_18C[2];
        public short field_18E;
        public short player_lives_count;
        public fixed byte gap_192[2];
        public int betraying_player_index;
        public fixed byte gap_198[2];
        public int field_19C;
        public fixed byte gap_19E[30];
        public int field_1BC;
        public fixed byte gap_1C0[60];
        public short random_index;
        public fixed byte gap_1FE[2];
        public int is_chatting;

        public unsafe string GetPlayerName()
        {
            fixed (char* namePtr = this.properties_1.player_name)
            {
                return new string(namePtr);
            }
        }
    }

    public static class game_state_player
    {
        public static string name(int index)
        {
            var addr = (Program.game_state_resolver["game_state_players"].address + 0x90) + index * 540;
            return Program.memory.ReadStringUnicode(addr, 16);
        }

        public static s_game_state_player get(int index)
        {
            var addr = (Program.game_state_resolver["game_state_players"].address + 0x4C) + index * 540;
            return Program.memory.ReadStruct<s_game_state_player>(addr);
        }

        public static uint get_player_unit_index(int index)
        {
            s_game_state_player player = get(index);
            return player.unit_index;
        }

        public static long get_player_object(int index)
        {
            s_game_state_player player = get(index);
            long object_address = game_state_object.get_object_address(player.unit_index);
            return object_address;
        }
    }
}
