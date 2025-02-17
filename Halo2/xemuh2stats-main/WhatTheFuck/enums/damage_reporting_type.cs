using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheFuck.enums
{
    public enum DamageReportingType : byte
    {
        Guardians = 0,
        FallingDamage = 1,
        GenericCollisionDamage = 2,
        GenericMeleeDamage = 3,
        GenericExplosion = 4,
        [Display(Name = "Magnum")]
        MagnumPistol = 5,
        [Display(Name = "Plasma Pistol")]
        PlasmaPistol = 6,
        [Display(Name = "Needler")]
        Needler = 7,
        [Display(Name = "SmG")]
        SMG = 8,
        [Display(Name = "Plasma Rifle")]
        PlasmaRifle = 9,
        [Display(Name = "Battle Rifle")]
        BattleRifle = 10,
        [Display(Name = "Carbine")]
        Carbine = 11,
        [Display(Name = "Shotgun")]
        Shotgun = 12,
        [Display(Name = "Sniper Rifle")]
        SniperRifle = 13,
        [Display(Name = "Beam Rifle")]
        BeamRifle = 14,
        [Display(Name = "Rocket Launcher")]
        RocketLauncher = 15,
        [Display(Name = "Fuel Rod")]
        FlakCannon = 16,
        [Display(Name = "Brute Shot")]
        BruteShot = 17,
        Disintegrator = 18,
        [Display(Name = "Brute Plasma Rifle")]
        BrutePlasmaRifle = 19,
        [Display(Name = "Energy Sword")]
        EnergySword = 20,
        FragGrenade = 21,
        PlasmaGrenade = 22,
        [Display(Name = "Flag")]
        FlagMeleeDamage = 23,
        [Display(Name = "Bomb")]
        BombMeleeDamage = 24,
        BombExplosionDamage = 25,
        [Display(Name = "Oddball")]
        BallMeleeDamage = 26,
        HumanTurret = 27,
        PlasmaTurret = 28,
        Banshee = 29,
        Ghost = 30,
        Mongoose = 31,
        Scorpion = 32,
        SpectreDriver = 33,
        SpectreGunner = 34,
        WarthogDriver = 35,
        WarthogGunner = 36,
        Wraith = 37,
        Tank = 38,
        [Display(Name = "Sentinel Beam")]
        SentinelBeam = 39,
        SentinelRPG = 40,
        Teleporter = 41
    }
}
