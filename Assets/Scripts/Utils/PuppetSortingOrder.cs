using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Combat;

namespace Utils
{
	public class PuppetSortingOrder : MonoBehaviour
	{
		public string MainLayerName;
		public string DeathLayerName;
		public int VerticalOrderMultiplier = -10000;

		SpriteRenderer[] SRenderers;
		SkinnedMeshRenderer[] MRenderers;
		int[] SOrders;
		int[] MOrders;

		Unit Parent;

		void Start()
		{
			Parent = GetComponentInParent<Unit>();

			List<SpriteRenderer> SpriteRenderers = new List<SpriteRenderer>();
			foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
			{
				if (renderer.sortingLayerName.Equals(MainLayerName)) SpriteRenderers.Add(renderer);
			}
			SRenderers = new SpriteRenderer[SpriteRenderers.Count];
			SOrders = new int[SpriteRenderers.Count];
			for (int i = 0; i < SRenderers.Length; ++i)
			{
				SRenderers[i] = SpriteRenderers[i];
				SOrders[i] = SpriteRenderers[i].sortingOrder;
			}

			List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (renderer.sortingLayerName.Equals(MainLayerName)) SkinnedMeshRenderers.Add(renderer);
			}
			MRenderers = new SkinnedMeshRenderer[SkinnedMeshRenderers.Count];
			MOrders = new int[SkinnedMeshRenderers.Count];
			for (int i = 0; i < MRenderers.Length; ++i)
			{
				MRenderers[i] = SkinnedMeshRenderers[i];
				MOrders[i] = SkinnedMeshRenderers[i].sortingOrder;
			}
		}

		void Update()
		{
			if (Parent.Alive)
			{
				int baseOrder = Mathf.FloorToInt(transform.position.y * VerticalOrderMultiplier);

				for (int i = 0; i < SRenderers.Length; ++i)
				{
					SRenderers[i].sortingOrder = baseOrder + SOrders[i];
				}

				for (int i = 0; i < MRenderers.Length; ++i)
				{
					MRenderers[i].sortingOrder = baseOrder + MOrders[i];
				}
			}
			else
			{
				for (int i = 0; i < SRenderers.Length; ++i)
				{
					SRenderers[i].sortingLayerName = DeathLayerName;
				}

				for (int i = 0; i < MRenderers.Length; ++i)
				{
					MRenderers[i].sortingLayerName = DeathLayerName;
				}

				// no need for sorting after this
				enabled = false;
			}

		}
	}
}