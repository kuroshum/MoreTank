using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{

    public void OnClickStartEasyButton()
    {
        GameMgr.ENEMY_NUM = 24;
        GameMgr.ROOM_NUM = 2;
        GameMgr.enemy_move_speed = 1.5f;
        SceneManager.LoadScene("play");
    }

    public void OnClickStartNormalButton()
    {
        GameMgr.ENEMY_NUM = 28;
        GameMgr.ROOM_NUM = 4;
        GameMgr.enemy_move_speed = 3.0f;
        SceneManager.LoadScene("play");
    }

    public void OnClickStartHardButton()
    {
        GameMgr.ENEMY_NUM = 36;
        GameMgr.ROOM_NUM = 5;
        GameMgr.enemy_move_speed = 5.0f;
        SceneManager.LoadScene("play");
    }
}
