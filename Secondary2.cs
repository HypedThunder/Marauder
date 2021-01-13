using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.HAND.Weapon;
using EntityStates.ClayBoss;

namespace EntityStates.Marauder.Weapon22
{
	// Token: 0x02000A2C RID: 2604
	public class BoneCrusher2 : BaseState
	{
		// Token: 0x06003BDC RID: 15324 RVA: 0x000F73E4 File Offset: 0x000F55E4
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = BoneCrusher2.baseDuration / this.attackSpeedStat;
			this.modelAnimator = base.GetModelAnimator();
			Transform modelTransform = base.GetModelTransform();
			this.attack = new OverlapAttack();
			this.attack.attacker = base.gameObject;
			this.attack.inflictor = base.gameObject;
			this.attack.teamIndex = TeamComponent.GetObjectTeam(this.attack.attacker);
			this.attack.damage = 7f * this.damageStat;
			this.attack.hitEffectPrefab = FireTarball.effectPrefab;
			this.attack.isCrit = Util.CheckRoll(this.critStat, base.characterBody.master);
			if (modelTransform)
			{
				this.attack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Hammer");
				ChildLocator component = modelTransform.GetComponent<ChildLocator>();
				if (component)
				{
					this.hammerChildTransform = component.FindChild("SwingCenter");
				}
			}
			if (this.modelAnimator)
			{
				base.PlayAnimation("Gesture", "Slam", "Slam.playbackRate", this.duration);
			}
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(2f);
			}
		}

		// Token: 0x06003BDD RID: 15325 RVA: 0x000F7550 File Offset: 0x000F5750
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && this.modelAnimator && this.modelAnimator.GetFloat("Hammer.hitBoxActive") > 0.5f)
			{
				if (!this.hasSwung)
				{
					Ray aimRay = base.GetAimRay();
					EffectManager.SimpleMuzzleFlash(FireTarball.effectPrefab, base.gameObject, "SwingCenter", true);
					this.hasSwung = true;
				}
				this.attack.forceVector = this.hammerChildTransform.right;
				this.attack.Fire(null);
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x06003BDE RID: 15326 RVA: 0x0000D472 File Offset: 0x0000B672
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x04003611 RID: 13841
		public static float baseDuration = 3f;

		// Token: 0x04003612 RID: 13842
		public static float returnToIdlePercentage;

		// Token: 0x04003613 RID: 13843
		public static float impactDamageCoefficient = 7f;

		// Token: 0x04003614 RID: 13844
		public static float earthquakeDamageCoefficient = 2f;

		// Token: 0x04003615 RID: 13845
		public static float forceMagnitude = 16f;

		// Token: 0x04003616 RID: 13846
		public static float radius = 3f;

		// Token: 0x04003617 RID: 13847
		public static GameObject hitEffectPrefab;

		// Token: 0x04003618 RID: 13848
		public static GameObject swingEffectPrefab;

		// Token: 0x04003619 RID: 13849
		public static GameObject projectilePrefab;

		// Token: 0x0400361A RID: 13850
		private Transform hammerChildTransform;

		// Token: 0x0400361B RID: 13851
		private OverlapAttack attack;

		// Token: 0x0400361C RID: 13852
		private Animator modelAnimator;

		// Token: 0x0400361D RID: 13853
		private float duration;

		// Token: 0x0400361E RID: 13854
		private bool hasSwung;
	}
}