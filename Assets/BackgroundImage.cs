using UnityEngine;
using System.Collections;

public class BackgroundImage : MonoBehaviour {

	public Sprite DestopImage;
	public Sprite IOSImage;

	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer> ().sprite = DestopImage;
		#if UNITY_IOS
		GetComponent<SpriteRenderer>().sprite = IOSImage;
		#endif
		#if UNITY_IPHONE
		GetComponent<SpriteRenderer>().sprite = IOSImage;
		#endif
	}

}
