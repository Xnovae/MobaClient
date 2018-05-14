/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyLib
{
    public class ObjectCommand
    {
        public enum ENUM_OBJECT_COMMAND
        {
            INVALID = -1,
            OC_MOVE,
            OC_UPDATE_IMPACT,
            //Buff Update
            OC_USE_SKILL,
            OC_SUPER_MOVE,
        }

        public int skillId;
        public int skillLevel;

        public ENUM_OBJECT_COMMAND commandID;
        public int logicCount;
        public uint startTime;

        public Vector3 targetPos;
        public int dir;
        //public float speed;

        public bool staticShoot;

        public GCPushUnitAddBuffer buffInfo;
        public bool UseDir = true;
        public bool UseY = true;

        public SkillAction skillAction;
        /// <summary>
        /// 命令执行的时间 给技能使用
        /// </summary>
        public int runFrame = 0;
        public GCPlayerCmd proto;

        public ObjectCommand()
        {
        }


        public ObjectCommand(ENUM_OBJECT_COMMAND cmd)
        {
            commandID = cmd;
        }
    }
    public class MoveCMD : ObjectCommand
    {
        public MoveCMD()
        {
            commandID = ENUM_OBJECT_COMMAND.OC_MOVE;
        }
        public void CalRunTime(Vector3 curPos, NpcAttribute attr)
        {
            var runTime = (targetPos - curPos).magnitude / attr.GetSpeed();
            runFrame = Util.FloatToFrame(runTime);
        }
    }

    public class SkillCMD : ObjectCommand
    {
        public SkillCMD()
        {
            commandID = ENUM_OBJECT_COMMAND.OC_USE_SKILL;
        }
    }


    /// <summary>
    /// 执行网络命令在特定的玩家 或者 怪兽身上
    /// 将网络命令转化成本地命令 在对象身上执行
    /// 每次执行1个命令 推送给状态机
    /// 命令执行完成 执行下一个命令
    /// 
    /// 保证命令按照顺序执行 Move命令执行完才执行Attack状态同步
    /// </summary>
    public class LogicCommand : MonoBehaviour
    {
        public void PushCommand(ObjectCommand cmd)
        {
            Log.Important("Push Command What " + cmd.commandID+" target "+cmd.targetPos+" dir "+cmd.dir);
            /*
            if (commandList.Count == 0 && state == State.Idle)
            {
                DoLogicCommand(cmd);
            }
            else
            {
                commandList.Add(cmd);
            }
            */
            commandList.Add(cmd);
        }


        /// <summary>
        /// 只确保在某一帧执行某个命令
        /// 不保证命令状态本身的完整性
        /// linear 插值
        /// step
        /// pulse
        /// </summary>
        /// <param name="cmd"></param>
        void DoLogicCommand(ObjectCommand cmd)
        {
            Log.AI("DoLogicCommad " + cmd.commandID);
            state = State.InCommand;
            currentCmd = cmd;
            switch (cmd.commandID)
            {
                case ObjectCommand.ENUM_OBJECT_COMMAND.OC_MOVE:
                    //简单直线移动
                    RunMoveCmd(cmd as MoveCMD);
                    break;
                case ObjectCommand.ENUM_OBJECT_COMMAND.OC_USE_SKILL:
                    EnterUseSkill(cmd as SkillCMD);
                    break;
                default:
                    state = State.Idle;
                    Debug.LogError("LogicCommand:: UnImplement Command " + cmd.commandID.ToString());
                    break;
            }
            //waitTime = currentCmd.runFrame * 1.0f / Util.FramePerSecond;
            waitTime = Util.FrameToFloat(currentCmd.runFrame);
        }
        /*
        private void Update()
        {
            if(state == State.InCommand)
            {
                waitTime -= Time.deltaTime;
                if(waitTime <= 0)
                {
                    state = State.Idle;
                }
            }  
        }
        */
        void EnterUseSkill(SkillCMD cmd)
        {
            if (cmd != null)
            {
                aiBase.GetAI().OnCommand(cmd);
            }
        }
        /// <summary>
        /// 移动命令执行的时间长度
        /// 服务器发送当前位置 以一定速度 向某个方向移动 的命令
        /// </summary>
        /// <param name="cmd"></param>
        private void RunMoveCmd(MoveCMD cmd)
        {
            cmd.CalRunTime(transform.position, attribute);
            mvController.MoveTo(cmd);
            aiBase.GetAI().OnCommand(cmd);
        }


        /*
		 * 执行当前的移动命令
         *  若上一个命令是移动命令则替换
         *  若上一个命令是技能命令则等待
         *  
         * 同时执行移动和技能命令
         * 技能命令不会打断移动命令
         执行命令直到命令结束才能执行下一个
         如何知道一个命令执行结束了？
		 */
        IEnumerator CommandHandle()
        {
            var runningCmd = new List<ObjectCommand>();
            while (true)
            {
                /*
                if (commandList.Count > 0 && state == State.Idle)
                {
                    var peekCmd = commandList [0];
                    commandList.RemoveAt(0);
                    DoLogicCommand(peekCmd);
                }
                */
                if (NetworkScene.Instance != null && commandList.Count > 0)
                {
                    var serverFrame = NetworkScene.Instance.GetPredictServerFrame();
                    foreach (var c in commandList)
                    {
                        if (c.proto.HasRunInFrame)
                        {
                            if (c.proto.RunInFrame < serverFrame)
                            {
                                DoLogicCommand(c);
                                runningCmd.Add(c);
                            }
                        }else
                        {
                            DoLogicCommand(c);
                            runningCmd.Add(c);
                        }
                    }

                    foreach (var r in runningCmd)
                    {
                        commandList.Remove(r);
                    }
                    runningCmd.Clear();
                }
                yield return null;
            }
        }

        void Start()
        {
            attribute = GetComponent<NpcAttribute>();
            mvController = GetComponent<MoveController>();
            aiBase = GetComponent<AIBase>();
            state = State.Idle;
            StartCoroutine(CommandHandle());
        }

        private NpcAttribute attribute;
        private MoveController mvController;
        private List<ObjectCommand> commandList = new List<ObjectCommand>();
        private ObjectCommand currentMoveCommand = null;
        private enum State
        {
            NotInit,
            Idle,
            InCommand,
        }
        private State state = State.NotInit;
        private AIBase aiBase;
        private ObjectCommand currentCmd;
        private float waitTime = 0;

    }
}
