using Unity.Entities;
using UnityEngine;

public interface IInputMove
{
    public Vector3 GetPosition();
}
// Interface fith randomization contract
public interface IRandomizable : IComponentData
{
    public virtual void Randomize(uint seed){}
}