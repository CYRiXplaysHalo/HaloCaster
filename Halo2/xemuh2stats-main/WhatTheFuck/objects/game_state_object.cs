using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WhatTheFuck.enums;
using xemuh2stats;

namespace WhatTheFuck.objects
{
    public static class game_state_object
    {
        private static Dictionary<uint, long> object_address_cache = new Dictionary<uint, long>();

        public static void clear_cache()
        {
            object_address_cache.Clear();
        }

        public static long get_object_address(uint id)
        {
            if(object_address_cache.ContainsKey(id)) return object_address_cache[id];

            if (id == uint.MaxValue) return uint.MaxValue;

            short abs_index = (short)(id & 0xFFFF);
            var addr = Program.game_state_resolver["game_state_objects"].address + 0x4C + (0xC * abs_index) + 8;
            var t_addr = Program.memory.ReadUInt(addr);
            var r_addr = (long)Program.qmp.Translate(t_addr);
            
            object_address_cache.Add(id, r_addr);

            return r_addr;
        }

        public static Vector3 get_object_position(uint object_datum)
        {
            var unit_addr = get_object_address(object_datum);

            if(unit_addr == uint.MaxValue)
                return Vector3.Zero;

            Vector3 result =
                Program.memory.ReadStruct<Vector3>(unit_addr + 0x30);

            return result;

        }


        public static DamageReportingType unit_object_get_weapon_type(uint unit_datum)
        {
            var unit_addr = get_object_address(unit_datum);

            if (unit_addr == uint.MaxValue)
                return DamageReportingType.Guardians;

            var unit_inventory = new List<uint>();

            for (var i = 0; i < 4; i++)
                unit_inventory.Add(Program.memory.ReadUInt(unit_addr + 536 + (i * 4)));

            var unit_weapon_slot = Program.memory.ReadByte(unit_addr + 534);

            var weapon_addr = get_object_address(unit_inventory[unit_weapon_slot]);

            if (weapon_addr == uint.MaxValue)
                return DamageReportingType.Guardians;

            var weapon_datum = Program.memory.ReadUInt(weapon_addr);

            var weapon_tag_addr = cache_file_tags.get_tag_address(weapon_datum);

            if (weapon_datum == uint.MaxValue)
                return DamageReportingType.Guardians;

            var barrels_addr = weapon_tag_addr + 0x2D0;
            var barrels_count = Program.memory.ReadUInt(barrels_addr);

            if (barrels_count == 0)
            {
                DamageReportingType type = (DamageReportingType) Program.memory.ReadByte(weapon_tag_addr + 0x1FC);
                return type;
            }
            else
            {

                var barrels_data = Program.memory.ReadUInt(barrels_addr + 4);

                long barrel_addr = 0;

                if (object_address_cache.ContainsKey(barrels_data))
                    barrel_addr = object_address_cache[barrels_data];
                else
                {
                    var t_barrel = (long) Program.qmp.Translate(barrels_data);
                    object_address_cache.Add(barrels_data, t_barrel);
                    barrel_addr = t_barrel;
                }

                var barrel_projectile_datum = Program.memory.ReadUInt(barrel_addr + 0x90);

                var projectile_addr = cache_file_tags.get_tag_address(barrel_projectile_datum);

                if (projectile_addr == uint.MaxValue)
                    return DamageReportingType.Guardians;

                DamageReportingType type = (DamageReportingType) Program.memory.ReadByte(projectile_addr + 0x128);

                return type;
            }
        }
    }
}
