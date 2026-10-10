using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.8f, -5.5f);
    public float followSpeed = 8f;
    public float lookSpeed = 9f;
    public float yaw;
    public float pitch;

    private void Update()
    {
        if (target == null)
            return;

        yaw += Input.GetAxis("Mouse X") * 2.3f;
        pitch -= Input.GetAxis("Mouse Y") * 2.1f;
        pitch = Mathf.Clamp(pitch, -22f, 32f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Vector3 focusPoint = target.position + Vector3.up * 1.6f;
        Quaternion lookRotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lookSpeed * Time.deltaTime);
    }
}
