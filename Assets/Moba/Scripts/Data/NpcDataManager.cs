using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace MyLib
{
    public class NpcDataManager
    {
        private Dictionary<int, NpcConfig> npcConfig;

        private static NpcDataManager _Instance;
        public static NpcDataManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new NpcDataManager();
                }
                return _Instance;
            }
        }

        public void Init()
        {
            var allSoldier = Resources.Load<GameObject>("AllSoldier");
            npcConfig = new Dictionary<int, NpcConfig>();
            foreach(Transform c in allSoldier.transform) {
                npcConfig.Add(c.gameObject.GetComponent<NpcConfig>().npcTemplateId, c.GetComponent<NpcConfig>());
            }

            /*
            using (var f = new StreamReader(string.Format("ConfigData/AllSoldier.json")))
            {
                var con = f.ReadToEnd();
                var jobj = JSON.Parse(con).AsObject;
                var entityConfig = EntityImport.InitGameObject(jobj);
                npcConfig = new Dictionary<int, NpcConfig>();
                var chd = entityConfig.GetChildren();
                //结构化配置
                foreach (var c in chd)
                {
                    npcConfig.Add(c.GetComponent<NpcConfig>().npcTemplateId, c.GetComponent<NpcConfig>());
                }
            }
            */

        }
        /// <summary>
        /// 类似于加载Level_1_4_103.json 
        /// 但是不要执行初始化只作为配置使用
        /// 转化为一个List结构
        /// </summary>
        private NpcDataManager()
        {
        }
        public NpcConfig GetConfig(int id)
        {
            if (npcConfig.ContainsKey(id))
            {
                var al = npcConfig[id];
                return al;
            }
            return null;
        }
    }
}