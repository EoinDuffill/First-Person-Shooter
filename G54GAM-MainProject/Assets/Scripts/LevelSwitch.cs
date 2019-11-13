using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSwitch : MonoBehaviour
{
    public int levelIndexToLoad;
    public bool boss = false;
    public int countDown = 15;
    private bool levelOver = false;
    public BossAllySpawner bossController;

    private bool finish = false;

    private void OnTriggerEnter(Collider collider)
    {
        if (!boss)
        {
            if (collider.tag == "Player")
            {
                LevelDataManager.instance.setData(collider.transform.Find("Inventory").GetComponent<Inventory>());

                SceneManager.LoadScene(levelIndexToLoad);
            }
        }
        
    }

    void Update()
    {
        if (boss && bossController.bossesDead && !levelOver)
        {
            levelOver = true;
            StartCoroutine(endLevel());
        }
        if (finish)
        {
            finish = false;
            LevelDataManager.instance.reset();

            SceneManager.LoadScene(levelIndexToLoad);
        }
    }

    IEnumerator endLevel()
    {
        Debug.Log("Ending");

        Transform panel = GameObject.Find("UICanvas").transform.Find("BossFight").Find("Panel");
        panel.gameObject.SetActive(true);
        Text gameOverText = panel.Find("gameOverText").GetComponent<Text>();

        while (countDown > 0)
        {
            countDown--;
            gameOverText.text = "Congratulations!\nRestarting in "+countDown;
            yield return new WaitForSeconds(1f);
        }
        finish = true;
    }

    public void reloadLevel(int level)
    {
        LevelDataManager.instance.death();

        SceneManager.LoadScene(level);
    }
}
