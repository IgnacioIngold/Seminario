/* Determina Comportamiento común para todas los tipos de Unidades. */
using System;

namespace Core.Entities
{
    // Todas las unidades que pueden recibir Daño.
    public interface IDamageable
    {
        void GetDamage(params object[] DamageStats);
    }
    //Todas las unidades que pueden recibir Daño y tienen 2 estados Básicos: vivos o muertos.
    public interface IKilleable : IDamageable
    {
        bool IsAlive { get; }
        bool invulnerable { get; }
    }
    //Todas las Unidades que pueden tener estadísticas de combate.
    public interface IAttacker<T>
    {
        void OnHitConfirmed(T data);
        void OnHitBlocked(T data);
        void OnKillConfirmed(T data);
        T GetDamageStats();
    }
}


