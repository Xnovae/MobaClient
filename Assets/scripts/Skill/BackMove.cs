using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    /// <summary>
    /// 一个技能配置Buff 驱动玩家移动
    /// 如何和其它玩家冲突怎么办？
    /// 并不影响动作
    /// </summary>
    public class BackMove : MonoBehaviour
    {
        //向后移动 距离和时间
        public float moveStep = -10;
        public float moveTime = 1;

        //需要同步服务器上的状态
        //可以跳过障碍物
        //服务器计算实际的跳跃结果
        //技能释放的时候就需要提前计算好跳跃位置 以方便后面的同步
        //向玩家添加Buff 服务器同步Buff给玩家
        private SkillLayoutRunner runner;
        private void Start()
        {
            runner = transform.parent.gameObject.GetComponent<SkillLayoutRunner>();
            MoveBack();
        }

        /// <summary>
        /// 添加Buff限制状态
        /// Skill 状态维持
        /// </summary>
        /// <returns></returns>
        private void MoveBack()
        {
            //从当前位置计算 合理的击退位置
            var cmd = runner.stateMachine.cmd;
            var pos = new MyVec3(cmd.SkillAction.X, cmd.SkillAction.Y, cmd.SkillAction.Z);
            var fpos = pos.ToFloat();
            var dir = cmd.SkillAction.Dir;
            var mapGrid = GridManager.Instance;

            var moveTotal = moveStep * moveTime;
            var endPos = fpos + Quaternion.Euler(0, dir, 0) * Vector3.forward * moveTotal;
            //得到击退的位置
            var newPos = mapGrid.RaycastNearestPoint(fpos, endPos);
            //逻辑上可以直接一段时间设置位置 表现上可能需要做平移

            //移动到位置
            var att = runner.stateMachine.attacker;
            var phy = att.GetComponent<IPhysicCom>();
            phy.MoveToIgnorePhysic(newPos);
        }
    }
}
