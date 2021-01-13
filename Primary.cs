using System;
using EntityStates.ImpBossMonster;
using EntityStates.Merc;
using Marauder;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.HAND.Weapon;

namespace EntityStates.Marauder.Weapon1
{
	// Token: 0x02000002 RID: 2
	public class CleavingStrikes : BaseState
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override void OnEnter()
		{
			base.OnEnter();
			this.stopwatch = 0f;
			this.earlyExitDuration = GroundLight.baseEarlyExitDuration / this.attackSpeedStat;
			this.animator = base.GetModelAnimator();
			this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
			this.hasSwung = false;
			this.hasHopped = false;
			string hitboxGroupName = "Hammer";
			bool active = NetworkServer.active;
			if (active)
			{
				base.characterBody.AddBuff(MarauderMod.comboBuff);
			}
			HealthComponent healthComponent = base.characterBody.healthComponent;
			CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
			bool flag = healthComponent;
			if (flag)
			{
				healthComponent.TakeDamageForce(CleavingStrikes.backwardVelocity * component.forward, true, false);
			}
			switch (this.comboState)
			{
				case CleavingStrikes.ComboState.Combo1:
					this.attackDuration = CleavingStrikes.comboAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(CleavingStrikes.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), hitboxGroupName);
					this.damageCoefficient = CleavingStrikes.comboDamageCoefficient;
					base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", 0.6f);
					break;
				case CleavingStrikes.ComboState.Combo2:
					this.attackDuration = CleavingStrikes.comboAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(CleavingStrikes.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), hitboxGroupName);
					this.damageCoefficient = CleavingStrikes.comboDamageCoefficient;
					base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", 0.5f);
					break;
				case CleavingStrikes.ComboState.Combo3:
					this.attackDuration = CleavingStrikes.comboAttackDuration / this.attackSpeedStat;
					this.overlapAttack = base.InitMeleeOverlap(CleavingStrikes.finisherDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), hitboxGroupName);
					this.damageCoefficient = CleavingStrikes.finisherDamageCoefficient;
					base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", 1.3f);
					break;
			}
			this.swingEffectPrefab = GroundLight.comboSwingEffectPrefab;
			this.hitEffectPrefab = GroundLight.finisherHitEffectPrefab;
			this.attackSoundString = GroundLight.finisherAttackSoundString;
			base.characterBody.SetAimTimer(this.attackDuration + 1f);
			this.overlapAttack.hitEffectPrefab = GroundLight.finisherHitEffectPrefab;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000222C File Offset: 0x0000042C
		public override void OnExit()
		{
			base.OnExit();
			bool active = NetworkServer.active;
			if (active)
			{
				base.characterBody.RemoveBuff(MarauderMod.comboBuff);
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000225C File Offset: 0x0000045C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.hitPauseTimer -= Time.fixedDeltaTime;
			base.characterBody.isSprinting = false;
			bool isAuthority = base.isAuthority;
			if (isAuthority)
			{
				bool flag = base.FireMeleeOverlap(this.overlapAttack, this.animator, "Hammer.hitBoxActive", GroundLight.forceMagnitude, true);
				this.hasHit = (this.hasHit || flag);
				bool flag2 = flag;
				if (flag2)
				{
					bool flag3 = !this.hasHopped;
					if (flag3)
					{
						bool flag4 = base.characterMotor && !base.characterMotor.isGrounded;
						if (flag4)
						{
							base.SmallHop(base.characterMotor, CleavingStrikes.hitHopVelocity);
						}
						this.hasHopped = true;
					}
					bool flag5 = !this.isInHitPause;
					if (flag5)
					{
						this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Combo.playbackRate");
						this.hitPauseTimer = GroundLight.hitPauseDuration / this.attackSpeedStat;
						this.isInHitPause = true;
					}
				}
				bool flag6 = this.hitPauseTimer <= 0f && this.isInHitPause;
				if (flag6)
				{
					base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
					this.isInHitPause = false;
				}
			}
			bool flag7 = this.stopwatch >= this.attackDuration * 0.35f && !this.hasSwung;
			if (flag7)
			{
				Util.PlaySound(this.attackSoundString, base.gameObject);
				HealthComponent healthComponent = base.characterBody.healthComponent;
				CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
				bool flag8 = healthComponent;
				if (flag8)
				{
					healthComponent.TakeDamageForce(CleavingStrikes.forwardVelocity * component.forward, true, false);
				}
				this.hasSwung = true;
				EffectManager.SimpleMuzzleFlash(GroundLight.finisherSwingEffectPrefab, base.gameObject, "SwingCenter", true);
				bool isAuthority2 = base.isAuthority;
				if (isAuthority2)
				{
					Vector3 position = this.childLocator.FindChild("SwingCenter").position;
					BlastAttack blastAttack = new BlastAttack();
					blastAttack.radius = 8f;
					blastAttack.procCoefficient = 1f;
					blastAttack.position = position;
					blastAttack.attacker = base.gameObject;
					blastAttack.crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
					blastAttack.baseDamage = base.characterBody.damage * this.damageCoefficient;
					blastAttack.falloffModel = BlastAttack.FalloffModel.None;
					blastAttack.baseForce = 3f;
					blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
					blastAttack.damageType = DamageType.ClayGoo;
					bool flag9 = this.comboState == CleavingStrikes.ComboState.Combo3;
					if (flag9)
					{
						blastAttack.damageType = DamageType.ClayGoo;
					}
					blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
					blastAttack.impactEffect = GroundPound.hitEffectPrefab.GetComponent<EffectComponent>().effectIndex;
					blastAttack.Fire();
				}
				this.animator.SetFloat("Hammer.hitBoxActive", 1f);
			}
			else
			{
				this.animator.SetFloat("Hammer.hitBoxActive", 0f);
			}
			bool flag10 = !this.isInHitPause;
			if (flag10)
			{
				this.stopwatch += Time.fixedDeltaTime;
			}
			else
			{
				bool flag11 = base.characterMotor;
				if (flag11)
				{
					base.characterMotor.velocity = Vector3.zero;
				}
			}
			bool flag12 = base.isAuthority && this.stopwatch >= this.attackDuration - this.earlyExitDuration;
			if (flag12)
			{
				bool flag13 = !this.hasSwung;
				if (flag13)
				{
					bool flag14 = this.overlapAttack != null;
					if (flag14)
					{
						this.overlapAttack.Fire(null);
					}
				}
				bool flag15 = base.inputBank.skill1.down && this.comboState != CleavingStrikes.ComboState.Combo3;
				if (flag15)
				{
					CleavingStrikes greatswordCombo = new CleavingStrikes();
					greatswordCombo.comboState = this.comboState + 1;
					this.outer.SetNextState(greatswordCombo);
				}
				else
				{
					bool flag16 = this.stopwatch >= this.attackDuration;
					if (flag16)
					{
						this.outer.SetNextStateToMain();
					}
				}
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002694 File Offset: 0x00000894
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000026A7 File Offset: 0x000008A7
		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
			writer.Write((byte)this.comboState);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000026C0 File Offset: 0x000008C0
		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
			this.comboState = (CleavingStrikes.ComboState)reader.ReadByte();
		}

		// Token: 0x04000001 RID: 1
		public static float comboDamageCoefficient = 2.4f;

		// Token: 0x04000002 RID: 2
		public static float finisherDamageCoefficient = 4f;

		// Token: 0x04000003 RID: 3
		public static float hitHopVelocity = 1f;

		// Token: 0x04000004 RID: 4
		public static float forwardVelocity = 5f;

		// Token: 0x04000005 RID: 5
		public static float backwardVelocity = 0.25f;

		// Token: 0x04000006 RID: 6
		public static float comboAttackDuration = 1.4f;

		// Token: 0x04000007 RID: 7
		public static float finisherAttackDuration = 2.25f;

		// Token: 0x04000008 RID: 8
		private float damageCoefficient;

		// Token: 0x04000009 RID: 9
		private float stopwatch;

		// Token: 0x0400000A RID: 10
		private float attackDuration;

		// Token: 0x0400000B RID: 11
		private float earlyExitDuration;

		// Token: 0x0400000C RID: 12
		private Animator animator;

		// Token: 0x0400000D RID: 13
		private OverlapAttack overlapAttack;

		// Token: 0x0400000E RID: 14
		private float hitPauseTimer;

		// Token: 0x0400000F RID: 15
		private bool isInHitPause;

		// Token: 0x04000010 RID: 16
		private bool hasSwung;

		// Token: 0x04000011 RID: 17
		private bool hasHit;

		// Token: 0x04000012 RID: 18
		private bool hasHopped;

		// Token: 0x04000013 RID: 19
		public CleavingStrikes.ComboState comboState;

		// Token: 0x04000014 RID: 20
		private BaseState.HitStopCachedState hitStopCachedState;

		// Token: 0x04000015 RID: 21
		private GameObject swingEffectPrefab;

		// Token: 0x04000016 RID: 22
		private GameObject hitEffectPrefab;

		// Token: 0x04000017 RID: 23
		private ChildLocator childLocator;

		// Token: 0x04000018 RID: 24
		private string attackSoundString;

		// Token: 0x02000008 RID: 8
		public enum ComboState
		{
			// Token: 0x0400002B RID: 43
			Combo1,
			// Token: 0x0400002C RID: 44
			Combo2,
			// Token: 0x0400002D RID: 45
			Combo3
		}
	}
}