using System;
using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster
{
	// Token: 0x02000B0B RID: 2827
	public class SprintBash : BasicMeleeAttack
	{
		// Token: 0x06003FEF RID: 16367 RVA: 0x0010B8C1 File Offset: 0x00109AC1
		protected override void PlayAnimation()
		{
			base.PlayAnimation("Gesture", "Slam", "Slam.playbackRate", this.duration);
		}

		// Token: 0x06003FF0 RID: 16368 RVA: 0x0010B8E4 File Offset: 0x00109AE4
		public override void OnEnter()
		{
			base.OnEnter();
			AimAnimator aimAnimator = base.GetAimAnimator();
			if (aimAnimator)
			{
				aimAnimator.enabled = true;
			}
			if (base.characterDirection)
			{
				base.characterDirection.forward = base.inputBank.aimDirection;
			}
		}

		// Token: 0x06003FF1 RID: 16369 RVA: 0x0010B930 File Offset: 0x00109B30
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority && base.inputBank && base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed)
			{
				base.skillLocator.utility.ExecuteIfReady();
				return;
			}
		}

		// Token: 0x06003FF2 RID: 16370 RVA: 0x0010B99C File Offset: 0x00109B9C
		public override void OnExit()
		{
			Transform transform = base.FindModelChild("SpinnyFX");
			if (transform)
			{
				transform.gameObject.SetActive(false);
			}
			base.OnExit();
		}

		// Token: 0x06003FF3 RID: 16371 RVA: 0x0010B9E4 File Offset: 0x00109BE4
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (base.fixedAge <= SprintBash.durationBeforePriorityReduces)
			{
				return InterruptPriority.PrioritySkill;
			}
			return InterruptPriority.Skill;
		}

		// Token: 0x04003B9E RID: 15262
		public static float durationBeforePriorityReduces;
	}
}