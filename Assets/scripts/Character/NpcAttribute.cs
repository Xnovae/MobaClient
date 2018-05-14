
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public enum CharacterState
    {
        Idle,
        Running,
        Attacking,
        Around,
        Stunned,
        Dead,
        Birth,
        CastSkill,
        Story,
        Patrol,

        Flee,
    };


    /// <summary>
    /// 其它组件访问对象的数据 都通过 NpcAttribute 进行
    /// </summary>
    public class NpcAttribute : MonoBehaviour
    {
        public CharacterState _characterState = CharacterState.Idle;
        public int OwnerId = -1;

        public string userName;
        /// <summary>
        /// Monster SpawnObject 
        /// </summary>
        public GameObject spawnTrigger;

        public Vector3 OriginPos
        {
            get;
            private set;
        }
        //废弃
        public float MoveSpeed = 0;

        public void SetOwnerId(int ownerId)
        {
            OwnerId = ownerId;
        }

        public GameObject GetOwner()
        {
            return ObjectManager.objectManager.GetLocalPlayer(OwnerId);
        }

        public float FastRotateSpeed
        {
            get
            {
                return 10;
            }
        }

        public float WalkSpeed
        {
            get
            {
                //return ObjUnitData.MoveSpeed;
                return 0;
            }
        }

        private bool _sb = true;

        public bool ShowBloodBar
        {
            get
            {
                return _sb;
            }
            set
            {
                _sb = value;
                if (!_sb)
                {
                    var bb = GetComponent<BloodBar>();
                    if (bb != null)
                    {
                        bb.HideBar();
                    }
                } else
                {
                    var bb = GetComponent<BloodBar>();
                    if (bb != null)
                    {
                        bb.ShowBar();
                    }

                }
            }
        }

        //[NpcAttributeAtt()]
        public float ApproachDistance
        {
            get
            {
                if (_ObjUnitData != null)
                {
                    return _ObjUnitData.ApproachDistance;
                }
                Debug.LogError("not init ObjData " + gameObject);
                return 0;
            }
        }


        /// <summary>
        /// 远程网络直接设置控制HP 
        /// </summary>
        /// <value>The H.</value>
        public int HP
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP, value);
            }
        }

        public bool IsMaster = false;
        public int TeamColor = 0;

        public void SetTeamColorNet(int teamColor)
        {
            TeamColor = teamColor;
            MyEventSystem.PushLocalEventStatic(GetLocalId(), MyEvent.EventType.TeamColor);
            SetTeamShader();
        }

        void SetTeamShader()
        {
            return;
        }

        public void SetShadowLayer()
        {
            foreach (var render in renders)
            {
                render.gameObject.layer = (int) GameLayer.ShadowMap;
            }
        }

        public void SetMotionLayer()
        {
            var renders = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            var render2 = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renders)
            {
                r.gameObject.layer = (int)GameLayer.MotionBlur;
            }
            foreach (var r in render2)
            {
                if (r.gameObject.name != "playerLight(Clone)")
                {
                    r.gameObject.layer = (int)GameLayer.MotionBlur;
                }
            }
        }

        private List<GameObject> renders = new List<GameObject>();
        private void CollectRender()
        {
            foreach (Transform t in transform)
            {
                if (t.GetComponent<Renderer>() != null && t.name != "playerLight(Clone)")
                {
                    renders.Add(t.gameObject);
                }
            }
            
        }

        public void SetNormalLayer()
        {
            foreach (var render in renders)
            {
                render.layer = (int) GameLayer.Npc;
            }
        }

        public void SetTeamHideShader()
        {
            return;
        }

        private Dictionary<string,Material> liveMat = new Dictionary<string, Material>();
        private Dictionary<string, Material> MVPMat = new Dictionary<string, Material>(); 
        private Dictionary<string, Material> NormalMat = new Dictionary<string, Material>(); 
        private Dictionary<string, Renderer> liveRender = new Dictionary<string, Renderer>(); 
        public void SetDeadShader()
        {
            var td = Resources.Load<Material>("Materials/tankDead");
            SaveMaterial();
            SetMaterial(td);
            Log.Sys("SetDeadShader");
        }

        public GameObject tower = null;

        private void SaveMaterial()
        {
            var mr = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in mr)
            {
                if (!liveMat.ContainsKey(meshRenderer.name))
                {
                    if (meshRenderer.name == "body" || meshRenderer.name == "tower2")
                    {
                        liveMat.Add(meshRenderer.name, meshRenderer.material);

                        var mt = MVPMat[meshRenderer.name] = new Material(meshRenderer.material);
                        var tex = Resources.Load<Texture2D>("player/tank-01_1");
                        mt.SetTexture("_MainTex", tex);

                        NormalMat[meshRenderer.name] = meshRenderer.material;
                        Log.Sys("SaveMat: "+meshRenderer.name);
                    }
                    liveRender[meshRenderer.name] = meshRenderer;
                }
            }

            if (tower != null)
            {
                var mr2 = tower.GetComponentInChildren<MeshRenderer>();
                if (mr2 != null)
                {
                    if (!liveMat.ContainsKey(mr2.name))
                    {
                        liveMat.Add(mr2.name, mr2.material);
                        var mt = MVPMat[mr2.name] = new Material(mr2.material);
                        var tex = Resources.Load<Texture2D>("player/tank-01_1");
                        mt.SetTexture("_MainTex", tex);

                        NormalMat[mr2.name] = mr2.material;
                        liveRender[mr2.name] = mr2;
                        Log.Sys("SaveMat: "+mr2.name);
                    }
                }
            }
        }

        private void SetMaterial(Material td)
        {
            var mr = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in mr)
            {
                if(meshRenderer.name == "body" || meshRenderer.name == "tower2")
                {
                    meshRenderer.material = td;
                }
            }
            if (tower != null)
            {
                tower.GetComponentInChildren<MeshRenderer>().material = td;
            }
            //Debug.LogError("SetMaterial: "+td);
        }

        private void RestoreMat()
        {
            Log.Sys("RestoreMat");
            if (IsDead)
            {
                return;
            }
            var mr = gameObject.GetComponentsInChildren<MeshRenderer>();
            var i = 0;
            foreach (var meshRenderer in mr)
            {
                if (meshRenderer.name != "playerLight(Clone)")
                {
                    if (liveMat.ContainsKey(meshRenderer.name))
                    {
                        meshRenderer.material = liveMat[meshRenderer.name];
                    }
                    i++;
                }
            }
            if (tower != null)
            {
                var mr2 = tower.GetComponentInChildren<MeshRenderer>();
                if (liveMat.ContainsKey(mr2.name))
                {
                    mr2.material = liveMat[mr2.name];
                }
            }
            //Debug.LogError("RestoreMat");
        }

        public void SetLiveShader()
        {
            RestoreMat();
        }

        public void SetTeamNormalShader()
        {
            return;
            var shaderRes = Resources.Load<ShaderResource>("levelPublic/ShaderResource");
            var myPlayer = ObjectManager.objectManager.GetMyAttr();
            var renders = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            var render2 = gameObject.GetComponentsInChildren<MeshRenderer>();
            if (myPlayer.TeamColor != TeamColor)
            {
                foreach (var r in renders)
                {
                    r.material.shader = Shader.Find("Custom/playerHideShader");
                }
                foreach (var r in render2)
                {
                    if (r.gameObject.name != "playerLight(Clone)")
                    {
                        r.material.shader = Shader.Find("Custom/playerHideShader");
                    } else
                    {
                        r.enabled = true;
                    }
                }

            } else
            {
                foreach (var r in renders)
                {
                    r.material.shader = Shader.Find("Custom/playerShader");
                }
                foreach (var r in render2)
                {
                    if (r.gameObject.name != "playerLight(Clone)")
                    {
                        r.material.shader = Shader.Find("Custom/playerShader");
                    }
                }
            }
        }

        public void SetIsMasterNet(bool isMaster)
        {
            Log.Sys("IsMasterNet: " + isMaster);
            IsMaster = isMaster;
            MyEventSystem.PushLocalEventStatic(GetLocalId(), MyEvent.EventType.IsMaster);
        }

        public void SetHPNet(int hp)
        {
            Log.Sys("SetHPNet: " + hp + " g " + gameObject);
            GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP, hp);
            NotifyHP();
        }

        public int HP_Max
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.HP_MAX);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.HP_MAX, value);
            }
        }

        public int MP
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.MP);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.MP, value);
            }
        }

        public int MP_Max
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.MP_MAX);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.MP_MAX, value);
            }
        }


        //TODO::调整人物属性采用当前游戏的数据设定
    

        public int Exp
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.EXP);
            }
        
            private set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.EXP, value);
            }

        }

        //TODO: 技能点应该属于Skill系统
        //public int AttributePoint = 0;


        bool _isDead = false;

        public delegate void SetDead(GameObject g);

        public SetDead SetDeadDelegate;

        //玩家升级后设置等级
        //int _Level = 1;
        public int Level
        {
            get
            {
                return GetComponent<CharacterInfo>().GetProp(CharAttribute.CharAttributeEnum.LEVEL);
            }
            set
            {
                GetComponent<CharacterInfo>().SetProp(CharAttribute.CharAttributeEnum.LEVEL, value);
                SetLevel();
            }
        }

        public bool IsDead
        {
            get
            {
                return _isDead;
            }
            set
            {
                if (_isDead == value)
                {
                    return;
                }
                _isDead = value;
                if (SetDeadDelegate != null && _isDead)
                {
                    SetDeadDelegate(gameObject);
                }
                if (_isDead)
                {
                    GetComponent<BuffComponent>().RemoveAllBuff();
                }

                if (ObjectManager.objectManager != null && _isDead)
                {
                    if (ObjectManager.objectManager.killEvent != null)
                    {
                        ObjectManager.objectManager.killEvent(gameObject);
                    }
                }

            }
        }

       

        private int _Damage
        {
            get
            {
                return _ObjUnitData.Damage;
            }
        }


        //int _PoisonDefense = 0;
        public int PoisonDefense
        {
            get
            {
                return GetWaterDefense();
            }
        }

        int _Armor
        {
            get
            {
                return _ObjUnitData.Armor;
            }
        }

        public int Armor
        {
            get
            {
                return GetAllArmor();
            }
        }
        private NpcConfig _npcconfig;
        public NpcConfig npcConfig
        {
            get
            {
                if(_npcconfig == null)
                {
                    _npcconfig =  NpcDataManager.Instance.GetConfig(_ObjUnitData.ID);
                }
                if(_npcconfig == null)
                {
                    _npcconfig = NpcConfig.defaultConfig;
                }
                return _npcconfig;
            }
        }
        UnitData _ObjUnitData;

        public UnitData ObjUnitData
        {
            private set
            {
                _ObjUnitData = value;
                if (_ObjUnitData.TextureReplace.Length > 0)
                {
                    SetTexture(_ObjUnitData.TextureReplace);
                }
            }
            get
            {
                return _ObjUnitData;
            }
        }

        NpcEquipment npcEquipment;

        public float AttackRange
        {
            get
            {
                if (_ObjUnitData != null)
                {
                    return _ObjUnitData.AttackRange;
                }
                Debug.LogError("not init ObjData " + gameObject);
                return 0;
            }
        }

        public float ReachRange
        {
            get
            {
                return 2;
            }
        }

        public float PatrolRange
        {
            get
            {
                return 5;
            }
        }

        public void ResetAttribute()
        {
            HP_Max = _ObjUnitData.HP;
            MP_Max = _ObjUnitData.MP;
            ChangeHP(0);
            ChangeMP(0);
        }

        public void SetHPMax(int num)
        {
            HP_Max = num;
            ChangeHP(0);
        }


        public void AddMpMax(int num)
        {
            MP_Max += num;
            ChangeMP(0);
        }

        void InitData()
        {
            Log.Important("Initial Object HP " + gameObject.name);
            var characterInfo = GetComponent<CharacterInfo>();
            if (ObjUnitData != null && characterInfo != null)
            {
                var view = GetComponent<KBEngine.KBNetworkView>(); 
                Log.Important("Player View State " + gameObject.name + " " + view.IsPlayer + " " + view.IsMine);
                HP_Max = _ObjUnitData.HP;
                HP = HP_Max;
                MP_Max = _ObjUnitData.MP;
                MP = MP_Max;
                Log.Important("Init Obj Data  " + gameObject.name + " " + HP + " " + _ObjUnitData.HP);
                ChangeHP(0);
                ChangeMP(0);
            }
        }

        public KBEngine.KBNetworkView GetNetView()
        {
            return GetComponent<KBEngine.KBNetworkView>();
        }

        //玩家升级后设置等级 调整对应UnitData
        void SetLevel()
        {
            var udata = Util.GetUnitData(_ObjUnitData.GetIsPlayer(), _ObjUnitData.ID, Level);
            SetObjUnitData(udata);
        }

        public Job job = Job.WARRIOR;
        CharacterInfo charInfo;
        /// <summary>
        /// 区分炮管和身体
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitSetColor()
        {
            yield return null;
            SaveMaterial();
            foreach (var material1 in liveMat)
            {
                //if (material1.Key == "tower2")
                {
                    if (job == Job.WARRIOR)
                    {
                        material1.Value.SetColor("_MainColor", Color.white);
                    }
                    else if (job == Job.ALCHEMIST)
                    {
                        material1.Value.SetColor("_MainColor", Color.red);
                    }
                    else if (job == Job.ARMOURER)
                    {
                        material1.Value.SetColor("_MainColor", Color.green);
                    }
                }
            }

            if (GetNetView().IsMe)
            {
                ServerData.Instance.playerInfo.Roles.Job = job;
            }
            if (!IsDead)
            {
                RestoreMat();
            }
        }

        public void SetJob(Job j)
        {
            Log.Sys("SetJob: "+j);
            job = j;
            var udata = Util.GetUnitData(true, (int)job, Level);
            SetObjUnitData(udata);
            StartCoroutine(WaitSetColor());
        }

        public void InitName()
        {
            userName = ServerData.Instance.playerInfo.Roles.Name;
        }

        public void SetObjUnitData(UnitData ud)
        {
            ObjUnitData = ud;
            _npcconfig = null;
            InitData();
        }

        /*
         * Player Equipment PoisonDefense
         * Monster Define in UnitData
         */
        int GetWaterDefense()
        {
            int d = 0;
            if (npcEquipment != null)
            {
                d += npcEquipment.GetPoisonDefense();
            }
            return d;
        }

        /*
         * BaseWeapon Damage
         * Fire Element Damage  Ice Element Electric
         */
        public int GetAllDamage(bool isStaticShoot)
        {
            int d = _Damage;

            if (isStaticShoot)
            {
                d = (int)(_Damage * GameConst.Instance.StaticShootBuffDamageRatio);
            }

            if (npcEquipment != null)
            {
                d += npcEquipment.GetDamage();
            }
            Log.Sys("Damage is what  " + d + " g " + gameObject);
            return d;
        }

        public float JumpForwardSpeed = 0;

        public float NetSpeed = 0;

        public void SetNetSpeed(float v)
        {
            NetSpeed = v;
        }
        public void AddNetSpeed(float v)
        {
            NetSpeed += v;
        }

        public bool GetStaticShootBuff()
        {
            var buffComp  = GetComponent<BuffComponent>();
            var super = buffComp.GetBuff(Affix.EffectType.SuperShootBuff);
            return !super && gameObject.GetComponent<BuffComponent>().GetBuff(Affix.EffectType.StaticShootBuff);
        }

        public float GetMoveSpeedCoff()
        {
            var speed = GetComponent<BuffComponent>().GetMoveSpeedCoff() + NetSpeed;
            if (IsMine())
            {
                return speed;
            }
            else
            {
                return speed*1.2f;
            }
        }

        public float GetTurnSpeedCoff()
        {
            return GetComponent<BuffComponent>().GetTurnSpeed();
        }

        public float ThrowSpeed = 0;

        public void AddThrowSpeed(float v)
        {
            ThrowSpeed += v;
        }
        public float GetSpeed()
        {
            //return _ObjUnitData.MoveSpeed;
            return npcConfig.moveSpeed;
        }

        public float GetSpeedCoff()
        {
            return GetComponent<BuffComponent>().GetSpeedCoff() + ThrowSpeed;
        }

        public int GetCriticalRate()
        {
            return ObjUnitData.CriticalHit + GetComponent<BuffComponent>().GetCriticalRate();
        }

        int GetAllArmor()
        {
            int a = _Armor;
            if (npcEquipment != null)
            {
                a += npcEquipment.GetArmor();
            }
            a += GetComponent<BuffComponent>().GetArmor();
            return a;
        }

        public void Init()
        {
            npcEquipment = GetComponent<NpcEquipment>();
            charInfo = GetComponent<CharacterInfo>();
        }

        void Start()
        {
            Init();
            OriginPos = transform.position;
            StartCoroutine(AdjustOri());
            gameObject.name += "_" + GetLocalId();
            CollectRender();
            var ld = GetLocalId();
            var eh = NGUITools.AddMissingComponent<EvtHandler>(this.gameObject);
            eh.AddEvent(MyEvent.EventType.IAmFirst, OnFirst);
        }

        void OnFirst(MyEvent evt)
        {
            /*
            SaveMaterial();
            var serverId = GetNetView().GetServerID();
            if (NormalMat.ContainsKey("body") && MVPMat.ContainsKey("body"))
            {
                if (evt.intArg == serverId)
                {
                    liveMat["body"] = MVPMat["body"];
                    liveMat["tower2"] = MVPMat["tower2"];
                }
                else
                {
                    liveMat["body"] = NormalMat["body"];
                    liveMat["tower2"] = NormalMat["tower2"];
                }
            }
            //SetJob(this.job);
            StartCoroutine(WaitSetColor());
            if (!IsDead)
            {
                RestoreMat();
            }
            */

        }


        /// <summary>
        /// 等人物掉 地面上再初始化 
        /// </summary>
        /// <returns>The ori.</returns>
        IEnumerator AdjustOri()
        {
            yield return new WaitForSeconds(0.5f);
            OriginPos = transform.position;
        }

        /// <summary>
        /// 是否是本地玩家控制对象 
        /// </summary>
        /// <returns><c>true</c> if this instance is me; otherwise, <c>false</c>.</returns>
        public bool IsMine()
        {
            return GetComponent<KBEngine.KBNetworkView>().IsMine;
        }

        /// <summary>
        /// 不是自己控制的对象则是代理
        /// 代理释放的技能不会产生伤害
        /// </summary>
        /// <returns><c>true</c> if this instance is proxy; otherwise, <c>false</c>.</returns>
        public bool IsProxy()
        {
            return !GetComponent<KBEngine.KBNetworkView>().IsMine;
        }

        public int GetLocalId()
        {
            return GetComponent<KBEngine.KBNetworkView>().GetLocalId();
        }

        /// <summary>
        /// 属性的修改都是对象自己负责自己的 其它人不能修改 
        /// 属性是可以同步的
        /// </summary>
        /// <param name="c">C.</param>
        public void ChangeHP(int c)
        {
            if (IsMine())
            { 
                HP += c;
                HP = Mathf.Min(Mathf.Max(0, HP), HP_Max);
                Log.GUI("Init GameObject HP " + gameObject.name + " HP " + HP);

                NotifyHP();
                if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
                {
                    MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                }
            }
        }

        public void NotifyHP()
        {
            var evt1 = new MyEvent(MyEvent.EventType.UnitHP);
            evt1.localID = GetLocalId();
            MyEventSystem.myEventSystem.PushLocalEvent(evt1.localID, evt1);
        }

        public void ChangeMP(int c)
        {
            if (IsMine())
            {
                MP += c;
                MP = Mathf.Min(Mathf.Max(0, MP), MP_Max);
                var rate = MP * 1.0f / MP_Max * 1.0f;

                var evt = new MyEvent(MyEvent.EventType.UnitMPPercent);
                evt.localID = GetLocalId();

                evt.floatArg = rate;
                MyEventSystem.myEventSystem.PushEvent(evt);

                var evt1 = new MyEvent(MyEvent.EventType.UnitMP);
                evt1.localID = GetLocalId();
                evt1.intArg = MP;
                evt1.intArg1 = MP_Max;
                MyEventSystem.myEventSystem.PushEvent(evt1);

                if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
                {
                    MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                }
            }
        }

        /*
         * Damage Type 
         */
        public void DoHurt(int v, bool isCritical, bool isStaticShoot,SkillData.DamageType dt = SkillData.DamageType.Physic)
        {
            Log.Sys("NpcAttribute::DoHurt Name:" + gameObject.name + " hurtValue:" + v + " Armor:" + Armor + " DamageType " + dt);
            if (dt == SkillData.DamageType.Physic)
            {
                int hurt = v - Armor;
                Log.Important("Get Hurt is " + hurt);
                if (hurt > 0)
                {
                    if (!isCritical && !isStaticShoot)
                    {
                        PopupTextManager.popTextManager.ShowWhiteText("-" + hurt.ToString(), transform);
                    }
                    else if (!isCritical && isStaticShoot)
                    {
                        PopupTextManager.popTextManager.ShowYellowText("-" + hurt.ToString(), transform);
                    }
                    else
                    {
                        PopupTextManager.popTextManager.ShowPurpleText("-" + hurt.ToString(), transform);
                    }
                    ChangeHP(-hurt);
                } else
                {
                    Log.Important("Armor too big for player " + Armor);
                }
            } else if (dt == SkillData.DamageType.Water)
            {
                var d = GetWaterDefense();
                int hurt = (int)(v * (1 - d / 100.0f));
                if (hurt > 0)
                {
                    ChangeHP(-hurt);
                }
            }
        }

        //calculate Hurt event in stunned

        public bool CheckDead()
        {
            return (HP <= 0);
        }

        //精英怪或者怪物变种 需要替换纹理
        void SetTexture(string tex)
        {
            var skins = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            skins.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture>(tex);

        }

        public void SetExp(int e)
        {
            Exp = e;
            if (IsMine())
            {
                MyEventSystem.PushEventStatic(MyEvent.EventType.UpdatePlayerData);
            }
        }

        //TODO: 单人副本中需要判断是否升级以及升级相关处理
        public void ChangeExp(int e)
        {
            Exp += e;
            var maxExp = _ObjUnitData.MaxExp;

            if (Exp >= maxExp)
            {
                LevelUp();
            } else
            {
                /*
                if (IsMine())
                {
                    var sync = CGAddProp.CreateBuilder();
                    sync.Key = (int)CharAttribute.CharAttributeEnum.EXP;
                    sync.Value = e;
                    KBEngine.Bundle.sendImmediate(sync);
                }
                */
            }

            var evt = new MyEvent(MyEvent.EventType.UpdatePlayerData);
            evt.localID = GetLocalId();
            MyEventSystem.myEventSystem.PushEvent(evt);
            if (IsMine())
            {
                MyEventSystem.PushEventStatic(MyEvent.EventType.UpdatePlayerData);
            }
        }

        public void ChangeLevel(int lev)
        {
            Level = lev;
            if (GetLocalId() == ObjectManager.objectManager.GetMyLocalId())
            {
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdatePlayerData);
            }
        }

        //TODO:玩家升级的逻辑处理  技能点
        void LevelUp()
        {
            /*
            //Modify Hp Mp
            Level += 1;
            Exp = 0;

            Log.Net("AddLevelUp " + IsMine());
            if (IsMine())
            {
                var setSync = CGSetProp.CreateBuilder();
                setSync.Key = (int)CharAttribute.CharAttributeEnum.EXP;
                setSync.Value = 0;
                KBEngine.Bundle.sendImmediate(setSync);

                var sync = CGAddProp.CreateBuilder();
                sync.Key = (int)CharAttribute.CharAttributeEnum.LEVEL;
                sync.Value = 1;
                KBEngine.Bundle.sendImmediate(sync);
            }

            Util.ShowLevelUp(Level);
            var par = Instantiate(Resources.Load<GameObject>("particles/events/levelUp")) as GameObject;
            NGUITools.AddMissingComponent<RemoveSelf>(par);
            par.transform.parent = ObjectManager.objectManager.transform;
            par.transform.position = transform.position;

            if (IsMine())
            {
                //MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdateMainUI);
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.UpdatePlayerData);
            }
            */
        }



        //TODO: 掉落物品机制重新设计 掉落物品和掉落黄金
        public List<List<float>> GetDropTreasure()
        {
            var mod = 100;
            return _ObjUnitData.GetRandomDrop(mod / 100.0f);
        }


        private SkillData GetDeadSkill()
        {
            return GetComponent<SkillInfoComponent>().GetDeadSkill();
        }

        IEnumerator AddHpProgress(float duration, float totalAdd)
        {
            float addRate = totalAdd / duration;
            float goneTime = 0;
            int count = 0;
            int tc = Mathf.RoundToInt(duration / 0.1f);
            while (count < tc)
            {
                if (goneTime > 0.1f)
                {
                    HP += Mathf.RoundToInt(addRate * 0.1f);
                    HP = Mathf.Min(HP_Max, HP);
                    ChangeHP(0);
                    goneTime -= 0.1f;
                }
                goneTime += Time.deltaTime;
                count++;
                yield return null;
            }
        }

        //TODO: 吃个药瓶
        public void AddHp(float duration, float totalAdd)
        {
            StartCoroutine(AddHpProgress(duration, totalAdd));
        }

        IEnumerator AddMpProgress(float duration, float totalAdd)
        {
            float addRate = totalAdd / duration;
            float goneTime = 0;
            int count = 0;
            int tc = Mathf.RoundToInt(duration / 0.1f);
            while (count < tc)
            {
                if (goneTime > 0.1f)
                {
                    MP += Mathf.RoundToInt(addRate * 0.1f);
                    MP = Mathf.Min(MP_Max, MP);
                    ChangeMP(0);
                    goneTime -= 0.1f;
                }
                goneTime += Time.deltaTime;
                count++;
                yield return null;
            }
        }

        public void AddMp(float duration, float totalAdd)
        {
            StartCoroutine(AddMpProgress(duration, totalAdd));
        }

        public void OnlyShowDeadEffect()
        {
            _characterState = CharacterState.Dead;
            var sdata = GetDeadSkill();
            if (sdata != null)
            {
                StartCoroutine(SkillLogic.MakeSkill(gameObject, sdata, transform.position));
            }
            
        }

        /// <summary>
        /// 死亡时一系列操作 
        /// </summary>
        public void ShowDead()
        {
            DeadIgnoreCol();
            OnlyShowDeadEffect();
        }

        public void DeadIgnoreCol()
        {
            IsDead = true;
            if (ObjectManager.objectManager != null && ObjectManager.objectManager.myPlayerGameObj != null)
            {
                Physics.IgnoreCollision(GetComponent<CharacterController>(), ObjectManager.objectManager.GetMyPlayer().GetComponent<CharacterController>());
            }
        }

        public bool CheckAni(string name)
        {
            return GetComponent<Animation>().GetClip(name) != null; 
        }

        public float thresholdY = 3;
        public float totalTime = 3;
        private float currentTime = 0;
        private Material material;


        private void ReviveShader()
        {
            if (tower != null)
            {
                var tac = tower.GetComponent<TowerAutoCheck>();
                tac.SetDead(false);
            }
            SetLiveShader();
        }

        private IEnumerator SetRebornShader()
        {
            if (liveMat.Count == 0)
            {
                SaveMaterial();
            }

            currentTime = 0;
            material = new Material(Resources.Load<Material>("Materials/WireFrameReborn"));
            var mr = gameObject.GetComponentsInChildren<MeshRenderer>();

            MeshRenderer bodyRenderer = null;
            foreach (var meshRenderer in mr)
            {
                if (meshRenderer.name == "body" || meshRenderer.name == "tower2")
                {
                    meshRenderer.material = material;
                    bodyRenderer = meshRenderer;
                }
            }

            if (tower != null)
            {
                var mr2 = tower.GetComponentInChildren<MeshRenderer>();
                mr2.material = material;

                var tac = tower.GetComponent<TowerAutoCheck>();
                tac.SetDead(false);

                while (true)
                {
                    mr2.material.SetFloat("_ThresholdY", thresholdY*currentTime/totalTime);
                    bodyRenderer.material.SetFloat("_ThresholdY", thresholdY*currentTime/totalTime);
                    if (currentTime >= totalTime)
                    {
                        SetLiveShader();
                        break;
                    }
                    yield return null;
                    currentTime += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 其它玩家复活立即切换状态，重置HP
        /// 设置RebornShader
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public IEnumerator NetworkRevive(GCPlayerCmd cmd)
        {
            var ainfo = cmd.AvatarInfo;
            Log.Sys("Revive");
            var np = NetworkUtil.FloatPos(ainfo.X, ainfo.Y, ainfo.Z);
            Util.ForceResetPos(this.GetComponent<Rigidbody>(), np);
            var nr = Quaternion.Euler(new Vector3(0, ainfo.Dir, 0));
            this.GetComponent<Rigidbody>().MoveRotation(nr);
            SetHPNet(HP_Max);
            ReviveAI();
            //StartCoroutine(SetRebornShader());
            yield return null;
        }


        private void ReviveAI()
        {
            var ai = GetComponent<AIBase>().GetAI();
            ai.ChangeStateForce(AIStateEnum.IDLE);
            _isDead = false;
            ReviveShader();
        }

        /// <summary>
        /// 自己复活
        /// </summary>
        /// <returns></returns>
        public IEnumerator Revive()
        {
            GetComponent<BuffComponent>().RemoveAllBuff();
            ChangeHP(HP_Max);
            ChangeMP(MP_Max);
            this.SetNetSpeed(0);
            SetJob(Job.WARRIOR);
            var pos = NetworkUtil.GetStartPos();
            //transform.position = pos;
            //this.rigidbody.MovePosition(pos);
            Util.ForceResetPos(this.GetComponent<Rigidbody>(), pos);
            //StartCoroutine(SetRebornShader());

            ReviveAI();
            NetDateInterface.Revive();
            yield return null;
        }

        public void ShowMe()
        {
            SaveMaterial();
            if (this.IsMine())
            {
                foreach (var material1 in liveMat)
                {
                    material1.Value.SetFloat("_Alpha", 1f);
                }
                if (!IsDead)
                {
                    RestoreMat();
                }
            }
            else
            {
                ShowBloodBar = true;
                foreach (var renderer1 in liveRender)
                {
                    renderer1.Value.enabled = true;
                }
            }
        }

        public void HideMe()
        {
            SaveMaterial();
            if (this.IsMine())
            {
                foreach (var material1 in liveMat)
                {
                    material1.Value.SetFloat("_Alpha", 0.3f);
                }
                if (!IsDead)
                {
                    RestoreMat();
                }
            }
            else
            {
                ShowBloodBar = false; 
                foreach (var renderer1 in liveRender)
                {
                    renderer1.Value.enabled = false;
                }
            }

        }

    }

}