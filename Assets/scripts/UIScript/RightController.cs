using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyLib;

public class RightBack : MonoBehaviour
{
	public enum RightBackState
	{
		Idle,
		Move,
	}

	public GUITexture tex;
	public RightController con;
	private RightBackState state = RightBackState.Idle;

    public float screenMoveWidthRatio = 0.8f;
    public float screenMoveHeightRatio = 0.8f;

    void Update ()
	{
		if (state == RightBackState.Idle) {
			var pos = new Vector2 (Screen.width - Screen.width / 7f, Screen.height / 4.5f);
			SetPos (pos);
		}
	}

	void SetPos (Vector2 p)
	{
        if (p.x - Screen.width * 0.5f * (1 + screenMoveWidthRatio) > 0)
        {
            p.x = Screen.width * 0.5f * (1 + screenMoveWidthRatio);
        }
        else if (p.x - Screen.width * 0.5f * (1 - screenMoveWidthRatio) < 0)
        {
            p.x = Screen.width * 0.5f * (1 - screenMoveWidthRatio);
        }

        var rsz = this.con.GetRealSize();

        if (p.y - Screen.height * 0.5f * (1 + screenMoveHeightRatio) + rsz.y / 2f > 0)
        {
            p.y = Screen.height * 0.5f * (1 + screenMoveHeightRatio) - rsz.y / 2f;

        }
        else if (p.y - Screen.height * 0.5f * (1 - screenMoveHeightRatio) - rsz.y /2f < 0)
        {
            p.y = Screen.height*0.5f*(1 - screenMoveHeightRatio) + rsz.y/2f;
        }

        tex.pixelInset = new Rect(p.x - rsz.x / 2, p.y - rsz.y / 2, rsz.x, rsz.y);
    }

	private bool first = false;
	private Vector2 lastPos = Vector2.zero;
	private Vector2 initPos = Vector2.zero;

	public void EnterMove ()
	{
		state = RightBackState.Move;
		first = true;
	}

	public void ExitMove ()
	{
		state = RightBackState.Idle;
	}

	private bool isCancel = true;

	public bool IsCancel ()
	{
		return isCancel;
	}

	public Vector2 GetPos ()
	{
		return initPos;
	}

	public void SetFingerPos (Vector2 pos)
	{
		if (first) {
			first = false;
			lastPos = pos;
			initPos = pos;
			isCancel = true;
			SetPos (pos);
		} else {
			//根据手指位置调整
			var diff = pos - initPos;
			var dir = pos - lastPos;
			lastPos = pos;

			//手指在中心半径范围内 50半径 相反运动
			var mag = diff.magnitude;
			var iner = this.con.InnerRadius * RightController.GetRate ();
			var external = this.con.ExternalRadius * RightController.GetRate ();
			if (mag < iner) {
				initPos -= dir;
				SetPos (initPos);

				//手指在圆环外面 跟随手指移动
			} else if (mag > external)
			{
			    var fingerDir = pos - initPos;
			    var distOff = fingerDir.normalized*external;
			    initPos = pos - distOff;
				//initPos += dir;
				SetPos (initPos);
			}
			if (mag < this.con.CancelRadius * RightController.GetRate ()) {
				MyEventSystem.myEventSystem.PushEvent (new MyEvent () {
					type = MyEvent.EventType.CancelShoot,
					boolArg = false,
				});
				isCancel = true;
			} else {
				MyEventSystem.myEventSystem.PushEvent (new MyEvent () {
					type = MyEvent.EventType.CancelShoot,
					boolArg = true,
				});
				isCancel = false;
			}
		}
	}

}

public class RightFinger : MonoBehaviour
{
	public RightController con;
	public GUITexture tex;

	public enum RightFingerState
	{
		Idle,
		Move,
	}

	private RightFingerState state = RightFingerState.Idle;
	private Vector2 fingerPos = Vector2.zero;

    void Update ()
	{
		if (state == RightFingerState.Idle) {
			var pos = new Vector2 (Screen.width - Screen.width / 7f, Screen.height / 4.5f);

			SetPos (pos);
		} else {
			SetPos (fingerPos);
		}
	}

	void SetPos (Vector2 p)
	{
		var rsz = this.con.GetFingerSize ();
		tex.pixelInset = new Rect (p.x - rsz.x / 2, p.y - rsz.y / 2, rsz.x, rsz.y);
	}

	public void EnterMove ()
	{
		state = RightFingerState.Move;
	}

	public void ExitMove ()
	{
		state = RightFingerState.Idle;
	}

	public void SetFingerPos (Vector2 pos)
	{
		fingerPos = pos;
	}

	public Vector2 GetPos ()
	{
		return fingerPos;
	}
}

public class RightController : MonoBehaviour
{
	public Color activeColor;
	public Color inactiveColor;

	public Texture2D joyStick;
	public Texture2D background2D;

	public float joyWidth = 350;
	public float joyHeight = 350;

	public Vector2 fingerSize = new Vector2 (100, 100);

	private float size;
	private GUITexture joyStickTexture;
	private GUITexture backObj;

	private RightBack rb;
	private RightFinger rf;

	public Vector2 ShootDir = Vector2.zero;

	public float InnerRadius = 80;
	public float ExternalRadius = 130;
	public float CancelRadius = 50;


	public enum RightState
	{
		Idle,
		Move,
	}

	private RightState state = RightState.Idle;

	public static RightController Instance;

    public bool CanProcessEvent;

    void Awake ()
	{
		Instance = this;
		if (Screen.width > Screen.height) {
			size = Screen.height;
		} else {
			size = Screen.width;
		}

		joyStickTexture = gameObject.AddComponent<GUITexture> ();
		joyStickTexture.texture = joyStick;
		joyStickTexture.color = inactiveColor;

		var bo = new GameObject ("RightBack");
        bo.transform.localScale = Vector3.zero;
		backObj = bo.gameObject.AddComponent<GUITexture> ();
		backObj.texture = background2D;
		backObj.color = inactiveColor;

		rb = gameObject.AddComponent<RightBack> ();
		rb.con = this;
		rb.tex = backObj;

		rf = gameObject.AddComponent<RightFinger> ();
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.zero;
		rf.con = this;
		rf.tex = joyStickTexture;

	}

	public Vector2 GetRealSize ()
	{
		var rate = Mathf.Min (Screen.width / 1024.0f, Screen.height / 768.0f);
		var v = new Vector2 (joyWidth, joyHeight) * rate;
		return v;
	}

	public Vector2 GetFingerSize ()
	{
		var rate = Mathf.Min (Screen.width / 1024.0f, Screen.height / 768.0f);
		var v = fingerSize * rate;
		return v;
	}

	public static float GetRate ()
	{
		var rate = Mathf.Min (Screen.width / 1024.0f, Screen.height / 768.0f);
		return rate;
	}

	private bool useMouse = false;

	private int fingerId = -1;

    private float enterTime = 0;
    private void Update()
    {
        if (!CanProcessEvent)
            return;

        if (state == RightState.Idle)
        {
            if (WindowMng.windowMng.IsOtherUIOpen())
            {
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = Input.mousePosition;
                if (mousePos.x > Screen.width/2)
                {
                    state = RightState.Move;
                    rb.EnterMove();
                    rf.EnterMove();
                    useMouse = true;

                    rb.SetFingerPos(mousePos);
                    rf.SetFingerPos(mousePos);

                    CalculateShootDir();
                    //MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.EnterShoot);
                    enterTime = Time.time;
                }
            }
            else 
#endif
            {
                foreach (var touch in Input.touches)
                {
                    fingerId = touch.fingerId;
                    //if (fingerId > 0 && fingerId < Input.touchCount)
                    if(touch.phase == TouchPhase.Began)
                    {
                        if (Input.GetTouch(fingerId).position.x > Screen.width/2)
                        {
                            state = RightState.Move;

                            var fp = Input.GetTouch(fingerId).position;
                            rb.EnterMove();
                            rf.EnterMove();
                            useMouse = false;
                            rb.SetFingerPos(fp);
                            rf.SetFingerPos(fp);
                            CalculateShootDir();
                            //MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.EnterShoot);
                            enterTime = Time.time;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (Time.time - enterTime > 0.0f && enterTime != 0)
            {
                MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.EnterShoot);
                enterTime = 0;
            }

            if (WindowMng.windowMng.IsOtherUIOpen())
            {
                state = RightState.Idle;
                rb.ExitMove();
                rf.ExitMove();
                useMouse = false;
                MyEventSystem.PushEventStatic(MyEvent.EventType.ExitShoot);
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (useMouse)
            {
                if (!Input.GetMouseButton(0))
                {
                    state = RightState.Idle;
                    rb.ExitMove();
                    rf.ExitMove();
                    useMouse = false;
                    var exit = new MyEvent()
                    {
                        type = MyEvent.EventType.ExitShoot,
                        boolArg =  !rb.IsCancel(),
                    };

                    MyEventSystem.myEventSystem.PushEvent(exit);
                    /*
                    if (!rb.IsCancel())
                    {
                        //MyEventSystem.PushEventStatic (MyEvent.EventType.Shoot);
                        GameInterface.gameInterface.PlayerAttack();
                    }
                     */ 

                }
                else
                {
                    var mousePos = Input.mousePosition;
                    rb.SetFingerPos(mousePos);
                    rf.SetFingerPos(mousePos);
                    CalculateShootDir();
                }
            }
            else 
#endif
            {
                var find = false;
                Touch touch = new Touch();
                var getTouch = false;
                foreach (var t in Input.touches)
                {
                    if (t.fingerId == fingerId)
                    {
                        touch = t;
                        getTouch = true;
                        break;
                    }
                }

                if (getTouch)
                {
                    var phase = touch.phase;
                    find = phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
                }
                else
                {
                    find = true;
                }
                if (find)
                {
                    state = RightState.Idle;
                    rb.ExitMove();
                    rf.ExitMove();

                    var exit = new MyEvent()
                    {
                        type = MyEvent.EventType.ExitShoot,
                        boolArg =  !rb.IsCancel(),
                    };
                    MyEventSystem.myEventSystem.PushEvent(exit);
                    /*
                    if (!rb.IsCancel())
                    {
                        GameInterface.gameInterface.PlayerAttack();
                    }
                     */ 
                }
                else
                {
                    var pos = touch.position;
                    rb.SetFingerPos(pos);
                    rf.SetFingerPos(pos);
                    CalculateShootDir();
                }
            }
        }
    }

    private void CalculateShootDir ()
	{
		var dir = rf.GetPos () - rb.GetPos ();
		ShootDir = dir.normalized;

        if (state == RightState.Move)
        {
            MyEventSystem.myEventSystem.PushEvent(new MyEvent()
            {
                type = MyEvent.EventType.ShootDir,
                vec2 = ShootDir,
            });
        }
	}

    public void ResetPos()
    {
        state = RightState.Idle;
        rb.ExitMove();
        rf.ExitMove();
        useMouse = false;
        var exit = new MyEvent()
        {
            type = MyEvent.EventType.ExitShoot,
            boolArg = !rb.IsCancel(),
        };
        MyEventSystem.myEventSystem.PushEvent(exit);
        /*
        if (!rb.IsCancel())
        {
            GameInterface.gameInterface.PlayerAttack();
        }
        */
    }
}

public class RightControllerProxy : MonoBehaviour
{
    private HashSet<UICamera.MouseOrTouch> m_TouchEvents = new HashSet<UICamera.MouseOrTouch>();

    void Awake()
    {
        m_TouchEvents = new HashSet<UICamera.MouseOrTouch>();
    }

    private void OnPress(bool isPress)
    {
        if (isPress)
        {
            if (!m_TouchEvents.Contains(UICamera.currentTouch))
            {
                m_TouchEvents.Add(UICamera.currentTouch);
            }

            RightController.Instance.CanProcessEvent = true;
        }
        else
        {
            if (m_TouchEvents.Contains(UICamera.currentTouch))
            {
                m_TouchEvents.Remove(UICamera.currentTouch);
            }

            if (m_TouchEvents.Count == 0)
            {
                RightController.Instance.CanProcessEvent = false;
                RightController.Instance.ResetPos();
            }
        }
    }
}