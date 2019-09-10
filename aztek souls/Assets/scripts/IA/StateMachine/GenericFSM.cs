using System;
using System.Collections.Generic;

namespace IA.StateMachine.Generic
{
    public class GenericFSM<T>
    {
        public State<T> current;

        public GenericFSM(State<T> initialState)
        {
            current = initialState;
            current.Enter(default(T));
        }

        public void Update()
        {
            current.Update();
        }

        public void Feed(T input)
        {
            var transition = current.GetTransition(input);
            if (transition != null)
            {
//#if (UNITY_EDITOR)
//                UnityEngine.MonoBehaviour.print("Transitioning from: " + current.StateName + " to: " + transition.Item2.StateName);
//#endif

                current.Exit(input);
                transition.Item1(input);
                current = transition.Item2;
                current.Enter(input);
            }
        }
    }

    public class State<T>
    {
        public string StateName { get; private set; }

        public event Action<T> OnEnter = delegate { };
        public event Action OnUpdate = delegate { };
        public event Action OnLateUpdate = delegate { };
        public event Action OnFixedUpdate = delegate { };
        public event Action<T> OnExit = delegate { };

        private Dictionary<T, Transition<T>> transitions = new Dictionary<T, Transition<T>>();
        /// <summary>
        /// Clase interna que permite almacenar un evento intermedio entre un estado y otro.
        /// </summary>
        class Transition<D>
        {
            public Action<D> OnTransition = delegate { };
            public D Input { get; private set; }
            public State<D> TargetState { get; private set; }

            public Transition(D input, State<D> targetState)
            {
                Input = input;
                TargetState = targetState;
            }
            public Transition(D input, State<D> targetState, Action<D> OnTransition)
            {
                Input = input;
                TargetState = targetState;
                this.OnTransition += OnTransition;
            }
        }

        public State(string name)
        {
            StateName = name;
        }

        /// <summary>
        /// Añade una transición simple hacia otro estado.
        /// </summary>
        /// <param name="key">Tipo de dato que identifica el estado a la cual haremos la transición.</param>
        /// <param name="nextState">El estado en el que terminaremos despues de la transición.</param>
        public State<T> AddTransition(T key, State<T> nextState)
        {
            if (!transitions.ContainsKey(key))
                transitions.Add(key, new Transition<T>(key, nextState));
            return this;
        }
        /// <summary>
        /// Añade una transición hacia otro estado, con un evento intermedio.
        /// </summary>
        /// <param name="key">Tipo de dato que identifica el estado a la cual haremos la transición.</param>
        /// <param name="nextState">El estado en el que terminaremos despues de la transición.</param>
        /// <param name="OnTransition"></param>
        /// <returns></returns>
        public State<T> AddTransition(T key, State<T> nextState, Action<T> OnTransition)
        {
            if (!transitions.ContainsKey(key))
                transitions.Add(key, new Transition<T>(key, nextState, OnTransition));
            return this;
        }
        /// <summary>
        /// Retorna el Evento intermedio y el estado final de la transición identificada por el parámetro dado.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Tupla donde: Action = Función de la transición, State = el estado Objetivo.</returns>
        public Tuple<Action<T>,State<T>> GetTransition(T input)
        {
            if (transitions.ContainsKey(input))
            {
                Transition<T> transition = transitions[input];
                return Tuple.Create(transition.OnTransition, transition.TargetState);
            }
            else return null;
        }

        public bool hasTransitionTo(T input)
        {
            return transitions.ContainsKey(input);
        }

        public void Enter(T input)
        {
            OnEnter(input);
        }
        public void Exit(T input)
        {
            OnExit(input);
        }
        public void Update()
        {
            OnUpdate();
        }
        public void FixedUpdate()
        {
            OnFixedUpdate();
        }
        public void LateUpdate()
        {
            OnLateUpdate();
        }
    }
}
