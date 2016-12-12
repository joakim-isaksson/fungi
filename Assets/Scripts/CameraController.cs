using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public float DesktopMinX;
	public float DesktopMaxX;
    public float IOSMinX;
    public float IOSMaxX;
    public float AndroidMinX;
    public float AndroidMaxX;
    public float Speed;

    private float MinX;
    private float MaxX;

    Transform Follow;

    void Awake()
    {
        MinX = DesktopMinX;
        MaxX = DesktopMaxX;

#if UNITY_IOS
        MinX = IOSMinX;
        MaxX = IOSMaxX;
#endif

#if UNITY_ANDROID
        MinX = AndroidMinX;
        MaxX = AndroidMaxX;
#endif
    }

    void Start()
	{
		Follow = GameObject.FindGameObjectWithTag("Player").transform;
	}

	void Update()
	{
		transform.position = new Vector3(
			Mathf.Clamp(Follow.position.x, MinX, MaxX),
			transform.position.y,
			transform.position.z
		);
	}
}
