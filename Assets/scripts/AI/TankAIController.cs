using UnityEngine;
using System.Collections;

namespace MyLib
{
    /// <summary>
    /// 自己玩家tank 状态控制
    /// </summary>
    [RequireComponent(typeof (AnimationController))]
    [RequireComponent(typeof (SkillCombineBuff))]
    //[RequireComponent(typeof (MySelfAttributeSync))]
    //[RequireComponent(typeof (PlayerSyncToServer))]
    [RequireComponent(typeof (TankPhysicComponent))]
    [RequireComponent(typeof(DropItemCheck))]
    public class TankAIController : AIBase
    {
        private AudioSource run;
        private void Awake()
        {
            var tower = Util.FindChildRecursive(transform, "tower");
            tower.gameObject.AddComponent<TowerAutoCheck>();

            attribute = GetComponent<NpcAttribute>();

            ai = new TankCharacter();
            ai.attribute = attribute;
            ai.AddState(new TankIdle());
            ai.AddState(new TankMoveAndShoot());
            ai.AddState(new TankDead());
            ai.AddState(new TankKnockBack());
            ai.AddState(new HumanStunned());
            ai.AddState(new TankStop());

            run = BackgroundSound.Instance.PlayEffectLoop("run", 0.1f);
            run.Play();
            StartCoroutine(CheckSpeed());
            StartCoroutine(CheckHP());
        }

      

        private GameObject BloodScreen = null;
        IEnumerator CheckHP()
        {
            yield return new WaitForSeconds(5);
            if (BloodScreen == null)
            {
                //BloodScreen = WindowMng.windowMng.PushView("UI/BloodScreen");
                var uiRoot = WindowMng.windowMng.GetMainUI();
                BloodScreen = WindowMng.windowMng.AddChildLow(uiRoot, Resources.Load<GameObject>("UI/BloodScreen"));
            }
            bool show = false;
            var nw = new WaitForSeconds(1);
            while (true)
            {
                if (attribute.HP < attribute.HP_Max*0.25f && !show)
                {
                    show = true;
                    TweenAlpha.Begin(BloodScreen, 1, 1);
                }
                else if(attribute.HP > attribute.HP_Max * 0.3f && show)
                {
                    show = false;
                    TweenAlpha.Begin(BloodScreen, 1, 0);

                }
                yield return nw;
            }

        }

        IEnumerator CheckSpeed()
        {
            while (true)
            {
                if (run != null)
                {
                    run.volume = attribute.MoveSpeed*0.05f;
                }
                yield return null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameObject.Destroy(run);
        }

        private IEnumerator CheckFallDead()
        {
            while (true)
            {
                var pos = transform.position;
                if (pos.y <= -10 && ai.state.type != AIStateEnum.DEAD)
                {
                    attribute.ChangeHP(-attribute.HP_Max);
                }
                yield return new WaitForSeconds(1);
            }
        }


        private void Start()
        {
            ai.ChangeState(AIStateEnum.IDLE);
            StartCoroutine(CheckFallDead());
        }
    }
}