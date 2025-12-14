using UnityEngine;

public class WheelRollByDistance : MonoBehaviour
{
    public Transform carRoot;
    public Transform[] wheels;
    public float wheelRadius = 0.35f;

    private Vector3 lastPos;

    void Start()
    {
        lastPos = carRoot.position;
    }

    void LateUpdate()
    {
        float distance = Vector3.Distance(carRoot.position, lastPos);
        float angle = (distance / (2 * Mathf.PI * wheelRadius)) * 360f;

        foreach (var wheel in wheels)
        {
            wheel.Rotate(Vector3.right, angle, Space.Self);
        }

        lastPos = carRoot.position;
    }
}
