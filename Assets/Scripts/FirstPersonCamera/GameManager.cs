using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Variables
    public GameObject player;
    #endregion

    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}