using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class TowerAutoCheck : MonoBehaviour
    {
        public float maxRotateChange = 9.0f;
        private Transform col;
        private Vector3 initPos;
        private Transform par;
        private Vector3 initScale;
        private void Awake()
        {
            var attr = NetworkUtil.GetAttr(this.gameObject);
            var parBox = Util.FindChildRecursive(transform.parent, "boxCollider");
            col = Util.FindChildRecursive(transform, "box");
            Physics.IgnoreCollision(col.GetComponent<Collider>(), parBox.GetComponent<Collider>());
            initPos = transform.localPosition * transform.parent.localScale.x;
            par = transform.parent;
            initScale = transform.lossyScale;

            attr.GetComponent<TankPhysicComponent>().tower = this.gameObject;
            attr.tower = this.gameObject;
        }

        public void AdjustY(float y)
        {
        }

        IEnumerator TraceParent()
        {
            yield return null;
            transform.parent = null;
            while (par != null && !IsDead)
            {
                var nPos = par.transform.position + initPos;
                transform.position = nPos;
                transform.localScale = initScale;
                yield return null;
            }
            if (par == null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        private void InitTowerTrace()
        {
            StartCoroutine(TraceParent());
        }
        public bool IsDead = false;
        public void SetDead(bool isDead)
        {
            if(this == null)
            {
                return;
            }

            IsDead = isDead;
            if (isDead)
            {
                //transform.parent = null;
                this.GetComponent<Rigidbody>().isKinematic = false;
                this.GetComponent<Rigidbody>().useGravity = true;
                col.GetComponent<Collider>().isTrigger = false;
                this.GetComponent<Rigidbody>().AddExplosionForce(500, par.transform.position+par.transform.forward*4, 10);
            }
            else
            {
                //transform.parent = par;
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = false;
                col.GetComponent<Collider>().isTrigger = true;
                //this.transform.localPosition = initPos;
                this.transform.localRotation = Quaternion.identity;
                this.transform.localScale = Vector3.one;
                InitTowerTrace();
            }
        }

        private void Start()
        {
            //StartCoroutine(AutoEnemy());
            var attr = NetworkUtil.GetAttr(gameObject);
            if (attr.IsMine())
            {
                var shootManager = new GameObject("ShootManager");
                shootManager.transform.parent = transform;
                Util.InitGameObject(shootManager);
                var sm = shootManager.AddComponent<ShootManager>();
                sm.autoCheck = this;
            }
            else
            {
                StartCoroutine(SynDir());
            }

            InitTowerTrace();
        }

        private int tarDir = 0;
        public void SyncTowerDir(int dir)
        {
            tarDir = dir;
        }

        private IEnumerator SynDir()
        {
            while (true)
            {
                if (!IsDead)
                {
                    maxRotateChange = GameConst.Instance.TowerMaxRotateChange;
                    var curDir = transform.eulerAngles.y;
                    var diffDir = tarDir - curDir;
                    var diffY = Util.NormalizeDiffDeg(diffDir);
                    var dy = Mathf.Clamp(diffY, -maxRotateChange, maxRotateChange);
                    var tarDir2 = Quaternion.Euler(new Vector3(0, curDir + dy, 0));
                    transform.rotation = tarDir2;
                }
                yield return new WaitForFixedUpdate();
            }
        }

      
    }
}
