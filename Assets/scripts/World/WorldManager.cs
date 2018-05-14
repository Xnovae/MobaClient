using UnityEngine;
using System.Collections;

namespace MyLib
{
	public class WorldManager : MonoBehaviour
	{
		public static WorldManager worldManager;
		public enum WorldStation {
			NotEnter,  //没有进入任何场景
			Entering,  //正在进入一个场景
			Enter,   //成功进入一个场景
			Relive,   //死亡复活
			AskChangeScene,  //请求进入一个场景
		}

		public enum SceneType
		{
			City, //城市中可以看到其它玩家
			InCopy, //单人副本只有玩家自己不用同步移动数据等战斗数据
		}
		public SceneType sceneType{
            get {
                if(activeScene != null){
                    if(activeScene.IsCity){
                        return SceneType.City;
                    }
                }
                return SceneType.InCopy;
            }
        }

		public WorldStation station = WorldStation.NotEnter;
		LoadingUI loadUI;
		int nextSceneId;

		CScene activeScene = null;
		public CScene GetActive() {
			return activeScene;
		}

	    private void ClearScene()
	    {
	        if (activeScene != null)
	        {
                activeScene.LeaveScene();
                GameObject.Destroy(activeScene.gameObject);
                activeScene = null;
            }
	    }

	    private IEnumerator QuitToLogin()
	    {
	        if (this.activeScene != null && !this.activeScene.IsCity)
	        {

	            this.WorldChangeScene(2, false);
	            yield return new WaitForSeconds(2);
	        }
	        ClearScene();
	        Application.LoadLevel("MainLogin");

	        if (MainLoginUI.Instance != null)
	        {
	            MainLoginUI.Instance.SetAccountName(PlatformSdkManager.Instance.UserName);
	        }
	    }

	    public void QuitWorldToLoginScene()
	    {
	        StartCoroutine(QuitToLogin());
	    }

		void Awake ()
		{
			worldManager = this;
			DontDestroyOnLoad (this.gameObject);
            gameObject.AddComponent<ActiveNearCamera>();
		}

		// Use this for initialization
		void Start ()
		{
		
		}
		
		// Update is called once per frame
		void Update ()
		{
		
		}
		public bool IsPeaceLevel() {
			return sceneType == SceneType.City;
		}

	    public static DungeonConfigData NextScene;
		//执行进入场景的代码逻辑
		IEnumerator EnterScene(GCEnterScene sceneData) {
            Log.Sys("EnterNextScene is "+nextSceneId);

			var sdata = CopyController.copyController.GetLevelInfo (nextSceneId);
		    NextScene = sdata;
			//删除旧的场景中的玩家数据
			if (activeScene != null) {
				activeScene.LeaveScene();
                GameObject.Destroy(activeScene.gameObject);
			}

            activeScene = CScene.CreateScene(sdata);
			activeScene.Init ();
			activeScene.EnterScene ();

			//Init Camera
			Log.Sys("Load Scene Name is "+sdata.SceneName);

			//等待加载静态场景资源
			AsyncOperation async = Application.LoadLevelAsync (sdata.SceneName);
			loadUI.async = async;
			loadUI.ShowLoad ("等待");
			while (!async.isDone) {
				yield return null;
			}
            //InitCamera After Load Scene
            //Scene Load Finish Then Load MainCamera

            if (CameraController.cameraController == null) {
                Log.Sys("CreateMainCamera");
                var mc = Resources.Load<GameObject> ("levelPublic/MainCamera");
                Instantiate (mc);//  as GameObject;

                var lm = Resources.Load<GameObject>("LightCamera");
                var lm2 = Instantiate (lm) as GameObject;
                lm2.GetComponent<Camera>().orthographicSize = GraphInit.Instance.lightShadowCameraSize;

                var sc = Instantiate(Resources.Load<GameObject>("levelPublic/ShadowCamera")) as GameObject;
                sc.GetComponent<Camera>().orthographicSize = GraphInit.Instance.lightShadowCameraSize;
            }


            if(BattleManager.battleManager == null) {
                var g = new GameObject("BattleManager");
                g.AddComponent<BattleManager>();
            }
            {
                var g = new GameObject("NpcManager"); 
                g.AddComponent<NpcManager>();
            }
            activeScene.ManagerInitOver();

            Log.Sys("Cameramain "+Camera.main);

			MyEventSystem.myEventSystem.PushEvent (MyEvent.EventType.EnterScene);
			//正在进入一个场景
			yield return null;

            var g1 = new GameObject("StreamLoadLevel");
            var loader = g1.AddComponent<StreamLoadLevel>();
            var water = g1.AddComponent<WaterEnvLoader>();
            var netLoader = g1.AddComponent<NetworkLoadZone>();
            yield return StartCoroutine(loader.LoadFirstRoom());

            var start = GameObject.Find("PlayerStart");
            CameraController.cameraController.TracePositon(start.transform.position);


			NetDebug.netDebug.AddConsole ("LoadScene Finish start Init UI");

			CreateUI ();

            NetDebug.netDebug.AddConsole ("CreateMyPlayer");
			//场景传送点
			CreateMyPlayer();
			//load Success 
			//loadUI.Hide (null);
			NetDebug.netDebug.AddConsole ("Init Player Over Next");
			station = WorldStation.Enter;

			//场景其它初始化交给LevelInit
			NetDebug.netDebug.AddConsole ("WorldManager:: InitLevel");
			CreateLevelInit ();
			//初始化缓存的场景玩家
			//ObjectManager.objectManager.InitCache ();
			NetDebug.netDebug.AddConsole ("Init World Finish");

            StartCoroutine(loader.LoadRoomNeibor());
		}



        /// <summary>
        /// 创建移动攻击等控制UI
        /// </summary>
		void CreateUI(){
			//初始化UIRoot
			UIPanel p = NGUITools.CreateUI (false, (int)GameLayer.UICamera);
			p.tag = "UIRoot";
			var root = p.GetComponent<UIRoot> ();
			root.scalingStyle = UIRoot.Scaling.ConstrainedOnMobiles;
			root.manualWidth = 1280;
			root.manualHeight = 720;
			root.fitWidth = true;
			root.fitHeight = true;
            root.transform.position = new Vector3(100, 100, 0);

            var leftController = Object.Instantiate(Resources.Load<GameObject>("levelPublic/LeftController")) as GameObject;
            //leftController.transform.position = new Vector3(0,0.12f,0);
               
            //var rightController = GameObject.Instantiate(Resources.Load<GameObject>("levelPublic/RightController")) as GameObject;
            //rightController.transform.position = new Vector3(0, 0.12f, 0);
        }

        /// <summary>
        /// 加载和场景匹配的游戏主UI
        /// 从网络初始化玩家数据,只在登陆进入游戏的时候需要，后续都是增量更新
        /// </summary>
		void CreateLevelInit() {

			Log.GUI ("Push Main UI ");
			var uiName = Util.GetUI ();
			WindowMng.windowMng.PushView (uiName, false, false);
			MyEventSystem.myEventSystem.PushEvent (MyEvent.EventType.UpdateMainUI);


			Log.Sys ("Init SaveGame And Data");
			if (SaveGame.saveGame == null) {
				//var saveGame = Instantiate (Resources.Load<GameObject> ("levelPublic/saveGame")) as GameObject;
				var g = new GameObject();
				var saveGame = g.AddComponent<SaveGame>();
				saveGame.GetComponent<SaveGame> ().InitData ();
			} else {
			}

			StartCoroutine(SaveGame.saveGame.InitDataFromNetwork());
		}

		//重新创建玩家自身
		void CreateMyPlayer() {

			GameObject player = null;
			if (sceneType == SceneType.InCopy) {
				player = ObjectManager.objectManager.CreateMyPlayerInCopy ();
			} else if (sceneType == SceneType.City) {
				player = ObjectManager.objectManager.CreateMyPlayerInCity ();
			}
			NetDebug.netDebug.AddConsole ("Create My Player Over");

			var evt = new MyEvent (MyEvent.EventType.PlayerEnterWorld);
			evt.player = player;
			MyEventSystem.myEventSystem.PushEvent (evt);

			NetDebug.netDebug.AddConsole ("PlayerEnterWorld Event");

            if(NetDebug.netDebug.IsWuDi){
                var aff = new Affix(){effectType=Affix.EffectType.DefenseAdd};
                aff.Duration = 9999;
                player.GetComponent<BuffComponent>().AddBuff(aff);
            }
			
            //关闭选择人物 界面等
		}

		public void WorldChangeScene(int sceneId, bool isRelive) {
            Log.Sys("WorldChangeScene:"+sceneId+" isRelive; "+isRelive);
            MobaLevelConfigData.Init();
            var hasData = MobaLevelConfigData.LevelLayout.ContainsKey(sceneId);
            if(!hasData){
                WindowMng.windowMng.ShowNotifyLog("关卡尚未开放");
                return;
            }

			StartCoroutine (ChangeScene(sceneId, isRelive));
		}

	    private AudioSource audios = null;
		//游戏过程中切换场景 向服务器请求场景切换
		public IEnumerator ChangeScene(int sceneId, bool isRelive) {

            Log.Sys("ChangeEnterNextScene "+sceneId);
			nextSceneId = sceneId;
            var sdata = CopyController.copyController.GetLevelInfo (nextSceneId);
            Log.Sys("backSound: "+sdata.background);
            BackgroundSound.Instance.PlaySound(sdata.background, sdata.volume/100.0f);
            GameObject.Destroy(audios);
		    if (!string.IsNullOrEmpty(sdata.noise))
		    {
		        audios = BackgroundSound.Instance.PlayEffectLoop(sdata.noise, sdata.noiseVolume/100.0f);
                audios.Play();
		    }

			station = isRelive ? WorldStation.Relive : WorldStation.AskChangeScene; 

			//先显示加载界面，首先加载网络资源接着加载静态资源
			loadUI = WindowMng.windowMng.PushView ("UI/loading").GetComponent<LoadingUI>();
			//再去清理UI 层深度信息
			MyEventSystem.myEventSystem.PushEvent (MyEvent.EventType.ChangeScene);

			Log.Net ("EnterScene Process");


            /*
			CGEnterScene.Builder es = CGEnterScene.CreateBuilder ();
			es.Id = sceneId;
			var packet = new KBEngine.PacketHolder ();
			yield return StartCoroutine (KBEngine.Bundle.sendSimple(this, es, packet));
            */
            yield return null;

            station = WorldStation.Entering;
            //yield return StartCoroutine(EnterScene(packet.packet.protoBody as GCEnterScene));
            yield return StartCoroutine(EnterScene(null));
            /*
			if (packet.packet.responseFlag == 0) {
			} else {
				Debug.LogError("ChangeScene Error ");
			}
            */
		}

	    public static void ReturnCity()
	    {
            WorldManager.worldManager.WorldChangeScene((int)LevelDefine.Hall, false);
	    }

	}
}
