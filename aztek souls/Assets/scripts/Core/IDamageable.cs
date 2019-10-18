﻿/* Determina Comportamiento común para todas los tipos de Unidades. */
using System;

namespace Core.Entities
{
    // Todas las unidades que pueden recibir Daño.
    public interface IDamageable<EntryHitData, ExitHitData>
    {
        ExitHitData Hit(EntryHitData EntryData);
        EntryHitData GetDamageStats();
        void FeedHitResult(ExitHitData result);
    }
    //Todas las unidades que pueden recibir Daño y tienen 2 estados Básicos: vivos o muertos.
    //public interface IKilleable<EntryHitData, ExitHitData> : IDamageable<EntryHitData>
    //{
    //    //bool IsAlive { get; }
    //    //bool invulnerable { get; }

    //    ExitHitData Hit(EntryHitData EntryData);
    //    EntryHitData GetDamageStats();
    //}
    ////Todas las Unidades que pueden tener estadísticas de combate.
    //public interface IAttacker<>
    //{
    //    //void OnHitConfirmed(HitData data);
    //    //void OnHitBlocked(HitData data);
    //    //void OnKillConfirmed(HitData data);
    //}
}

namespace Core
{
    public struct HitData
    {
        public float Damage;
        public bool BreakDefence;

        public static HitData Empty()
        {
            return new HitData() { Damage = 0, BreakDefence = false };
        }
    }
    public struct HitResult
    {
        public bool TargetEliminated;
        public bool HitConnected;
        public bool HitBlocked;
        public int bloodEarned;

        public static HitResult Empty()
        {
            return new HitResult() { TargetEliminated = false, HitConnected = false, HitBlocked = false, bloodEarned = 0};
        }
    }
}


