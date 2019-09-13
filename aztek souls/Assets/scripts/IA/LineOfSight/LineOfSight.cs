using System;
using UnityEngine;

namespace IA.LineOfSight
{
    public static class LineOfSightEntity
    {
        /// <summary>
        /// Crea una instancia nueva de LineOfSight basándose en cualquier UnityEngine.Transform
        /// </summary>
        /// <param name="range">Rango máximo de la visión</param>
        /// <param name="angle">Ángulo máximo de la visión</param>
        public static LineOfSight CreateSightEntity(this Transform _origin, float range = 1, float angle = 1)
        {
            return new LineOfSight(_origin, range, angle);
        }
    }

    [Serializable]
    public class LineOfSight
    {
        public LayerMask visibles = ~0;

        public Transform target;
        public Transform origin;
        public float range;
        public float angle;

        /// <summary>
        /// El vector resultante de la resta de ambas posiciones: B - A.
        /// </summary>
        [Tooltip("El vector resultante de la resta de ambas posiciones: B - A.")]
        public Vector3 positionDiference = Vector3.zero;
        /// <summary>
        /// Dirección normalizada hacia el objetivo.
        /// </summary>
        [Tooltip("Dirección normalizada hacia el objetivo.")]
        public Vector3 dirToTarget = Vector3.zero;
        /// <summary>
        /// Último ángulo calculado entre la posición de origen y el objetivo.
        /// </summary>
        [Tooltip("Último ángulo calculado entre la posición de origen y el objetivo.")]
        public float angleToTarget = 0;
        /// <summary>
        /// Última distancia calculada entre la posición de origen y el objetivo.
        /// </summary>
        [Tooltip("Última distancia calculada entre la posición de origen y el objetivo.")]
        public float distanceToTarget = 0;


        /// <summary>
        /// Crea un nueva Línea de visión.
        /// </summary>
        /// <param name="origin">El origen de coordenadas para el cálculo de visión</param>
        /// <param name="range">La distancia máxima de la visión</param>
        /// <param name="angle">El ángulo máximo de visión</param>
        public LineOfSight(Transform origin, float range, float angle)
        {
            this.origin = origin;
            this.range = range;
            this.angle = angle;
            LayerMask visibility = ~0;
        }
        /// <summary>
        /// Crea un nueva Línea de visión.
        /// </summary>
        /// <param name="origin">El origen de coordenadas para el cálculo de visión</param>
        /// <param name="target">Objetivo a comprobar</param>
        /// <param name="range">La distancia máxima de la visión</param>
        /// <param name="angle">El ángulo máximo de visión</param>
        /// <param name="v">Elementos visibles para esta entidad</param>
        public LineOfSight(Transform origin, Transform target, float range, float angle, LayerMask v)
        {
            this.target = target;
            this.origin = origin;
            this.range = range;
            this.angle = angle;
            visibles = v;
        }

        /// <summary>
        /// Guarda una referencia a un objetivo recurrente.
        /// Habilita el uso de .IsInSight sin necesidad de especificar un objetivo.
        /// </summary>
        /// <param name="target">Objetivo recurrente</param>
        public LineOfSight setUniqueTarget(Transform target)
        {
            this.target = target;
            return this;
        }

        /// <summary>
        /// Indica si un objetivo está dentro de la línea de visión
        /// </summary>
        /// <returns> verdadero si el objetivo recurrente está dentro de la línea de visión</returns>
        public bool IsInSight()
        {
            Update(this.target);

            if (distanceToTarget > range || angleToTarget > angle) return false;

            RaycastHit hitInfo;
            if (Physics.Raycast(origin.position, dirToTarget, out hitInfo, range, visibles))
                return hitInfo.transform == target;

            return true;
        }
        /// <summary>
        /// Indica si el objetivo específicado está dentro de la línea de visión
        /// </summary>
        /// <param name="target">Objetivo a comprobar</param>
        /// <returns>Verdadero si el Objetivo específicado está dentro de la línea de visión</returns>
        public bool IsInSight(Transform target)
        {
            Update(target);

            if (distanceToTarget > range || angleToTarget > angle) return false;

            RaycastHit hitInfo;
            if (Physics.Raycast(origin.position, dirToTarget, out hitInfo, range, visibles))
                return hitInfo.transform == target;

            return true;
        }

        /// <summary>
        /// Actualiza el estado de la línea de visión.
        /// </summary>
        public void Update(Transform target = null)
        {
            if (target == null) target = this.target;

            positionDiference = target.position - origin.transform.position;
            positionDiference.y = 0;
            distanceToTarget = positionDiference.magnitude;

            Vector3 originPos = origin.transform.forward;
            originPos.y = 0;

            angleToTarget = Vector3.Angle(originPos.normalized, positionDiference.normalized);

            dirToTarget = positionDiference.normalized;
        }
    }
}
