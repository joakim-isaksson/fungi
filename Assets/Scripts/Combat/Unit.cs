using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Combat
{
	public class Unit : MonoBehaviour
	{
		[Header("Characteristics")]
		public UnitType Type;
		public bool CanShoot;
		public bool VIP;
		public UnitStats Stats;

		[Header("Special Ability")]
		public SpecialAbilityType SpecialAbility;
		public int SpecialAbilityStrength;
		public int SpecialAbilityCoolDown;

		[Header("Animation Speeds")]
		public float WalkingSpeed = 2.0f;
		public float RunningSpeed = 4.0f;
		public float DefendDelay = 2.0f;
		public float WaitDelay = 2.0f;
		public float HurtDelay = 2.0f;
		public float HealDelay = 2.0f;
		public float DeathDelay = 1.0f;
		public float ProjectileSpeed = 8.0f;
		public float ProjectileDelay = 0.5f;
		public float AttackDmgDelay = 1.0f;

		[Header("Animators")]
		public Puppet2D_GlobalControl Puppet;
		public Animator IconAnimator;
		public Animator CharAnimator;
		public Animator TextAnimator;
		public Text CombatText;

		[Header("Sounds")]
		public AudioClip HurtSfx;
		public float HurtSfxDelay = 0.15f;
		public AudioClip BlockSfx;
		public AudioClip AttackSfx;
		public float AttackSfxDelay;
		public AudioClip ShootSfx;
		public float ShootSfxDelay;
		public AudioClip SpecialAttackSfx;
		public AudioClip DeathSfx;
		public AudioClip WalkSfx;
		public AudioClip DefenceSfx;
		public AudioClip WaitSfx;

		[Header("Projectiles")]
		public GameObject ProjectilePrefab;
		public Transform ProjectileSpawnPoint;
		public Transform ProjectileHitPoint;

		[HideInInspector]
		public bool Alive;

		[HideInInspector]
		public int HitPoints;

		[HideInInspector]
		public int Size;

		[HideInInspector]
		public int TurnNumber;

		[HideInInspector]
		public bool Defending;

		[HideInInspector]
		public bool Walked;

		[HideInInspector]
		public bool Waiting;

		[HideInInspector]
		public int CoolDownLeft;

		[HideInInspector]
		public bool CounterAttackUsed;

		[HideInInspector]
		public Tile Tile;

		[HideInInspector]
		public int PlayerId;

		[HideInInspector]
		public bool AIControlled;

		[HideInInspector]
		public AudioSource Asrc;

		void Awake()
		{
			Asrc = GetComponent<AudioSource>();
		}

		public void Spawn(Tile tile, int size, int playerId, bool aiControlled)
		{
			Alive = true;
			Tile = tile;
			Size = size;
			HitPoints = Stats.Vitality;
			PlayerId = playerId;
			AIControlled = aiControlled;
			CoolDownLeft = SpecialAbilityCoolDown;

			transform.position = tile.transform.position;
			ResetFacing();
		}

		public IEnumerator Move(List<Tile> path, bool running, Action callback)
		{
			if (!running) Walked = true;

			bool useRunAnimation = (path.Count > 2 && running);
			float speed = running ? RunningSpeed : WalkingSpeed;

			if (useRunAnimation)
			{
				CharAnimator.SetTrigger("Running");
				Asrc.clip = WalkSfx;
				Asrc.loop = true;
				Asrc.pitch = 2.0f;
				Asrc.Play();
			}
			else
			{
				CharAnimator.SetTrigger("Walking");
				Asrc.clip = WalkSfx;
				Asrc.loop = true;
				Asrc.pitch = 1.0f;
				Asrc.Play();
			}

			foreach (Tile target in path)
			{
				Tile = target;

				FaceTarget(target.transform);

				while (!transform.position.Equals(target.transform.position))
				{
					transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
					yield return null;
				}
			}

			CharAnimator.SetTrigger("Iddle");
			Asrc.Stop();

			ResetFacing();

			callback();
		}

		public IEnumerator Defence(Action callback)
		{
			Defending = true;
			IconAnimator.SetTrigger("Defence");
			Asrc.PlayOneShot(DefenceSfx);
			yield return new WaitForSeconds(DefendDelay);
			callback();
		}

		public IEnumerator Wait(Action callback)
		{
			IconAnimator.SetTrigger("Wait");
			Asrc.PlayOneShot(WaitSfx);
			yield return new WaitForSeconds(WaitDelay);
			callback();
		}

		public IEnumerator Attack(Unit target, Action callback, bool counterAttack)
		{
			int attack = GenAttackDamage();
			float defenceBonus = target.Defending ? 0.3f : 0.0f;
			int dmg = Mathf.CeilToInt(attack * (1.0f - target.Stats.Defence - defenceBonus));

			FaceTarget(target.transform);

			CharAnimator.SetTrigger("Attack");
			Asrc.clip = AttackSfx;
			Asrc.loop = false;
			Asrc.PlayDelayed(AttackSfxDelay);

			yield return new WaitForSeconds(AttackDmgDelay);

			target.TakeDamage(dmg);
			target.CharAnimator.SetTrigger("Hurt");
			target.CombatText.text = "" + dmg;
			target.TextAnimator.SetTrigger("Damage");

			target.Asrc.clip = target.Defending ? target.BlockSfx : target.HurtSfx;
			target.Asrc.loop = false;
			target.Asrc.PlayDelayed(HurtSfxDelay);

			if (!target.Alive)
			{
				yield return new WaitForSeconds(HurtDelay / 2);
				target.CharAnimator.SetTrigger("Death");
				target.Asrc.PlayOneShot(DeathSfx);
				yield return new WaitForSeconds(DeathDelay);
			}
			else
			{
				yield return new WaitForSeconds(HurtDelay);
			}

			if (target.Alive && !target.CounterAttackUsed && !counterAttack)
			{
				target.CounterAttackUsed = true;
				StartCoroutine(target.Attack(this, delegate
				{
					ResetFacing();
					callback();
				}, true));
			}
			else
			{
				ResetFacing();
				callback();
			}
		}

		public IEnumerator Shoot(Unit target, Action callback)
		{
			int attack = GenShootDamage(target);
			float defenceBonus = target.Defending ? 0.3f : 0.0f;
			int dmg = Mathf.CeilToInt(attack * (1.0f - target.Stats.Defence - defenceBonus));

			FaceTarget(target.transform);

			CharAnimator.SetTrigger("Shoot");
			Asrc.clip = ShootSfx;
			Asrc.loop = false;
			Asrc.PlayDelayed(ShootSfxDelay);

			yield return new WaitForSeconds(ProjectileDelay);

			GameObject projectile = Instantiate(ProjectilePrefab);
			projectile.transform.position = ProjectileSpawnPoint.position;
			Vector3 targetPoint = target.ProjectileHitPoint.transform.position;
			while (!projectile.transform.position.Equals(targetPoint))
			{
				projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPoint, ProjectileSpeed * Time.deltaTime);
				yield return null;
			}
			Destroy(projectile);

			target.TakeDamage(dmg);
			target.CharAnimator.SetTrigger("Hurt");
			target.CombatText.text = "" + dmg;
			target.TextAnimator.SetTrigger("Damage");

			AudioClip hurtSound = target.Defending ? target.BlockSfx : target.HurtSfx;
			target.Asrc.PlayOneShot(hurtSound);

			if (!target.Alive)
			{
				yield return new WaitForSeconds(HurtDelay / 2);
				target.CharAnimator.SetTrigger("Death");
				target.Asrc.PlayOneShot(DeathSfx);
				yield return new WaitForSeconds(DeathDelay);
			}
			else
			{
				yield return new WaitForSeconds(HurtDelay);
			}

			ResetFacing();
			callback();
		}

		public IEnumerator DrainLife(Unit target, Action callback)
		{
			int dmg = Mathf.CeilToInt(SpecialAbilityStrength * (1.0f - target.Stats.MagicDefence));

			FaceTarget(target.transform);

			CharAnimator.SetTrigger("DrainLife");

			yield return new WaitForSeconds(ProjectileDelay);

			GameObject projectile = Instantiate(ProjectilePrefab);
			projectile.transform.position = ProjectileSpawnPoint.position;
			Vector3 targetPoint = target.ProjectileHitPoint.transform.position;
			while (!projectile.transform.position.Equals(targetPoint))
			{
				projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPoint, ProjectileSpeed * Time.deltaTime);
				yield return null;
			}
			Destroy(projectile);

			target.TakeDamage(dmg);
			target.CharAnimator.SetTrigger("Hurt");
			target.IconAnimator.SetTrigger("Cursed");
			target.CombatText.text = "" + dmg;
			target.TextAnimator.SetTrigger("Damage");
			target.Asrc.PlayOneShot(SpecialAttackSfx);
			target.Asrc.PlayOneShot(target.HurtSfx);

			GainLife(dmg);
			IconAnimator.SetTrigger("Healed");
			CombatText.text = "" + dmg;
			TextAnimator.SetTrigger("Heal");

			if (!target.Alive)
			{
				yield return new WaitForSeconds(HurtDelay / 2);
				target.CharAnimator.SetTrigger("Death");
				target.Asrc.PlayOneShot(DeathSfx);
				yield return new WaitForSeconds(DeathDelay);
			}
			else
			{
				yield return new WaitForSeconds(HurtDelay);
			}

			ResetFacing();
			callback();
		}

		public IEnumerator Heal(Unit target, Action callback)
		{
			FaceTarget(target.transform);

			CharAnimator.SetTrigger("Heal");

			yield return new WaitForSeconds(ProjectileDelay);

			GameObject projectile = Instantiate(ProjectilePrefab);
			projectile.transform.position = ProjectileSpawnPoint.position;
			Vector3 targetPoint = target.ProjectileHitPoint.transform.position;
			while (!projectile.transform.position.Equals(targetPoint))
			{
				projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPoint, ProjectileSpeed * Time.deltaTime);
				yield return null;
			}
			Destroy(projectile);

			int headled = target.GainLife(SpecialAbilityStrength);
			target.CharAnimator.SetTrigger("Healed");
			target.IconAnimator.SetTrigger("Healed");
			target.CombatText.text = "" + headled;
			target.TextAnimator.SetTrigger("Heal");
			target.Asrc.PlayOneShot(SpecialAttackSfx);

			yield return new WaitForSeconds(HealDelay);

			ResetFacing();
			callback();
		}

		void FaceTarget(Transform target)
		{
			if (transform.position.x < target.transform.position.x) FaceRight();
			else FaceLeft();
		}

		void ResetFacing()
		{
			if (PlayerId < 1) FaceRight();
			else FaceLeft();
		}

		void FaceRight()
		{
			if (Puppet != null) Puppet.flip = false;
		}

		void FaceLeft()
		{
			if (Puppet != null) Puppet.flip = true;
		}

		int GainLife(int amount)
		{
			int newHitPoints = Mathf.Min(HitPoints + amount, Stats.Vitality);
			int lifeGain = newHitPoints - HitPoints;
			HitPoints = newHitPoints;
			return lifeGain;
		}

		void TakeDamage(int damage)
		{
			while (damage > 0 && Size > 0)
			{
				if (HitPoints > damage)
				{
					HitPoints -= damage;
					damage = 0;
				}
				else
				{
					damage -= HitPoints;
					if (--Size > 0) HitPoints = Stats.Vitality;
				}
			}

			if (Size == 0)
			{
				HitPoints = 0;
				Alive = false;
			}
		}

		int GenAttackDamage()
		{
			int totalDmg = 0;
			for (int i = 0; i < Size; ++i)
			{
				totalDmg += UnityEngine.Random.Range(Stats.AttackMinDmg, Stats.AttackMaxDmg);
			}
			return totalDmg;
		}

		int GenShootDamage(Unit target)
		{
			int totalDmg = 0;
			for (int i = 0; i < Size; ++i)
			{
				totalDmg += UnityEngine.Random.Range(Stats.ShootMinDmg, Stats.ShootMaxDmg);
			}

			totalDmg -= (int)(Tile.Position.Distance(target.Tile.Position) * Stats.DistancePenalty);
			if (totalDmg < 0) totalDmg = 1;

			return totalDmg;
		}
	}
}
