
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
using MyLib;
using System.Collections.Generic;

namespace MyLib
{
    //[RequireComponent(typeof(ShadowComponent))]
    [RequireComponent(typeof(NpcEquipment))]
    [RequireComponent(typeof(MyAnimationEvent))]
    [RequireComponent(typeof(KBEngine.KBNetworkView))]
    public class SelectChar : MonoBehaviour
    {
        public string newName;
        public long playerId;
        public int level;
        public uint job;
        public RolesInfo roleInfo;
        private Vector3 oldScale;

        void Awake()
        {
            var body = NGUITools.AddMissingComponent<Rigidbody>(gameObject);
            body.useGravity = false;
            body.freezeRotation = true;
            body.isKinematic = true;
            oldScale = transform.localScale;
            Debug.LogError("OldScale: " + oldScale);

            /*
            var tower = transform.FindChild("tower/tower2");
            tower.localRotation = Quaternion.Euler(new Vector3(-90, -128, 0));
            */

        }

        //private Transform tower;
        void Start()
        {
            this.transform.localRotation = Quaternion.Euler(new Vector3(0, 169.1f, 0));
            transform.localScale = Vector3.one;
            //tower = transform.Find("tower");
            //StartCoroutine(RotTower());
        }

        /*
        private IEnumerator RotTower()
        {
            var sa = Resources.Load<GameObject>("CameraShake/rotAnim");
            var cs = sa.GetComponent<CameraShakeData>();
            var passTime = 0.0f;
            var initPos = tower.transform.eulerAngles.y;
            var tm = 4.0f;
            while (true)
            {
                passTime %= tm;
                var rate = passTime/tm;
                var v = cs.shakeCurve.Evaluate(rate);
                var pos = v*cs.MaxOffset;
                var newPos = initPos + pos;
                tower.eulerAngles = new Vector3(0, newPos, 0);
                yield return null;
                passTime += Time.deltaTime;
            }

        }
        */

        void OnEnable()
        {
            /*
            animation ["stand"].wrapMode = WrapMode.Loop;
            animation.CrossFade ("stand");
            */
           // GetComponent<ShadowComponent>().HideShadow();
        }
        // Update is called once per frame
        void Update()
        {
            //Debug.Log (animation.isPlaying);
        }

        static Dictionary<long, GameObject> fakeObj = new Dictionary<long, GameObject>();

        //ui界面构建玩家模型
        public static GameObject ConstructChar(Job job)
        {

            var udata = Util.GetUnitData(true, (int)Job.WARRIOR, 0);
            GameObject player = null;
            player = Instantiate(Resources.Load<GameObject>(udata.ModelName)) as GameObject;
            var oldScale = player.transform.localScale;
            NGUITools.AddMissingComponent<NpcAttribute>(player);
            var selChar = NGUITools.AddMissingComponent<SelectChar>(player);

            selChar.GetComponent<NpcAttribute>().SetObjUnitData(udata);
            selChar.GetComponent<NpcEquipment>().InitDefaultEquip();
            player.transform.localScale = oldScale;
            return player;
        }
        /*
     * 4 Job Start From 0
     */
        public static GameObject ConstructChar(RolesInfo roleInfo)
        {
            Log.Sys("SelectChar::ConstructChar " + roleInfo);
            GameObject player = null;
            if (fakeObj.TryGetValue(roleInfo.PlayerId, out player))
            {
                return player;
            }

            var udata = Util.GetUnitData(true, (int)Job.WARRIOR, 0);
            Log.Sys("udata " + udata + " " + udata.name + " " + udata.ModelName);
            player = Instantiate(Resources.Load<GameObject>(udata.ModelName)) as GameObject;
            var oldScale = player.transform.localScale;

            NGUITools.AddMissingComponent<NpcAttribute>(player);
            var selChar = NGUITools.AddMissingComponent<SelectChar>(player);
            player.GetComponent<NpcAttribute>().SetObjUnitData(udata);
            player.GetComponent<NpcEquipment>().InitDefaultEquip();


            selChar.name = roleInfo.Name;
            selChar.playerId = roleInfo.PlayerId;
            selChar.level = roleInfo.Level;
            selChar.roleInfo = roleInfo;

            player.transform.localScale = oldScale;
            return player;
        }
    }
}
