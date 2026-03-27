using System;
using Unity.Entities;
using UnityEngine;

public interface IInputService
{
    public Vector3 GetPosition();
    event Action<bool> OnButtonPressed;
}
// Interface fith randomization contract
public interface IRandomizable : IComponentData
{
    public virtual void Randomize(uint seed){}
}
public interface IServiceLocator
{
    public T GetService<T>();
}