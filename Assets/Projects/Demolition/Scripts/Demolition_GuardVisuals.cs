using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_GuardVisuals : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer batteryRenderer;

        private MaterialPropertyBlock propertyBlock;

        public void Initialize(Animator anim,Renderer battery,int maxHits)
        {
            animator=anim;
            batteryRenderer=battery;

            if(animator!=null)
            {
                animator.applyRootMotion=false;
                animator.speed=1f;
            }

            if(propertyBlock==null)propertyBlock=new MaterialPropertyBlock();
            UpdateBattery(maxHits,maxHits);
        }

        public void SetGuarding(bool value)
        {
            if(animator==null)return;

            animator.SetBool("IsGuarding",value);

            if(!value)
            {
                animator.SetFloat("GuardX",0f);
                animator.SetFloat("GuardY",0f);
            }
        }

        public void EnterGuardAnimation()
        {
            if(animator==null)return;

            animator.speed=1f;
            animator.SetBool("IsGuarding",true);
            animator.SetFloat("GuardX",0f);
            animator.SetFloat("GuardY",0f);

            int hash=Animator.StringToHash("Guard");

            if(animator.HasState(0,hash))
                animator.CrossFadeInFixedTime(hash,0.1f,0);
        }

        public void SetGuardMovement(Vector3 worldDirection)
        {
            if(animator==null)return;

            worldDirection.y=0f;

            if(worldDirection.sqrMagnitude<0.001f)
            {
                animator.SetFloat("GuardX",0f);
                animator.SetFloat("GuardY",0f);
                return;
            }

            Vector3 local=transform.InverseTransformDirection(worldDirection.normalized);

            animator.SetFloat("GuardX",Mathf.Clamp(local.x,-1f,1f));
            animator.SetFloat("GuardY",Mathf.Clamp(local.z,-1f,1f));
        }

        public void SetWalking(bool isWalking,float animSpeed=1f)
        {
            if(animator==null)return;

            animator.SetBool("IsWalking",isWalking);
            animator.speed=isWalking?animSpeed:1f;
        }

        public void PlayHit()
        {
            if(animator==null)return;

            animator.speed=1f;
            animator.SetBool("StartDialogue",false);
            animator.SetTrigger("Hit");

            int hash=Animator.StringToHash("Hit");

            if(animator.HasState(0,hash))
                animator.CrossFadeInFixedTime(hash,0.05f,0,0f);
        }

        public bool IsPlayingHit()
        {
            if(animator==null)return false;

            AnimatorStateInfo current=animator.GetCurrentAnimatorStateInfo(0);

            if(current.IsName("Hit"))
                return true;

            if(animator.IsInTransition(0))
            {
                AnimatorStateInfo next=animator.GetNextAnimatorStateInfo(0);

                if(next.IsName("Hit"))
                    return true;
            }

            return false;
        }

        public void PlayDialogue(float dialogueIndex=-1f)
        {
            if(animator==null)return;

            if(dialogueIndex<0f)
                dialogueIndex=Random.Range(0f,1f);

            animator.speed=1f;
            animator.SetFloat("DialogueIndex",dialogueIndex);
            animator.SetBool("StartDialogue",true);

            StartCoroutine(ResetDialogueFlag());
        }

        private IEnumerator ResetDialogueFlag()
        {
            yield return null;

            if(animator!=null)
                animator.SetBool("StartDialogue",false);
        }

        public bool IsPlayingDialogue()
        {
            if(animator==null)return false;

            return animator.GetCurrentAnimatorStateInfo(0).IsName("Dialogue");
        }

        public void CrossFadeIdle(float transitionDuration=0.2f)
        {
            if(animator==null)return;

            animator.speed=1f;
            animator.SetBool("IsGuarding",false);
            animator.SetBool("IsWalking",false);
            animator.SetFloat("GuardX",0f);
            animator.SetFloat("GuardY",0f);

            int hash=Animator.StringToHash("Idle");

            if(animator.HasState(0,hash))
                animator.CrossFadeInFixedTime(hash,transitionDuration,0);
        }

        public void UpdateBattery(int remainingHits,int maxHits)
        {
            if(batteryRenderer==null||maxHits<=0)return;

            if(propertyBlock==null)
                propertyBlock=new MaterialPropertyBlock();

            float fill=Mathf.Clamp01((float)remainingHits/maxHits);

            batteryRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetInteger("_Life",maxHits);
            propertyBlock.SetFloat("_FillAmount",fill);
            batteryRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}