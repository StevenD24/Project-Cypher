using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

public class MechSoldierControl : MonoBehaviour
{
    #region Inspector
    // [SpineAnimation] attribute allows an Inspector dropdown of Spine animation names coming form SkeletonAnimation.
    [SpineAnimation]
    public string run_1;
    [SpineAnimation]
    public string run_2;

    [SpineAnimation]
    public string idle_1;
    [SpineAnimation]
    public string idle_2;
    [SpineAnimation]
    public string idle_3;

    [SpineAnimation]
    public string walk_1;
    [SpineAnimation]
    public string walk_2;

    [SpineAnimation]
    public string attack;

    [SpineAnimation]
    public string jump;

    [SpineAnimation]
    public string getHit;

    [SpineAnimation]
    public string death;

    [SpineAnimation]
    public string skill_1;
    [SpineAnimation]
    public string skill_2;

    #endregion

    SkeletonAnimation skeletonAnimation;

    // Spine.AnimationState and Spine.Skeleton are not Unity-serialized objects. You will not see them as fields in the inspector.
    public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;
    // Start is called before the first frame update
    void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
    }

    public void running_1()
    {
        spineAnimationState.SetAnimation(0, run_1, true);
    }
    public void running_2()
    {
        spineAnimationState.SetAnimation(0, run_2, true);
    }
    public void walking_1()
    {
        spineAnimationState.SetAnimation(0, walk_1, true);
    }
    public void walking_2()
    {
        spineAnimationState.SetAnimation(0, walk_2, true);
    }
    public void idleAnim_1()
    {
        spineAnimationState.SetAnimation(0, idle_1, true);
    }
    public void idleAnim_2()
    {
        spineAnimationState.SetAnimation(0, idle_2, true);
    }
    public void idleAnim_3()
    {
        spineAnimationState.SetAnimation(0, idle_3, true);
    }
    public void jumpAnim()
    {
        spineAnimationState.SetAnimation(0, jump, true);
    }
    public void getHitAnim()
    {
        spineAnimationState.SetAnimation(0, getHit, true);
    }
    public void deathAnim()
    {
        spineAnimationState.SetAnimation(0, death, true);
    }

    public void attackAnim()
    {
        spineAnimationState.SetAnimation(0, attack, true);
    }
    public void skillAnim_1()
    {
        spineAnimationState.SetAnimation(0, skill_1, true);
    }
    public void skillAnim_2()
    {
        spineAnimationState.SetAnimation(0, skill_2, true);
    }
}
