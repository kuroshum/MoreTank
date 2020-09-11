using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnahoriDungeon : MonoBehaviour
{
    /*
    *設定する値
    */
    private int max;        //縦横のサイズ ※必ず奇数にすること

    /*
    *内部パラメータ
    */
    private List<List<StageChip>> walls;      //マップの状態 0：壁 1：通路
    private int[] startPos;    //スタートの座標
    private int[] goalPos;     //ゴールの座標

    private int Vert;
    private int Hori;

    private int minSize = 5;
    private int maxSize = 9;

    private List<Border> border;

    private List<List<RoomChip>> room;

    private AnaHori ah;

    int[] startRoomPos;
    Border startBorder;

    System.Random rnd;


    public int[] CreateRange(int randx)
    {
        int[] range = new int[randx * 2 + 1];
        for (int i = 0; i < randx * 2 + 1; i++)
        {
            range[i] = -randx + i;
        }

        return range;
    }

    public void MakeObst(int len, List<RoomChip> tmp_room)
    {
        int size = len * 2 - 1;
        int cnt = 0;
        room = new List<List<RoomChip>>(size);
        for (int i = 0; i < size; i++)
        {
            room.Add(new List<RoomChip>(size));
            for (int j = 0; j < size; j++)
            {
                room[i].Add(new RoomChip(tmp_room[cnt].type, tmp_room[cnt].x, tmp_room[cnt].y));
                cnt++;
            }
        }

        room = ah.Generate(room, room.Count);

        for (int i = 0; i < room.Count; i++)
        {
            for (int j = 0; j < room.Count; j++)
            {
                walls[room[i][j].x][room[i][j].y].type = room[i][j].type;
            }
        }
    }

    public List<RoomChip> MakeRoom(int[] startPos, int id, int len)
    {
        int[] size = CreateRange(len);

        List<RoomChip> tmp_room = new List<RoomChip>();

        for (int i = 0; i < size.Length; i++)
        {
            for (int j = 0; j < size.Length; j++)
            {
                if (i == 0 || j == 0 || i == size.Length - 1 || j == size.Length - 1)
                {
                    if ((i != 0 || j != 0) && (i != size.Length - 1 || j != size.Length - 1) && (i != 0 || j != size.Length - 1) && (i != size.Length - 1 || j != 0))
                    {
                        if (walls[startPos[0] + size[i]][startPos[1] + size[j]].type != "#" && walls[startPos[0] + size[i]][startPos[1] + size[j]].type != "b")
                        {
                            border.Add(new Border(startPos[0] + size[i], startPos[1] + size[j], id));
                            walls[startPos[0] + size[i]][startPos[1] + size[j]].Set("-", id);
                        }
                    }

                }
                else
                {
                    walls[startPos[0] + size[i]][startPos[1] + size[j]].Set("#", id);
                    tmp_room.Add(new RoomChip("#", startPos[0] + size[i], startPos[1] + size[j]));
                }
            }
        }

        return tmp_room;

    }

    public bool CheckRoom(int[] startRoomPos, int len)
    {
        int[] size = CreateRange(len);

        for (int i = 0; i < size.Length; i++)
        {
            for (int j = 0; j < size.Length; j++)
            {

                if(startRoomPos[0] + size[i] > max-1 || startRoomPos[1] + size[j] > max-1 || startRoomPos[0] + size[i] < 0 || startRoomPos[1] + size[j] < 0)
                {
                    return false;
                }
                
                if (walls[startRoomPos[0] + size[i]][startRoomPos[1] + size[j]].type == "#")
                {
                    return false;
                }
                
            }
        }
        return true;
    }

    public int[] SearchPos(int len, int id)
    {

        while (true)
        {
            int ind = rnd.Next(0, border.Count);

            startBorder = border[ind];

            for (int i = -1; i < 2; i += 2)
            {
                if(startBorder.x + i > max-1 || startBorder.x + i < 0 || startBorder.y + i > max - 1 || startBorder.y + i < 0)
                {
                    Vert = 0;
                    Hori = 0;
                    break;
                }


                if (walls[startBorder.x + i][startBorder.y].type == "#")
                {
                    Vert = -i;
                    Hori = 0;
                }

                if (walls[startBorder.x][startBorder.y + i].type == "#")
                {
                    Vert = 0;
                    Hori = -i;
                }
            }

            if (Vert == 0 && Hori == 0) continue;

            startRoomPos = new int[2] { startBorder.x + (len * Vert), startBorder.y + (len * Hori) };

            //walls[startRoomPos[0], startRoomPos[1]] = "s";

            if (CheckRoom(startRoomPos, len)) break;
        }

        walls[startBorder.x][startBorder.y].type = "b";

        for (int i = 0; i < border.Count; i++)
        {
            if ((border[i].x == startBorder.x || border[i].y == startBorder.y) && border[i].id == startBorder.id)
            {
                if (walls[border[i].x][border[i].y].type != "#" && walls[border[i].x][border[i].y].type != "b")
                {
                    walls[border[i].x][border[i].y].Set("-", id);
                }
                border.RemoveAt(i);
                i--;
            }
        }

        return startRoomPos;


    }

    public string Generate(int ROOM_NUM, int max)
    {
        rnd = new System.Random(System.Environment.TickCount);

        this.max = max;

        border = new List<Border>();

        ah = this.GetComponent<AnaHori>();



        //マップ状態初期化
        walls = new List<List<StageChip>>(max);

        for (int i = 0; i < max; i++)
        {
            walls.Add(new List<StageChip>(max));
            for (int j = 0; j < max; j++)
            {
                walls[i].Add(new StageChip(" ", 0));
                //walls[i][j].type = " ";
            }
        }

        //スタート地点の取得
        startPos = new int[] { (max - 1) / 2, (max - 1) / 2 };
        int[] tmp = startPos;


        //ランダムでx,yを設定
        int len = rnd.Next(minSize, maxSize);
        List<RoomChip> tmp_room  = MakeRoom(startPos, 1, len);
        MakeObst(len, tmp_room);
        

        for (int i = 1; i < ROOM_NUM+1; i++)
        {
            len = rnd.Next(minSize, maxSize);
            startPos = SearchPos(len, i);
            tmp_room = MakeRoom(startPos, 1, len);
            MakeObst(len, tmp_room);
        }

        walls[tmp[0]][tmp[1]].type = "S";

        int[] RemoveRange = CreateRange(3);
        for(int i =0; i < walls.Count; i++)
        {
            for(int j = 0; j < walls.Count; j++)
            {
                if (walls[i][j].type == "b")
                {
                    for(int h = 0; h < RemoveRange.Length; h++)
                    {
                        for(int w = 0; w < RemoveRange.Length; w++)
                        {
                            if (walls[i + RemoveRange[h]][j + RemoveRange[w]].type == "+")
                            {
                                walls[i + RemoveRange[h]][j + RemoveRange[w]].type = "#";
                            }
                        }
                    }
                } 
            }
        }

        string StageFile = Application.dataPath + "/" + "Resources" + "/" + "stage3.txt";
        ReadWrite.ListWrite(StageFile, walls, max, max);

        string data = "";

        for (int i = 0; i < max; i++)
        {
            for (int j = 0; j < max; j++)
            {
                data = data + walls[i][j].type;
            }
            data = data + "\n";
        }

        return data;
    }

    bool Check_dead_end(int i, int j, int h, int v)
    {
        int cnt = 0;
        if (walls[i + h + 1][j + v + 1].type == "-") cnt++;
        if (walls[i + h - 1][j + v + 1].type == "-") cnt++;
        if (walls[i + h + 1][j + v - 1].type == "-") cnt++;
        if (walls[i + h - 1][j + v - 1].type == "-") cnt++;

        if (cnt >= 4) return false;
        else return true;
    }
}
