using UnityEngine;
using System.Collections;

public class GameConst : MonoBehaviour
{
    public Color teamRed = new Color(0.97f, 0.04f, 0.22f);
    public Color teamBlue = new Color(0.04f, 0.12f, 0.97f);

    public int TEAM_RED = 0;
    public int TEAM_BLUE = 1;

    public float MaxRotateChange = 3.0f;
    public float TowerMaxRotateChange = 3.0f;
    public float MoveSpeed = 8.0f;
    public float BulletSpeed = 20.0f;


    public float SectorDegree = 10;
    public float SectorDistance = 20;
    public float SectorLength = 15;

    public int AssistDamage = 50;

    public int StaticShootBuffShowDelay = 0;
    //public float StaticShootBuffContinue = 2;
    public float StaticShootBuffDamageRatio = 2;

    public static GameConst Instance;

    public int NuclearWaitTime = 5;
    public int NuclearWorkTime = 3;

    public int NewMatchLevel = 3;

    public float KnockBackSpeed = 10;

    public int SpeakWaitTime = 10;
    public int TestMap = 0;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
