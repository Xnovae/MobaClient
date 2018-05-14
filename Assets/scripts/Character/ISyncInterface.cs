using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 所有从服务器到客户端的同步接口
    /// </summary>
    public abstract class ISyncInterface : MonoBehaviour
    {
        public abstract void InitSetPos(Vector3 pos);
        /// <summary>
        /// 初始化同步信息
        /// </summary>
        /// <param name="info"></param>
        public abstract void InitSync(AvatarInfo info);
        /// <summary>
        /// 服务器通知释放技能
        /// </summary>
        /// <param name="sk"></param>
        public abstract void NetworkAttack(GCPlayerCmd sk);
        public abstract void NetworkBuff(GCPlayerCmd gc);
        /// <summary>
        /// 服务器每帧同步属性
        /// </summary>
        /// <param name="gc"></param>
        public abstract void NetworkAttribute(GCPlayerCmd gc);
        public abstract void Revive(GCPlayerCmd gc);
        public abstract void SetLevel(AvatarInfo info);
        public abstract void SetPositionAndDir(AvatarInfo info);
        public abstract void DoNetworkDamage(GCPlayerCmd cmd);
        public abstract void NetworkRemoveBuff(GCPlayerCmd cmd);
        public abstract void Dead(GCPlayerCmd cmd);
        public abstract bool CheckSyncState();

        /// <summary>
        /// 获取服务器速度
        /// </summary>
        /// <returns></returns>
        public abstract MyVec3 GetServerVelocity();
        /// <summary>
        /// 获取服务器上的玩家位置
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetServerPos();

        public abstract Vector3 GetCurInfoPos();
        public abstract Vector3 GetCurInfoSpeed();
        public abstract void AddFakeMove();
    }
}
