using UnityEngine;
using UnityEngine.EventSystems;

public class TapDetectionManager : MonoBehaviour
{
    void Update()
    {
        // PHONE TOUCH
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Interactable"))
                {
                    hit.transform.gameObject.SendMessage("OnTapped",
                        SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // UNITY EDITOR MOUSE
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Interactable"))
                {
                    hit.transform.gameObject.SendMessage("OnTapped",
                        SendMessageOptions.DontRequireReceiver);
                }
            }
        }
#endif
    }
}