using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public abstract class MeSyncToServer : MonoBehaviour
    {
        /// <summary>
        /// 本地初始化自己的AvatarInfo
        /// </summary>
        /// <param name="info"></param>
        public abstract void InitData(AvatarInfo info);
        /// <summary>
        /// 同步位置信息给服务器
        /// </summary>
        public abstract void SyncPos();
        /// <summary>
        /// 同步非位置属性给服务器
        /// </summary>
        public abstract void SyncAttribute();
    }
}
