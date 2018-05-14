using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

using System.IO;

namespace MyLib
{
    /// <summary>
    /// 将关卡的Entity通过导出给服务器由服务器控制Entity的销毁和生成
    /// Master 退出游戏不会删除Entity了
    /// 
    /// {
    /// Descriptor: "GameObject"
    /// Name:"level_1_4_74"
    /// child:[
    ///     {
    ///         Descriptor: "GameObject"
    ///         Name:"properties"
    ///         child:[
    ///             {
    ///             Descriptor: "GameObject"
    ///             Name: "NotKill",
    ///             Component:[
    ///                 {
    ///                     Name: "SpawnChest",
    ///                     SpawnId: 0,
    ///                     RateToSpawn:
    ///                     Pos: "<vec>a,b,c" 
    ///                 },
    ///                 {
    ///                       
    ///                 },
    ///             ]        
    ///             Child: 
    ///             [
    ///             ]
    ///             
    /// 
    ///             }
    ///         ]
    ///     }
    /// ]
    /// }
    /// 
    /// SpawnChest 服务器版本
    /// 
    /// 导出的类型需要是 MyLib 命名空间下的直接类型
    /// </summary>
    public class EntityConfigExport : MonoBehaviour
    {
        [ButtonCallFunc()]
        public bool Export;

        public void ExportMethod()
        {
            spawnId = 0;
            var jobj = ExportGameObject(this.gameObject);
            //var etyPath = Path.Combine(Application.dataPath, "../../tankServer/SocketServer/bin/Debug");
            var etyPath = Path.Combine(Application.dataPath, "../../tankServer/SocketServer/ConfigData");

            var file = Path.Combine(etyPath, this.gameObject.name + ".json");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            var s = jobj.ToString();
           
            File.WriteAllText(file, s);
            Debug.LogError("导出配置: " + file);
        }

        public static JSONClass ExportGo(GameObject go)
        {
            spawnId = 0;
            var jobj = ExportGameObject(go);
            return jobj;
        }

        private static JSONClass ExportGameObject(GameObject go)
        {
            var jobj = new JSONClass();
            jobj.Add("Descriptor", "GameObject");
            jobj.Add("Name", go.name);
            jobj.Add("InstId", new JSONData(go.GetInstanceID()));
            var pos = Util.ConvertPos(go.transform.position);
            jobj.Add("Pos", "<vec>" + pos[0] + "," + pos[1] + "," + pos[2]);
            jobj.Add("Scale", ExportVec(go.transform.localScale));

            var jarr = new JSONArray();
            jobj.Add("Component", jarr);
            var com = go.GetComponents<MonoBehaviour>();
            foreach (var monoBehaviour in com)
            {
                if (monoBehaviour != null)
                {
                    jarr.Add(ExportComponent(monoBehaviour));
                }
            }
            var col = go.GetComponents<Collider>();
            foreach (var collider1 in col)
            {
                jarr.Add(ExportCollider(collider1));
            }

            var jarr2 = new JSONArray();
            jobj.Add("Child", jarr2);
            foreach (Transform c in go.transform)
            {
                if (c.gameObject.activeSelf)
                {
                    jarr2.Add(ExportGameObject(c.gameObject));
                }
            }
            return jobj;
        }

        private static string ExportVec(Vector3 vec)
        {
            var i = Util.ConvertPos(vec);
            return "<vec>" + i[0] + "," + i[1] + "," + i[2];
        }

        private static JSONArray ExportValueArray(List<object> values)
        {
            var ret = new JSONArray();
            foreach(var v in values)
            {
                var vv = ExportStruct(v);
                ret.Add(vv);
            }
            return ret;
        }

        private static JSONArray ExportVecArr(List<Vector3> vec)
        {
            var ret = new JSONArray();
            foreach (var v in vec)
            {
                var vv = ExportVec(v);
                ret.Add(vv);
            }
            return ret;
        }

        private static JSONClass ExportCollider(Collider col)
        {
            var jobj = new JSONClass();
            jobj.Add("Descriptor", "Class");
            jobj.Add("Type", col.GetType().Name);
            jobj.Add("Layer", ((GameLayer)col.gameObject.layer).ToString());
            if (col.GetType() == typeof(BoxCollider))
            {
                var bc = col as BoxCollider;
                var ap = bc.transform.position;
                var ascale = bc.transform.localScale;
                var ar = bc.transform.rotation;

                var realPos = ap + ar * bc.center;
                var realSize = ar * new Vector3(ascale.x * bc.size.x, ascale.y * bc.size.y, ascale.z * bc.size.z);
                jobj.Add("Center", ExportVec(realPos));
                jobj.Add("Size", ExportVec(realSize));
            }
            else
            {
                Debug.LogError("NotSupport Collider: " + col.gameObject.name + " col " + col.GetType().Name);
            }
            return jobj;
        }

        private static int spawnId = 0;
        private static JSONClass ExportComponent(MonoBehaviour behaviour)
        {
            var jobj = new JSONClass();
            jobj.Add("Descriptor", "Class");
            jobj.Add("Type", behaviour.GetType().Name);

            var pro = behaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var p in pro)
            {
                if (p.Name == "SpawnId")
                {
                    jobj.Add(p.Name, new JSONData(++spawnId));
                    p.SetValue(behaviour, spawnId);
                }
                else
                {
                    var v = p.GetValue(behaviour);
                    jobj.Add(p.Name, ExportValue(v));
                }
            }
            return jobj;
        }

        /// <summary>
        /// 导出结构体 或者 Class类
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static JSONClass ExportStruct(object obj)
        {
            var jobj = new JSONClass();
            jobj.Add("Descriptor", "Class");
            jobj.Add("Type", obj.GetType().Name);

            var pro = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            foreach (var p in pro)
            {
                var v = p.GetValue(obj);
                jobj.Add(p.Name, ExportValue(v));
            }
            return jobj;
        }

        private static JSONNode ExportValue(object v)
        {
            if (v == null)
            {
                return new JSONData("null");
            }
            var vt = v.GetType();
            if (vt == typeof(string))
            {
                return new JSONData(v as string);
            }
            else if (vt == typeof(int))
            {
                return new JSONData(System.Convert.ToInt32(v));
            }
            else if (vt == typeof(float))
            {
                return new JSONData(System.Convert.ToSingle(v));
            }
            else if (vt == typeof(bool))
            {
                return new JSONData(System.Convert.ToBoolean(v));
            }else if (vt.IsEnum)
            {
                return new JSONData(System.Convert.ToInt32(v));//整数枚举类型比较好存储一些 注意不能修改枚举的顺序 
            }else if(vt == typeof(GameObject))
            {
                return "<go>" +(v as GameObject).GetInstanceID(); //通过InstanceID 寻找GameObject
            }
            else if (vt.IsGenericType)
            {
                var typeDef = vt.GetGenericTypeDefinition();
                var arg = vt.GetGenericArguments();
                if (typeDef == typeof(List<>))
                {
                    if (arg.Length > 0)
                    {
                        var p0 = arg[0];
                        if (p0 == typeof(Vector3))
                        {
                            return ExportVecArr((v as IEnumerable).Cast<Vector3>().ToList());
                        }else if(p0.IsValueType && !vt.IsPrimitive) //近似判断是否是 struct结构体, primitive 条件只能排除int long 不能排除其它value类型
                        {
                            return ExportValueArray((v as IEnumerable).Cast<object>().ToList());
                        }else if(p0.IsClass)
                        {
                            return ExportValueArray((v as IEnumerable).Cast<object>().ToList());
                        }
                    }
                }
            }
            else
            {
                return new JSONData("Unknown:" + v.ToString());
            }

            return new JSONData("Unknown:" + v.ToString());
        }

    }
}
