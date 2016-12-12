using UnityEngine;
using System.Collections;
using System;

namespace World
{
	public class Hero : MonoBehaviour
	{
		public float MoveSpeed = 5.0f;

		[HideInInspector]
		public MapPoint Location;

		[HideInInspector]
		public MapPoint PrevLocation;

		Puppet2D_GlobalControl Puppet;
		Animator PuppetAnimator;

		void Awake()
		{
			Puppet = GetComponentInChildren<Puppet2D_GlobalControl>();
			PuppetAnimator = Puppet.GetComponent<Animator>();

			ReArrengePuppetLayer();
		}

		public IEnumerator Move(MapPoint destination)
		{
			FaceTarget(destination.transform);
			PuppetAnimator.SetTrigger("Walking");
			while (!transform.position.Equals(destination.transform.position))
			{
				transform.position = Vector3.MoveTowards(transform.position, destination.transform.position, MoveSpeed * Time.deltaTime);
				yield return null;
			}
			PuppetAnimator.SetTrigger("Iddle");
			FaceRight();

			PrevLocation = Location;
			Location = destination;

			destination.OnArrival();
		}

		void FaceTarget(Transform target)
		{
			if (transform.position.x < target.transform.position.x) FaceRight();
			else FaceLeft();
		}

		void FaceRight()
		{
			Puppet.flip = false;
		}

		void FaceLeft()
		{
			Puppet.flip = true;
		}

		void ReArrengePuppetLayer()
		{
			foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
			{
				if (renderer.sortingLayerName.Equals("character")) renderer.sortingLayerName = "Hero";
			}

			foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (renderer.sortingLayerName.Equals("character")) renderer.sortingLayerName = "Hero";
			}
		}
	}
}
