using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WhatTheFuck.enums;

namespace xemuh2stats.objects
{
    public static class weapon_stat
    {
        public enum e_weapon_statistic_type
        {
            Guardians = 0,
            FallingDamage = 1,
            GenericCollisionDamage = 2,
            GenericMeleeDamage = 3,
            GenericExplosion = 4,
            Magnum = 5,
            PlasmaPistol = 6,
            Needler = 7,
            SMG = 8,
            PlasmaRifle = 9,
            BattleRifle = 10,
            Carbine = 11,
            Shotgun = 12,
            SniperRifle = 13,
            BeamRifle = 14,
            BrutePlasmaRifle = 15,
            RocketLauncher = 16,
            FlakCannon = 17,
            BruteShot = 18,
            Disintegrator = 19,
            SentinelBeam = 20,
            SentinelRPG = 21,
            EnergySword = 22,
            FragGrenade = 23,
            PlasmaGrenade = 24,
            FlagMeleeDamage = 25,
            BombMeleeDamage = 26,
            BallMeleeDamage = 27,
            HumanTurret = 28,
            PlasmaTurret = 29,
            Banshee = 30,
            Ghost = 31,
            Mongoose = 32,
            Scorpion = 33,
            SpectreDriver = 34,
            SpectreGunner = 35,
            WarthogDriver = 36,
            WarthogGunner = 37,
            Wraith = 38,
            Tank = 39,
            BombExplosionDamage = 40
        }

        public static List<string> weapon_list = new List<string>()
        {
            "magnum",
            "plasma pistol",
            "needler",
            "SMG",
            "plasma rifle",
            "battle rifle",
            "carbine",
            "shotgun",
            "sniper rifle",
            "beam rifle",
            "brute plasma rifle",
            "rocket launcher",
            "fuel rod",
            "brute shot",
            "unused 1",
            "sentinal beam",
            "unused 2",
            "energy sword",
            "frag grenade",
            "plasma grenade",
            "flag",
            "bomb",
            "oddball",
        };

        [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1)]
        public struct s_weapon_stat
        {
            public ushort kills;
            public ushort deaths;
            public ushort suicide;
            public ushort shots_fired;
            public ushort shots_hit;
            public ushort head_shots;
        }


        public static s_weapon_stat get_weapon_stats(int player_index, int weapon_index)
        {
            var addr = Program.exec_resolver["weapon_stats"].address + (player_index * 0x36a) + (weapon_index * 0x10);

            if (player_index > 4)
                addr = Program.exec_resolver["game_results_globals_extra"].address + 0xDE +
                       ((player_index - 5) * 0x36A) + (weapon_index * 0x10);

            return new s_weapon_stat()
            {
                kills = Program.memory.ReadUShort(addr),
                deaths = Program.memory.ReadUShort(addr + 2),
                suicide = Program.memory.ReadUShort(addr + 6),
                shots_fired = Program.memory.ReadUShort(addr + 8),
                shots_hit = Program.memory.ReadUShort(addr + 10),
                head_shots = Program.memory.ReadUShort(addr + 12)
            };
            //var data = Program.memory.ReadMemory(false, Program.game_state_resolver["weapon_stats"].address + (player_index * 0x36a) + (weapon_index * 0x10), 0x10);

            // return ReadStruct<s_weapon_stat>(data, 0, 0x10);
        }

        public static long get_addr(int player_index)
        {
            return Program.exec_resolver["weapon_stats"].address + (player_index * 0x36a);
        }

        public static int damage_reporting_type_to_results_index(DamageReportingType type)
        {
            var result = 0;
            switch (type)
            {
                case DamageReportingType.Guardians: // case 0
                    result = 0;
                    break;
                case DamageReportingType.FallingDamage: // case 1
                    result = 1;
                    break;
                case DamageReportingType.GenericCollisionDamage: // case 2
                    result = 2;
                    break;
                case DamageReportingType.GenericMeleeDamage: // case 3
                    result = 3;
                    break;
                case DamageReportingType.GenericExplosion: // case 4
                    result = 4;
                    break;
                case DamageReportingType.MagnumPistol: // case 5
                    result = 5;
                    break;
                case DamageReportingType.PlasmaPistol: // case 6
                    result = 6;
                    break;
                case DamageReportingType.Needler: // case 7
                    result = 7;
                    break;
                case DamageReportingType.SMG: // case 8
                    result = 8;
                    break;
                case DamageReportingType.PlasmaRifle: // case 9
                    result = 9;
                    break;
                case DamageReportingType.BattleRifle: // case 10
                    result = 10;
                    break;
                case DamageReportingType.Carbine: // case 11
                    result = 11;
                    break;
                case DamageReportingType.Shotgun: // case 12
                    result = 12;
                    break;
                case DamageReportingType.SniperRifle: // case 13
                    result = 13;
                    break;
                case DamageReportingType.BeamRifle: // case 14
                    result = 14;
                    break;
                case DamageReportingType.RocketLauncher: // case 15
                    result = 16;
                    break;
                case DamageReportingType.FlakCannon: // case 16
                    result = 17;
                    break;
                case DamageReportingType.BruteShot: // case 17
                    result = 18;
                    break;
                case DamageReportingType.Disintegrator: // case 18
                    result = 19;
                    break;
                case DamageReportingType.BrutePlasmaRifle: // case 19
                    result = 15;
                    break;
                case DamageReportingType.EnergySword: // case 20
                    result = 22;
                    break;
                case DamageReportingType.FragGrenade: // case 21
                    result = 23;
                    break;
                case DamageReportingType.PlasmaGrenade: // case 22
                    result = 24;
                    break;
                case DamageReportingType.FlagMeleeDamage: // case 23
                    result = 25;
                    break;
                case DamageReportingType.BombMeleeDamage: // case 24
                    result = 26;
                    break;
                case DamageReportingType.BombExplosionDamage: // case 25
                    result = 40;
                    break;
                case DamageReportingType.BallMeleeDamage: // case 26
                    result = 27;
                    break;
                case DamageReportingType.HumanTurret: // case 27
                    result = 28;
                    break;
                case DamageReportingType.PlasmaTurret: // case 28
                    result = 29;
                    break;
                case DamageReportingType.Banshee: // case 29
                    result = 30;
                    break;
                case DamageReportingType.Ghost: // case 30
                    result = 31;
                    break;
                case DamageReportingType.Mongoose: // case 31
                    result = 32;
                    break;
                case DamageReportingType.Scorpion: // case 32
                    result = 33;
                    break;
                case DamageReportingType.SpectreDriver: // case 33
                    result = 34;
                    break;
                case DamageReportingType.SpectreGunner: // case 34
                    result = 35;
                    break;
                case DamageReportingType.WarthogDriver: // case 35
                    result = 36;
                    break;
                case DamageReportingType.WarthogGunner: // case 36
                    result = 37;
                    break;
                case DamageReportingType.Wraith: // case 37
                    result = 38;
                    break;
                case DamageReportingType.Tank: // case 38
                    result = 39;
                    break;
                case DamageReportingType.SentinelBeam: // case 39
                    result = 20;
                    break;
                case DamageReportingType.SentinelRPG: // case 40
                    result = 21;
                    break;
                default:
                    return result;
            }

            return result;
        }
    }
}
