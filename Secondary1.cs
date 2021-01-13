using System;
using UnityEngine;
using EntityStates.HAND.Weapon;
using EntityStates.Marauder.Weapon22;

namespace EntityStates.Marauder.Weapon2
{
	// Token: 0x02000A28 RID: 2600
	public class BoneCrusher1 : BaseState
	{
		// Token: 0x06003BC8 RID: 15304 RVA: 0x000F6E10 File Offset: 0x000F5010
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = ChargeSlam.baseDuration / this.attackSpeedStat;
			this.modelAnimator = base.GetModelAnimator();
			if (this.modelAnimator)
			{
				base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", this.duration);
			}
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(4f);
			}
		}

		// Token: 0x06003BC9 RID: 15305 RVA: 0x000F6E86 File Offset: 0x000F5086
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.characterMotor.isGrounded && base.isAuthority)
			{
				this.outer.SetNextState(new BoneCrusher2());
				return;
			}
		}

		// Token: 0x06003BCA RID: 15306 RVA: 0x0000D472 File Offset: 0x0000B672
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		// Token: 0x040035FA RID: 13818
		public static float baseDuration = 4.5f;

		// Token: 0x040035FB RID: 13819
		private float duration;

		// Token: 0x040035FC RID: 13820
		private Animator modelAnimator;
	}
}