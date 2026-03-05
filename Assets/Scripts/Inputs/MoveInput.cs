using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveInput : MonoBehaviour,IPointerDownHandler,IDragHandler,IInputMove
{
   [SerializeField] private Camera cam;
   private Vector3 pointerPosition;
    
    public Vector3 GetPosition() => pointerPosition;

    public virtual void OnPointerDown(PointerEventData ped)
    {
       Vector3 pos = ped.position;
       pos.z = cam.WorldToScreenPoint(transform.position).z;
       pointerPosition = cam.ScreenToWorldPoint(pos);
    }
     public virtual void OnDrag(PointerEventData ped)
    {
        Vector3 pos = ped.position;
        pos.z = cam.WorldToScreenPoint(transform.position).z;
        pointerPosition = cam.ScreenToWorldPoint(pos);
    } 
}