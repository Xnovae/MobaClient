using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//SceneId ---> ConfigData List
namespace MyLib
{
    public class EnvConfig {
        public string waterBottom;
        public string waterFace;
        public float offY = 0;
        public string skyBox;

        public bool useFog = false;
        public Color fogColor;
        public float fogStart;
        public float fogEnd;
        public FogMode fogMode;
        public float fogDensity;
        public float cameraDist = 200;

        public float lightCoff = 3;
        public Vector3  ambient = new Vector3(0.6f, 0.6f, 0.6f);

        public bool hasRain = false;
        public Vector3 rainAmbient = new Vector3(0.3f, 0.3f, 0.3f);
        public float rainLightCoff = 3;

        public bool hasLightning = false;

        public string envParticle;
    }
    public class MobaLevelConfigData
    {
        static bool initYet = false;
        public static Dictionary<int, List<LevelConfig>> LevelLayout = new Dictionary<int, List<LevelConfig>>();
        public static Dictionary<string, EnvConfig> envConfig = new Dictionary<string, EnvConfig>();

        public static void Init()
        {
            if (initYet)
            {
                return;
            }
            initYet = true;
            List<LevelConfig> l1;

            l1 = new List<LevelConfig>(){
                new LevelConfig("ENTRANCE_CITY", 0, 0){useOtherZone=true, zoneId=77, type="tank"},
            };
            LevelLayout.Add((int)LevelDefine.Hall, l1);

            l1 = new List<LevelConfig>(){
                new LevelConfig("ENTRANCE_CITY", 0, 0){useOtherZone=true, zoneId=77, type="tank"},
            };
            LevelLayout.Add((int)LevelDefine.Prepare, l1);

            l1 = new List<LevelConfig>(){
                new LevelConfig("", 0, 0){useOtherZone = true, zoneId = 103, type = "tank"},
            };
            LevelLayout.Add((int)LevelDefine.Battle, l1);

            l1 = new List<LevelConfig>()
            {
                new LevelConfig("ENTRANCE_CITY", 0, 0){useOtherZone=true, zoneId=77, type="tank"},
            };
            LevelLayout.Add((int)LevelDefine.SelectHero, l1);

            envConfig.Add("mine", new EnvConfig() {
                useFog = true,

                fogColor = new Color(0/255.0f, 0/255.0f, 0/255.0f, 1),
                fogStart = 24,
                fogEnd = 45,
                fogMode = FogMode.ExponentialSquared,
                fogDensity = 0.02f,
                cameraDist = 200,
                //hasRain = true,
            });

            envConfig.Add("suntemple", new EnvConfig(){
                waterBottom = "skyboxes/stemple_lake_light",
                waterFace = "skyboxes/stemple_water",
                offY = 0,

                useFog = true,
                fogColor = new Color(15/255.0f, 59/255.0f, 52/255.0f, 1),
                fogStart = 35,
                fogEnd = 60,
                fogMode = FogMode.ExponentialSquared,
                fogDensity = 0.02f,
                cameraDist = 200,
                lightCoff = 5,
                ambient = new Vector3(0.7f, 0.7f, 0.7f),
            });

            envConfig.Add("lava", new EnvConfig(){
                waterFace = "skyboxes/lava",
                skyBox = "skyboxes/crypt_sky",

                useFog = true,
                fogColor = new Color(255/255.0f, 111/255.0f, 31/255.0f, 1),
                fogStart = 40,
                fogEnd = 55,
                fogMode = FogMode.ExponentialSquared,
                fogDensity = 0.02f,
                cameraDist = 200,
            });

            envConfig.Add("town", new EnvConfig() {
                skyBox = "skyboxes/town_sky",
                useFog = true,
                fogColor = new Color(39/255.0f, 104/255.0f, 158/255.0f, 1),
                fogStart = 64,
                fogEnd = 100,
                fogMode = FogMode.ExponentialSquared,
                fogDensity = 0.01f,
                cameraDist = 200,
                hasRain = false,
                hasLightning = false ,
            });

            envConfig.Add("tank", new EnvConfig()
            {
                useFog = true,
                fogColor =  new Color(124/255.0f, 124/255.0f, 124/255.0f, 1),
                fogStart = 120,
                fogEnd = 200,
                fogMode = FogMode.Linear,
                fogDensity = 1,
                //envParticle = "particles/atmosphere/mine_dust",
                cameraDist = 200,
            });

        }
    }
}