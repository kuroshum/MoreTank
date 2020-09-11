using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateStage : MonoBehaviour {
    public GameObject gstage;
    public GameObject gwall;
    public GameObject gpoint;
    public GameObject gearth;
    public Vector3 pos;
    private Vector3 space = new Vector3(1.0f, 1.0f, 1.0f);
    private int iwidth;

    private AnaHori ah;
    private AnahoriDungeon ad;

    private int max;

    void Start () {
        /*
         * 初期座標の設定
         */
        pos = Vector3.zero;
    }

    private string LoadStage(string StageFile)
    {
        /*
         * テキストの中身(ステージマップ)を読み込んで変数に保存
         */
        ReadWrite rw = GameObject.Find("Astage").GetComponent<ReadWrite>();
        string textdata = rw.Read(StageFile);

        return textdata;
    }


    public void Create(Vector3 pos, int ROOM_NUM) {

        max = 50;

        ah = this.GetComponent<AnaHori>();
        ad = this.GetComponent<AnahoriDungeon>();


        iwidth = 0;
        /*
         * ステージマップが保存されているテキストのパス
         */
        //string StageFile = System.IO.Path.GetFileName(@"C:\Users\stage.txt");
        //string StageFile = Application.dataPath +  "/" + "Resources" + "/" + "stage2.txt";

        //var ws = ad.Generate(ROOM_NUM, max);
        string textdata = ad.Generate(ROOM_NUM, max);
        //string textdata = LoadStage(StageFile);
        GameObject obj = null;

        Vector3 init_pos;

        /*
         * 変数に保存したステージマップを走査する
         * #ならCubeを生成し、Cubeの大きさだけx軸に右に移動
         * 改行文字ならz軸に下に移動して、x軸を初期化
         * 空白、-、ならそのままx軸に右に移動
         */
        foreach (char c in textdata)
        {
            if (c == '#' || c == 'S' || c == 'b') {
                init_pos = new Vector3(pos.x, pos.y - 2, pos.z);

                obj = Instantiate(gstage, init_pos, Quaternion.identity, GameObject.Find("Astage").transform) as GameObject;
                obj.name = gstage.name;
                pos.x += obj.transform.lossyScale.x;
                iwidth++;
            }
            else if (c == '*')
            {
                init_pos = new Vector3(pos.x, pos.y - 2, pos.z);

                obj = Instantiate(gstage, init_pos, Quaternion.identity, GameObject.Find("Astage").transform) as GameObject;
                obj.name = gstage.name;

                obj = Instantiate(gpoint, pos, Quaternion.identity, GameObject.Find("Apoint").transform) as GameObject;
                obj.name = gpoint.name;

                pos.x += obj.transform.lossyScale.x;
                iwidth++;
            }
            else if (c == '\n') {
                Vector3 origin = new Vector3((float)iwidth, 1.0f, 0f);
                pos.z -= space.z;
                pos.x -= origin.x;
                iwidth = 0;
            }
            else if (c == ' ')
            {
                pos.x += 1.0f;
                iwidth++;
            }
            else if (c == '-' || c == '+') {
                //obj = Instantiate(gearth, pos, Quaternion.identity, GameObject.Find("Astage").transform) as GameObject;
                //obj.name = gearth.name;

                init_pos = new Vector3(pos.x, pos.y - 2, pos.z);
                obj = Instantiate(gwall, init_pos, Quaternion.identity, GameObject.Find("Awall").transform) as GameObject;
                obj.name = gwall.name;

                pos.x += obj.transform.lossyScale.x;
                iwidth++;
                //pos.x += space.x;
                //iwidth++;
            }
        }
    }

    public void DeleteObj(string objName) {
        /*
         * ステージというタグのオブジェクトをすべて消去
         */ 
        GameObject[] objs = GameObject.FindGameObjectsWithTag(objName);
        foreach(GameObject obj in objs) {
            GameObject.DestroyImmediate(obj);
        }
    }

    public void Delete()
    {
        DeleteObj("Stage");
        DeleteObj("Wall");
        DeleteObj("Point");
        DeleteObj("Earth");
    }
	
}