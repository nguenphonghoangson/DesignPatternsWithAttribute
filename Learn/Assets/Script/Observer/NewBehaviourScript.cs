using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameDevPatterns
{
    #region Command Pattern
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public KeyCode Key { get; }

        public CommandAttribute(string name, KeyCode key = KeyCode.None)
        {
            Name = name;
            Key = key;
        }
    }

    public class CommandManager : MonoBehaviour
    {
        private Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
        private Dictionary<KeyCode, string> keyBindings = new Dictionary<KeyCode, string>();
        private object[] emptyParameters = new object[0];

        private void Awake()
        {
            RegisterCommandsInScene();
        }

        private void RegisterCommandsInScene()
        {
            var monoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in monoBehaviours)
            {
                var methods = mb.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);

                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<CommandAttribute>();
                    commands[attr.Name] = method;
                    if (attr.Key != KeyCode.None)
                    {
                        keyBindings[attr.Key] = attr.Name;
                    }
                }
            }
        }

        private void Update()
        {
            foreach (var binding in keyBindings)
            {
                if (Input.GetKeyDown(binding.Key))
                {
                    ExecuteCommand(binding.Value);
                }
            }
        }

        public void ExecuteCommand(string commandName)
        {
            if (commands.TryGetValue(commandName, out var method))
            {
                method.Invoke(method.DeclaringType, emptyParameters);
            }
        }
    }

    #endregion

    #region Object Pooling
    [AttributeUsage(AttributeTargets.Class)]
    public class PooledObjectAttribute : Attribute
    {
        public int InitialSize { get; }
        public int MaxSize { get; }

        public PooledObjectAttribute(int initialSize = 10, int maxSize = 100)
        {
            InitialSize = initialSize;
            MaxSize = maxSize;
        }
    }

    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    public class ObjectPool : MonoBehaviour
    {
        private Dictionary<Type, Queue<Component>> pools = new Dictionary<Type, Queue<Component>>();
        private Dictionary<Type, PooledObjectAttribute> poolSettings = new Dictionary<Type, PooledObjectAttribute>();

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<PooledObjectAttribute>() != null);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<PooledObjectAttribute>();
                poolSettings[type] = attr;
                pools[type] = new Queue<Component>();

                for (int i = 0; i < attr.InitialSize; i++)
                {
                    CreateNewInstance(type);
                }
            }
        }

        private void CreateNewInstance(Type type)
        {
            var obj = new GameObject(type.Name);
            var component = obj.AddComponent(type) as Component;
            obj.SetActive(false);
            pools[type].Enqueue(component);
        }

        public T Spawn<T>(Vector3 position) where T : Component
        {
            var type = typeof(T);
            if (!pools.ContainsKey(type))
                return null;

            if (pools[type].Count == 0)
            {
                var settings = poolSettings[type];
                if (pools[type].Count < settings.MaxSize)
                {
                    CreateNewInstance(type);
                }
            }

            var component = pools[type].Dequeue();
            var poolable = component as IPoolable;
            
            component.transform.position = position;
            component.gameObject.SetActive(true);
            poolable?.OnSpawn();

            return component as T;
        }

        public void Despawn<T>(T component) where T : Component
        {
            var type = typeof(T);
            if (!pools.ContainsKey(type))
                return;

            var poolable = component as IPoolable;
            poolable?.OnDespawn();
            
            component.gameObject.SetActive(false);
            pools[type].Enqueue(component);
        }
    }

    #endregion

    #region State Pattern
    [AttributeUsage(AttributeTargets.Class)]
    public class StateAttribute : Attribute
    {
        public string StateName { get; }
        public bool IsDefault { get; }

        public StateAttribute(string stateName, bool isDefault = false)
        {
            StateName = stateName;
            IsDefault = isDefault;
        }
    }

    public interface IState
    {
        void Enter();
        void Exit();
        void Update();
    }

    public class StateMachine : MonoBehaviour
    {
        private Dictionary<string, Type> stateTypes = new Dictionary<string, Type>();
        private Dictionary<Type, IState> stateInstances = new Dictionary<Type, IState>();
        private IState currentState;

        private void Awake()
        {
            RegisterStates();
        }

        private void RegisterStates()
        {
            var stateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<StateAttribute>() != null);

            foreach (var type in stateTypes)
            {
                var attr = type.GetCustomAttribute<StateAttribute>();
                this.stateTypes[attr.StateName] = type;

                if (attr.IsDefault)
                {
                    ChangeState(attr.StateName);
                }
            }
        }

        public void ChangeState(string stateName)
        {
            if (!stateTypes.ContainsKey(stateName))
                return;

            var stateType = stateTypes[stateName];
            
            currentState?.Exit();

            if (!stateInstances.ContainsKey(stateType))
            {
                stateInstances[stateType] = Activator.CreateInstance(stateType) as IState;
            }

            currentState = stateInstances[stateType];
            currentState.Enter();
        }

        private void Update()
        {
            currentState?.Update();
        }
    }

    #endregion

    

    // Example Usage
    public class PlayerController : MonoBehaviour
    {
        [Command("Jump", KeyCode.Space)]
        private void Jump()
        {
            Debug.Log("Player Jumped!");
        }

        [Event(nameof(OnPlayerDamaged))]
        private void OnPlayerDamaged(int damage)
        {
            Debug.Log($"Player took {damage} damage!");
        }
        [Event(nameof(OnPlayerDamaged))]
        private void OnPlayerDamaged(string damage)
        {
            Debug.Log($"Player took {damage} damage!");
        }
        [Event(nameof(OnPlayerDamaged))]
        private string OnPlayerDamaged()
        {
            return string.Empty;
        }
    }

    [PooledObject(initialSize: 20, maxSize: 50)]
    public class Bullet : MonoBehaviour, IPoolable
    {
        public void OnSpawn()
        {
            // Initialize bullet
        }

        public void OnDespawn()
        {
            // Clean up bullet
        }
    }

    [State("Idle", isDefault: true)]
    public class IdleState : IState
    {
        public void Enter() => Debug.Log("Entering Idle State");
        public void Exit() => Debug.Log("Exiting Idle State");
        public void Update() { }
    }

    [State("Running")]
    public class RunningState : IState
    {
        public void Enter() => Debug.Log("Entering Running State");
        public void Exit() => Debug.Log("Exiting Running State");
        public void Update() { }
    }
}