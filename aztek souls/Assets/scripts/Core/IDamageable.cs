/* Determina Comportamiento común para todas los tipos de Unidades. */
using System;

namespace Core.Entities
{
    // Todas las unidades que pueden recibir Daño.
    public interface IDamageable<EntryHitData, ExitHitData>
    {
        ExitHitData Hit(EntryHitData EntryData);
        EntryHitData DamageStats();
        void GetHitResult(ExitHitData result);
    }
    //Todas las unidades tienen 2 estados Básicos: vivos o muertos.
    public interface IKilleable
    {
        bool IsAlive { get; }
        bool invulnerable { get; }
    }
}

namespace Core
{
    public struct HitData
    {
        public float Damage;
        public bool BreakDefence;
        public int AttackID;
        public Inputs AttackType;

        public static HitData Default()
        {
            return new HitData() { Damage = 0, BreakDefence = false, AttackID = 0, AttackType = Inputs.light };
        }
    }
    public struct HitResult
    {
        public bool TargetEliminated;
        public bool HitConnected;
        public bool HitBlocked;
        public int bloodEarned;

        public static HitResult Default()
        {
            return new HitResult() { TargetEliminated = false, HitConnected = false, HitBlocked = false, bloodEarned = 0};
        }
    }
}


