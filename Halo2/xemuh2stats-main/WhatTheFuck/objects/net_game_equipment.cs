 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatTheFuck.enums;
using WhatTheFuck.extensions;
using xemuh2stats;

namespace WhatTheFuck.objects
{
    public enum equipment_powerup_type : ushort
    {
        None = 0,
        DoubleSpeed = 1,
        Overshield = 2,
        ActiveCamouflage = 3,
        FullSpectrumVision = 4,
        Health = 5,
        Grenade = 6
    }
    public enum netgame_equipment_classification : byte
    {
        Weapon = 0,
        PrimaryLightLand = 1,
        SecondaryLightLand = 2,
        PrimaryHeavyLand = 3,
        PrimaryFlying = 4,
        SecondaryHeavyLand = 5,
        PrimaryTurret = 6,
        SecondaryTurret = 7,
        Grenade = 8,
        Powerup = 9
    }
    public class net_game_equipment_storage
    {
        public uint tag_index;
        public string type;
        public netgame_equipment_classification classification;
        public long candy_monitor_address;
        public int respawn_time;

        public bool is_respawning => Program.memory.ReadUInt(candy_monitor_address) == uint.MaxValue;
        public uint respawn_timer => Program.memory.ReadUInt(candy_monitor_address + 4);
    }
    public static class net_game_equipment
    {
        private static Dictionary<int, net_game_equipment_storage> equipment_cache = new Dictionary<int, net_game_equipment_storage>();
        private static Dictionary<uint, long> address_cache = new Dictionary<uint, long>();
        private static bool cache_built = false;

        public static void clear_cache()
        {
            equipment_cache.Clear();
            address_cache.Clear();
            cache_built = false;
        }

        public static void check_cache()
        {
            if (!cache_built)
            {
                cache_built = true;
                var scenario_addr = cache_file_tags.get_scenario_address();

                var netgame_equipment_addr = scenario_addr + 0x120;
                var netgame_count = Program.memory.ReadUInt(netgame_equipment_addr);

                if (netgame_count > 0)
                {
                    var netgame_data = Program.memory.ReadUInt(netgame_equipment_addr + 4);
                    long netgame_addr = 0;
                    
                    if(address_cache.ContainsKey(netgame_data)) 
                        netgame_addr = address_cache[netgame_data];
                    else
                    {
                        netgame_addr = (long)Program.qmp.Translate(netgame_data);
                        address_cache.Add(netgame_data, netgame_addr);
                    }

                    
                    for (var i = 0; i < netgame_count; i++)
                    {
                        var netgame_item_addr = netgame_addr + 0x90 * i;
                        
                        var store = new net_game_equipment_storage();
                        store.classification = (netgame_equipment_classification)Program.memory.ReadByte(netgame_item_addr + 0x14);
                        store.tag_index = Program.memory.ReadUInt(netgame_item_addr + 0x5C);
                        store.respawn_time = Program.memory.ReadInt(netgame_item_addr + 0x10);
                        store.candy_monitor_address = Program.game_state_resolver["game_engine"].address + 2012 + (8 * i);

                        switch (store.classification)
                        {
                            case netgame_equipment_classification.Weapon:
                            {
                                var collection_addr = cache_file_tags.get_tag_address(store.tag_index);

                                var permutations_count = Program.memory.ReadUInt(collection_addr);
                                if (permutations_count > 0)
                                {
                                    var permuatation_data = Program.memory.ReadUInt(collection_addr + 4);
                                    long permutation_addr = 0;
                                    if(address_cache.ContainsKey(permuatation_data))
                                        permutation_addr = address_cache[permuatation_data];
                                    else
                                    {
                                        permutation_addr = (long)Program.qmp.Translate(permuatation_data);
                                        address_cache.Add(permuatation_data, permutation_addr);
                                    }

                                    var permutation_tag_index = Program.memory.ReadUInt(permutation_addr + 0x8);

                                    var weapon_tag_addr = cache_file_tags.get_tag_address(permutation_tag_index);

                                    var barrels_addr = weapon_tag_addr + 0x2D0;
                                    var barrels_count = Program.memory.ReadUInt(barrels_addr);

                                    if (barrels_count == 0)
                                    {
                                        DamageReportingType type = (DamageReportingType)Program.memory.ReadByte(weapon_tag_addr + 0x1FC);
                                        store.type = type.GetDisplayName();
                                    }
                                    else
                                    {

                                        var barrels_data = Program.memory.ReadUInt(barrels_addr + 4);

                                        long barrel_addr = 0;

                                        if (address_cache.ContainsKey(barrels_data))
                                            barrel_addr = address_cache[barrels_data];
                                        else
                                        {
                                            var t_barrel = (long)Program.qmp.Translate(barrels_data);
                                            address_cache.Add(barrels_data, t_barrel);
                                            barrel_addr = t_barrel;
                                        }

                                        var barrel_projectile_datum = Program.memory.ReadUInt(barrel_addr + 0x90);

                                        var projectile_addr = cache_file_tags.get_tag_address(barrel_projectile_datum);

                                        DamageReportingType type = (DamageReportingType)Program.memory.ReadByte(projectile_addr + 0x128);

                                        store.type = type.GetDisplayName();
                                    }
                                }
                                break;
                            }
                            case netgame_equipment_classification.Powerup:
                            {
                                var collection_addr = cache_file_tags.get_tag_address(store.tag_index);

                                var permutations_count = Program.memory.ReadUInt(collection_addr);
                                if (permutations_count > 0)
                                {
                                    var permuatation_data = Program.memory.ReadUInt(collection_addr + 4);
                                    long permutation_addr = 0;
                                    if (address_cache.ContainsKey(permuatation_data))
                                        permutation_addr = address_cache[permuatation_data];
                                    else
                                    {
                                        permutation_addr = (long) Program.qmp.Translate(permuatation_data);
                                        address_cache.Add(permuatation_data, permutation_addr);
                                    }

                                    var permutation_tag_index = Program.memory.ReadUInt(permutation_addr + 0x8);

                                    var equipment_tag_addr = cache_file_tags.get_tag_address(permutation_tag_index);

                                    var equipment_powerup_type = (equipment_powerup_type) Program.memory.ReadUShort(equipment_tag_addr + 0x12C);

                                    store.type = equipment_powerup_type.GetDisplayName();
                                }

                                break;
                            }
                        }

                        equipment_cache.Add(i, store);
                    }
                }
            }
        }

        public static List<net_game_equipment_storage> get_useful_net_game_items()
        {
            check_cache();
            var list = new List<net_game_equipment_storage>();

            var t = equipment_cache.Where(x => x.Value.classification == netgame_equipment_classification.Powerup);
            foreach (var item in t)
            {
                list.Add(item.Value);
            }

            t = equipment_cache.Where(x => x.Value.type == "Sniper Rifle" || x.Value.type == "Rocket Launcher" || x.Value.type == "Energy Sword");
            foreach (var item in t)
            {
                list.Add(item.Value);
            }

            return list;
        }
    }
}
