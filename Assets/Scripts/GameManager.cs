using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject obstacle;
    public Transform spawnPoint;
    private int score = 0;

    public TextMeshProUGUI scoreText;
    public GameObject playBtn;
    public GameObject player;


    private void Start()
    {

    }

    IEnumerator SpawnObstacle()
    {
        while (true)
        {
            float waitTime = Random.Range(0.5f, 2f);
            yield return new WaitForSeconds(waitTime);
            Instantiate(obstacle, spawnPoint.position, Quaternion.identity);
        }
    }

    private void ScoreUp()
    {
        score++;
        scoreText.text = score.ToString();
    }

    public void GameStart()
    {
        player.SetActive(true);
        playBtn.SetActive(false);
        StartCoroutine("SpawnObstacle");
        InvokeRepeating("ScoreUp", 2f, 1f);
    }
}
