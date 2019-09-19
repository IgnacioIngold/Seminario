using System.Collections.Generic;

namespace IA.RandomSelections
{
    public static class RoulleteSelection
    {
        /// <summary>
        /// Ejecuta una ruleta que devuelve un índice aleatório.
        /// </summary>
        /// <param name="NumberCollection"></param>
        /// <returns>Retorna un índice aleatorio.</returns>
        public static int Roll(IEnumerable<float> NumberCollection)
        {
            float Sum = 0;
            float RandomIndex = UnityEngine.Random.Range(0f, 1f);
            foreach (var Numero in NumberCollection)
                Sum += Numero;

            List<float> newValues = new List<float>();
            foreach (var Numero in NumberCollection)
                newValues.Add(Numero / Sum);
            Sum = 0;

            for (int i = 0; i < newValues.Count; i++)
            {
                Sum += newValues[i];
                if (Sum > RandomIndex) return i;
            }
            return -1;
        }
    }
}
