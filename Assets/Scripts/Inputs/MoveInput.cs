using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IInputService
{
    [SerializeField] private Camera cam;
    [SerializeField] private float xPosition;

    public Vector3 GetPosition() => new Vector3(xPosition, 0f, 0f);

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnButtonPressed?.Invoke(true);
        UpdatePointerPosition(ped.position);
    }
    public virtual void OnDrag(PointerEventData ped) => UpdatePointerPosition(ped.position);
    public virtual void OnPointerUp(PointerEventData ped) => OnButtonPressed?.Invoke(false);

    //Events
    public event Action<bool> OnButtonPressed;

    private void UpdatePointerPosition(Vector2 screenPos)
    {
        // Ray from the camera to the screen point
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Horizontal plane at the object's current height
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        //Intersection ray with the Plane
        if (groundPlane.Raycast(ray, out float enter))
            xPosition = ray.GetPoint(enter).x;
    }
}
