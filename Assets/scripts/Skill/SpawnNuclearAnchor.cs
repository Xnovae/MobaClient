using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class SpawnNuclearAnchor : MonoBehaviour
    {
        public int MonsterID;

        private void Start()
        {
            var runner = transform.parent.GetComponent<SkillLayoutRunner>();
            var isMe = runner.stateMachine.attacker.GetComponent<NpcAttribute>().GetNetView().IsMe;
            if (isMe)
            {
                var unitData = Util.GetUnitData(false, MonsterID, 0);
                var cg = CGPlayerCmd.CreateBuilder();
                cg.Cmd = "AddEntity";

                var entityInfo = EntityInfo.CreateBuilder();
                entityInfo.UnitId = MonsterID;
                var ip = NetworkUtil.ConvertPos(this.transform.position);
                entityInfo.X = ip[0];
                entityInfo.Y = ip[1] + 50;
                entityInfo.Z = ip[2];
                entityInfo.SpawnId = 0;
                entityInfo.EType = EntityType.CHEST;
                entityInfo.HP = unitData.HP;
                entityInfo.LifeLeft = GameConst.Instance.NuclearWaitTime+GameConst.Instance.NuclearWorkTime;
                entityInfo.PlayerID =
                    runner.stateMachine.attacker.GetComponent<NpcAttribute>().GetNetView().GetServerID();
                cg.EntityInfo = entityInfo.Build();
                var scene = WorldManager.worldManager.GetActive();
                scene.BroadcastMsg(cg);
            }
        }
    }
}
