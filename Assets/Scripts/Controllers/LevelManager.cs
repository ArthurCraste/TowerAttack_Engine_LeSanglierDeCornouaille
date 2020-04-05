using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private float m_CurrentTime = 240;

    public void Start()
    {
        EntityManager entityManager = FindObjectOfType<EntityManager>();
        if(entityManager != null)
        {
            entityManager.OnTowerDestroy += EndGame;
        }
    }

    private void Update()
    {
        UpdateTime();
    }

    private void EndGame(Alignment alignment)
    {
        switch(alignment)
        {
            case Alignment.Player:
                Debug.Log("LOOOOOOOOOOSE ! GAME OVER !");
                break;
            case Alignment.IA:
                Debug.Log("WIN ! YOU'RE THE BEST");
                break;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    #region Timer
    public float GetCurrentTime()
    {
        return m_CurrentTime;
    }

    private void UpdateTime()
    {
        m_CurrentTime -= Time.deltaTime;

        if (m_CurrentTime <= 0)
        {
            EndGame(Alignment.Player);
        }
    }
    #endregion STAMINA
}
