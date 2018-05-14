using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib {
    [System.Serializable]
	public class IEffect {
		protected GameObject obj;
		public Affix affix;
		protected Affix.EffectType type;
        private bool _isDie = false;
        public int EffectId = 0;

        public bool IsDie
        {
            get { return _isDie; }
            set { _isDie = value; Log.Sys("SetisDie: "+_isDie+" type "+affix.effectType); }
        } 
		public GameObject attacker;
	    public GameObject target;
        public Vector3 attackerPos;

		protected float passTime = 0;
		GameObject unitTheme;
        protected NpcAttribute attri;
        public int attackerId;

        /// <summary>
        /// 初始化Buff
        /// </summary>
        /// <param name="af">Af.</param>
        /// <param name="o">O.</param>
		public virtual void Init(Affix af, GameObject o) {
			affix = af;
			obj = o;
            attri = obj.GetComponent<NpcAttribute>();
			if (affix.UnitTheme != null) {
				//var par = GameObject.Instantiate(affix.UnitTheme) as GameObject;
			    var par = ParticlePool.Instance.GetGameObject(affix.UnitTheme, ParticlePool.InitParticle);
				//par.transform.parent = obj.transform;
                var sync = par.AddMissingComponent<SyncPosWithTarget>();
                sync.target = obj;

				par.transform.localPosition = affix.ThemePos;
				par.transform.localRotation = Quaternion.identity;
				par.transform.localScale = Vector3.one;

				unitTheme = par;
            }
		}
        /// <summary>
        /// 激活Buff
        /// </summary>
		public virtual void OnActive() {
		}
        /// <summary>
        /// Buff状态更新 
        /// </summary>
		public virtual void OnUpdate() {
			passTime += Time.deltaTime;
			if (passTime >= affix.Duration) {
				IsDie = true;
			}
		}

		public virtual void OnDie(){
			if (unitTheme != null) {
				//GameObject.Destroy(unitTheme);
                ParticlePool.Instance.ReturnGameObject(unitTheme, ParticlePool.ResetParticle);
			    unitTheme = null;
			}
            //本地Buff的Id都是0
		    if (attri.IsMine() && this.EffectId != 0)
		    {
		        //NetDateInterface.FastRemoveBuff((int)this.type, attri.gameObject);
                NetDateInterface.RemoveBuffId(this.EffectId, attri.gameObject);
		    }
		}

        /// <summary>
        /// 速度修正系数
        /// </summary>
        /// <returns>The speed coff.</returns>
        public virtual float GetSpeedCoff() {
            return 1;
        }

        public virtual float GetMoveCoff()
        {
            return 1;
        }

        public virtual float GetTurnSpeed()
        {
            return 1;
        }
        public virtual int GetCriticalRate() {
            return 0;
        }

		public virtual int GetArmor ()
		{
			return 0;
		}
		protected BuffComponent GetBuffCom() {
			return obj.GetComponent<BuffComponent> ();
		}
        public virtual bool CanUseSkill() {
            return true;
        }

	    public virtual int GetExtraParams()
	    {
	        return 0;
	    }

        public virtual int GetDefaultSkill()
        {
            return 0;
        }

        public virtual string GetSkillName()
        {
            return null;
        }
	}
}
