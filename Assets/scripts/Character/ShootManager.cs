using UnityEngine;
using System.Collections;
using MyLib;

//进入Shoot状态会产生一个瞄准线在当前坦克的视野方向位置
//当前Ray方向

public class ShootSector : MonoBehaviour
{
	private ShootManager sm;
	private float degree = 10;
	private float distance = 20;
	private GameObject sector;

	private float sectorLength = 15;

	public enum SectorState
	{
		Idle,
		Target,
	}

	private SectorState state = SectorState.Idle;

    private NpcAttribute attr;
	void Start ()
	{
		sector = Object.Instantiate (Resources.Load<GameObject> ("levelPublic/ShootSector")) as GameObject;
		sector.SetActive (false);
        GameObject.DontDestroyOnLoad(sector);
		sm = GetComponent<ShootManager> ();
	    attr = NetworkUtil.GetAttr(this.gameObject);
	    sm.attr = attr;
	}

    void OnDestroy()
    {
        GameObject.Destroy(sector);
    }
	void Update ()
	{
		if (sm.Shoot) {
			if (state == SectorState.Idle)
			{
				if (CheckHit ()) {
					ShowSector ();	
				}
			} else {
				if (CheckHit ()) {
					ShowSector ();
				} else {
					HideSector ();
				}
			}
		} else {
			HideSector ();
		}
	}

	private void HideSector ()
	{
		sector.SetActive (false);
		state = SectorState.Idle;
	}

	//ShootManager 朝向是当前摇杆操控朝向
	//而扇形需要在操控朝向和目标朝向之间插值
	private void ShowSector ()
	{
	    sectorLength = GameConst.Instance.SectorLength;

		state = SectorState.Target;
		sector.SetActive (true);
		//var curDir = transform.parent.forward;
	    var curDir = Quaternion.Euler(new Vector3(0, sm.controlDir, 0)) * Vector3.forward;
		var targetDir = hitTarget.transform.position - transform.position;
		targetDir.y = 0;
		targetDir.Normalize ();
		var theta = Util.DegBetweenVec (curDir, targetDir);
		var endWidth = 2 * Mathf.Tan (Mathf.Deg2Rad * theta / 2)*sectorLength	;
		var lr = sector.GetComponent<LineRenderer> ();
		lr.SetWidth (0.01f, endWidth);

		//var curDeg = transform.parent.eulerAngles.y;
	    var curDeg = sm.controlDir;
		var secDeg = curDeg + theta / 2;
		//Debug.Log("curDeg theta: "+curDeg+" t "+theta);
		sector.transform.rotation = Quaternion.Euler (new Vector3 (0, secDeg, 0));
		sector.transform.position = transform.position;
	}


	public GameObject hitTarget;
    public bool IsHitTarget = false;
    //3条射线，当前角度 偏左 偏右角度
    private bool CheckHit()
    {
        if (attr == null)
        {
            return false;
        }

        degree = GameConst.Instance.SectorDegree;
        distance = GameConst.Instance.SectorDistance;

        //var curDir = transform.parent.rotation.eulerAngles.y;
        var curDir = sm.controlDir;
        //var dir = transform.parent.forward;
        var dir = Quaternion.Euler(new Vector3(0, curDir, 0))*Vector3.forward;
        //主体的位置
        var pos = attr.gameObject.transform.position + new Vector3(0, 0.3f, 0) + dir*1; //向前偏移一些距离

        var forwardDir = Quaternion.Euler(new Vector3(0, curDir, 0))*Vector3.forward;
        var left = Quaternion.Euler(new Vector3(0, curDir - degree, 0))*Vector3.forward;
        var right = Quaternion.Euler(new Vector3(0, curDir + degree, 0))*Vector3.forward;
        var tests = new Vector3[]
        {
            forwardDir, left, right,
        };

        RaycastHit hitInfo;
        bool hitYet = false;
        hitTarget = null;
        foreach (var d in tests)
        {
            var h = Physics.Raycast(pos, d, out hitInfo, distance, SkillDamageCaculate.GetDamageLayer());
            Log.Sys("HitTarget: " + h + " pos " + hitInfo.collider + " d " + d + " pos " + pos);
            if (h)
            {
                Log.Sys("HitTarget: " + h + " pos " + hitInfo.collider);
                if (hitInfo.collider.gameObject.tag == GameTag.Player)
                {
                    hitYet = true;
                    hitTarget = hitInfo.collider.gameObject;
                    break;
                }
            }
        }
        IsHitTarget = hitYet;
        Log.Sys("HitResult:"+IsHitTarget);
        return hitYet;
    }
}

public class ShootManager : MonoBehaviour
{
	private GameObject ray;

	public bool Shoot = false;
    private ShootSector sector;
    private UILabel cancelShoot;
    public TowerAutoCheck autoCheck;

    public float maxRotateChange = 3.0f;

    public NpcAttribute attr;
	IEnumerator Start ()
	{
		ray = Object.Instantiate (Resources.Load<GameObject> ("levelPublic/ShootRay")) as GameObject;
        GameObject.DontDestroyOnLoad(ray);
		sector = gameObject.AddComponent<ShootSector> ();
		ray.SetActive (false);
		var eh = this.gameObject.AddComponent<EvtHandler> ();
		eh.AddEvent (MyEvent.EventType.EnterShoot, EnterShoot);
		eh.AddEvent (MyEvent.EventType.ExitShoot, ExitShoot);
		eh.AddEvent (MyEvent.EventType.ShootDir, ShootDir);

	    var cs = GameObject.Instantiate(Resources.Load<GameObject>("UI/CancelShot")) as GameObject;
	    var uiRoot = WindowMng.windowMng.GetUIRoot();
	    while (uiRoot == null)
	    {
	        uiRoot = WindowMng.windowMng.GetUIRoot();
	        yield return null;
	    }
	    cs.transform.parent = uiRoot.transform;
        Util.InitGameObject(cs);
	    cancelShoot = cs.GetComponent<UILabel>();
        cancelShoot.gameObject.SetActive(false);

        NGUITools.AddMissingComponent<EvtHandler>(this.gameObject).AddEvent(MyEvent.EventType.CancelShoot, evt =>
        {
            if (evt.boolArg)
            {
                cancelShoot.gameObject.SetActive(false);
            }
            else
            {
                cancelShoot.gameObject.SetActive(true);
            }
        });
	}


    void OnDestroy()
    {
        GameObject.Destroy(ray);
    }

    private float startDir = 0;
	void EnterShoot (MyEvent evt)
	{
	    if (autoCheck.IsDead)
	    {
	        return;
	    }
	    if (Shoot)
	    {
	        return;
	    }
		ray.SetActive (true);
		Shoot = true;
	    startDir = transform.parent.rotation.eulerAngles.y;
	}

	void ExitShoot (MyEvent evt)
	{
        Log.Sys("ExitShoot:"+evt.type);
	    if (!Shoot)
	    {
	        return;
	    }
		Shoot = false;
	    StartCoroutine(CheckLastDir(evt.boolArg));
	}

    public float controlDir;
    private Vector3 lastDir;
    private void ShootDir(MyEvent evt)
    {
        Log.Sys("ShootDir: "+evt.vec2);
        if (this != null)
        {
            if (!autoCheck.IsDead)
            {
                //Vector3 lastDir;
                var conDir = new Vector3(evt.vec2.x, 0, evt.vec2.y);
                //controlDir = conDir;
                if (conDir.sqrMagnitude <= 0.1f)
                {
                    return;
                }
                conDir.Normalize();
                controlDir = Quaternion.FromToRotation(Vector3.forward, conDir).eulerAngles.y;
                if (sector.IsHitTarget && sector.hitTarget != null)
                {
                    var targetDir = sector.hitTarget.transform.position - transform.position;
                    targetDir.y = 0;
                    targetDir.Normalize();
                    //transform.parent.forward = targetDir;
                    //lastDir = targetDir;
                    lastDir = ((targetDir + conDir)).normalized;
                    //lastDir = conDir;
                }
                else
                {
                    //transform.parent.forward = conDir;
                    lastDir = conDir;
                }

                transform.forward = conDir;
            }
        }
    }

    private IEnumerator CheckLastDir(bool useSkill)
    {
        while (!Shoot)
        {
            var ld = Quaternion.FromToRotation(Vector3.forward, lastDir).eulerAngles.y;
            var diffY = ld - startDir;
            diffY = Util.NormalizeDiffDeg(diffY);
            if (Mathf.Abs(diffY) < 10)
            {
                break;
            }
            yield return null;
        }

        if (!Shoot)
        {
            ray.SetActive(false);
            cancelShoot.gameObject.SetActive(false);
            if (useSkill)
            {
                GameInterface.gameInterface.PlayerAttack();
            }
        }
    }

    // Update is called once per frame
	void FixedUpdate ()
	{
	    if (ray.activeSelf && !autoCheck.IsDead)
	    {
            maxRotateChange = GameConst.Instance.TowerMaxRotateChange;

	        ray.transform.position = transform.position;
	        //ray.transform.rotation = Quaternion.Euler(new Vector3(0, transform.parent.eulerAngles.y, 0));
	        ray.transform.rotation = Quaternion.Euler(new Vector3(0, controlDir, 0));


	        if (Camera.main == null)
	        {
	            return;
	        }

	        Vector3 sp = Camera.main.WorldToScreenPoint(transform.position + transform.forward + new Vector3(0, 2.5f, 0));
	        var uiWorldPos = UICamera.mainCamera.ScreenToWorldPoint(sp);
	        uiWorldPos.z = 0;
	        cancelShoot.transform.position = uiWorldPos;

	        var ld = Quaternion.FromToRotation(Vector3.forward, lastDir).eulerAngles.y;
            Log.Sys("LastDir: "+ld);
	        //var curDir = transform.parent.forward;
	        //var diffDir = Quaternion.FromToRotation(curDir, lastDir);
	        //var diffY = diffDir.eulerAngles.y;
	        var diffY = ld - startDir;
	        diffY = Util.NormalizeDiffDeg(diffY);
            //Log.Sys("DiffY: "+diffY);
	        var dy = Mathf.Clamp(diffY, -maxRotateChange, maxRotateChange)*attr.GetTurnSpeedCoff();
            //var delta = Quaternion.Euler(new Vector3(0, dy, 0));
	        //transform.localRotation *= delta;
	        //transform.parent.localRotation *= delta;
	        var newDir = startDir + dy;
	        transform.parent.rotation = Quaternion.Euler(new Vector3(0, newDir, 0));
	        startDir = newDir;
	    }
	}
}
