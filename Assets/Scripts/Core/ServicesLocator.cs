using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[DefaultExecutionOrder (-100)]
public class ServiceLocator : MonoBehaviour, IServiceLocator
{
    // To set mono services in inspector
    [SerializeField] private List<MonoBehaviour> monoServicesList = new List<MonoBehaviour>();
    // To store C# services
    private List<Type> servicesList = new List<Type>()
    {
        // We need to set new instance, e g typeof(NetworkManager)
    };

    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    
    void Awake()
    {
        foreach (var mono in monoServicesList)
        {
            if (mono == null) continue;

            // Register by the concrete class type
            Register(mono.GetType(), mono);
            // Interfaces
            foreach (var interfaceType in mono.GetType().GetInterfaces())
            {
                Register(interfaceType, mono);
            }
        }
        
        foreach (var type in servicesList)
        {
            if (type == null) continue;
            object instance = Activator.CreateInstance(type);
        
            Register(type, instance);
        
            foreach (var interfaceType in type.GetInterfaces())
            {
                Register(interfaceType, instance);
            }
        }
    }

    // Register a service instance
    public void Register(Type type ,object service)
    {
        if (!_services.ContainsKey(type))
        {
            _services.Add(type, service);
        }
    }

    // Retrieve a service instance
    public T GetService<T>()
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
            return (T)service;

        Debug.LogError($"Service of type {type} not registered.");
        return default;
    }
}

