using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LockIcon : MonoBehaviour
{
    [Tooltip("The speed at which the lock image rotates (Degrees / Second)")]
    [SerializeField]
    [Range(-720, 720)]
    private float m_lockRotateSpeed = 0.0f;

    private RectTransform m_transform;
    private Image m_image;

    void Start()
    {
        m_transform = GetComponent<RectTransform>();
        m_image = GetComponent<Image>();
    }

    void Update()
    {
        if (GameController.CameraRig.LockTarget != null)
        {
            Vector3 screenPos = Camera.main.WorldToViewportPoint(GameController.CameraRig.LockTarget.position);
            m_transform.anchorMin = screenPos;
            m_transform.anchorMax = screenPos;
            m_image.enabled = (screenPos.z > 0);
        }
        else
        {
            m_image.enabled = false;
        }
        m_transform.Rotate(Vector3.forward, m_lockRotateSpeed * Time.deltaTime);
    }
}
