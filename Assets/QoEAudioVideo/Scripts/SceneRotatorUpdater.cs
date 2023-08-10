using OscJack;
using UnityEngine;

public class SceneRotatorUpdater : MonoBehaviour
{
    public string IPAddress = "127.0.0.1";
    public int Port = 9100;

    private OscClient _sceneRotatorConnection;
    private Transform _transform;

    // Start is called before the first frame update
    void Awake()
    {
        _sceneRotatorConnection = new OscClient(IPAddress, Port);
        UpdateRotation();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRotation();
    }

    void OnDestroy()
    {
        _sceneRotatorConnection?.Dispose();
        _sceneRotatorConnection = null;
    }

    private void UpdateRotation()
    {
        var ea_transformRotation = transform.rotation.eulerAngles;
        _sceneRotatorConnection.Send("/SceneRotator/ypr", ParseAngleToHalfRotation(ea_transformRotation.y), ParseAngleToQuaterRotation(ea_transformRotation.x), ParseAngleToHalfRotation(ea_transformRotation.z));
    }

    private float ParseAngleToHalfRotation(float angle)
        => ParseAngleToRange(angle, 180, 360);

    private float ParseAngleToQuaterRotation(float angle)
        => ParseAngleToRange(angle, 90, 360);

    private float ParseAngleToRange(float angle, float breakPoint, float range)
    {
        angle = angle % range;
        if (angle > breakPoint)
        {
            var modulus = angle % breakPoint;
            return -breakPoint + modulus;
        }

        if (angle < -breakPoint)
        {
            var modulus = Mathf.Abs(angle) % breakPoint;
            return breakPoint - modulus;
        }

        return angle;
    }
}
