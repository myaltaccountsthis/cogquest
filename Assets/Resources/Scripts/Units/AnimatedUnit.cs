using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedUnit : Unit
{
	public Sprite[] animSprites;
	public int[] animFrames;
	public float animFramesPerSecond;
	public bool loopAnim;

	[SerializeField]
	private SpriteRenderer animSpriteRenderer;
	private float animTimeBetweenFrames;
	private int animFrameIndex;

	[SerializeField]
	private Animator treadAnimator;

	protected override void Awake()
	{
		base.Awake();

		animTimeBetweenFrames = 1 / animFramesPerSecond;
		animFrameIndex = 0;
	}

	protected override void Update()
	{
		base.Update();

		if (treadAnimator != null)
		{
			if (IsMoving)
			{
				treadAnimator.enabled = true;
				treadAnimator.speed = speed * 2.66667f;
			}
			else
			{
				treadAnimator.enabled = false;
			}
		}
	}

	public override void DoAttack()
	{
		animFrameIndex = 0;
		CancelInvoke(nameof(StepAnim));
		StepAnim();
	}

	private void StepAnim()
	{
		if (!isAttacking && loopAnim)
		{
			CancelInvoke(nameof(StepAnim));
			return;
		}
		animSpriteRenderer.sprite = animSprites[animFrames[animFrameIndex]];
		animFrameIndex++;
		if (animFrameIndex >= animFrames.Length)
		{
			if (loopAnim)
			{
				animFrameIndex = 0;
				Invoke(nameof(StepAnim), animTimeBetweenFrames);
			}
		}
		else
			Invoke(nameof(StepAnim), animTimeBetweenFrames);
	}
}
