
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

namespace KBEngine
{
    
    /*
	 * Player ---> Multiple View
	 * ViewID ---> Owner Player Owner ID
	 */
    public class KBNetworkView : KBMonoBehaviour
    {
        /*
		 * Client Object ID
		 */
        //Awake的初始化在 私有变量 int 赋值之后？还是之前Public 遗留的问题？
        static int LocalID = 0;

        public void SetLocalId(int lid)
        {
            localId = lid;
        }


        int localId = -1;
        int serverId = -1;

        public int GetLocalId()
        {
            if (localId == -1)
            {
                localId = LocalID++;
            }
            return localId;
        }

        /// <summary>
        /// 玩家则返回玩家Id
        /// 怪物则返回ViewId 
        /// </summary>
        /// <returns>The server I.</returns>
        public int GetServerID()
        {
            return serverId;
        }

        public void SetServerID(int sId)
        {
            serverId = sId;
        }

        /// <summary>
        /// 是玩家还是怪物或者宠物等对象
        /// </summary>
        public bool IsPlayer = true;

        public bool IsMe
        {
            get
            {
                return localId == MyLib.ObjectManager.objectManager.GetMyLocalId(); 
            }
        }

        //是否是我可以控制的对象
        //网络状态下 和 普通状态下
        //网络状态下 == myPlayer 如果是怪物 则还需要我是Master 才行
        public bool IsMine
        {
            get
            {
                if(serverId == -1)
                {
                    Debug.Log("KBNetworkView:: No NetworkConnect Init Player Is Mine");
                    return true;
                }

                var isMe = serverId == MyLib.ObjectManager.objectManager.myPlayerServerId;
                return isMe;
            }
        }

        void Start()
        {
            //localId = LocalID++;
            Log.Sys("Implement Local ID " + localId + " " + gameObject.name);
        }

        void Awake()
        {
            //localId = LocalID++;
        }

    }

}