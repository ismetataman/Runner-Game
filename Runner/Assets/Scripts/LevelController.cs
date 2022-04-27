using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController Current;
    public bool gameActive= false;

    public GameObject startMenu, gameMenu, gameOverMenu, finishMenu;
    public Button rewardedAdButton;
    public Text scoreText, finishScoreText, currentLevelText, nextLevelText,staringMenuMoneyText,gameOverMenuMoneyText,finishGameMenuMoneyText;
    public Slider levelProgressBar;
    public float maxDistance;
    public GameObject finishLine;
    int currentLevel;
    public int score;
    public AudioSource gameMusicAudioSource;
    public AudioClip victoryAudioClip, gameOverAudioClip;
    public DailyReward dailyReward;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[]parentsInScene = this.gameObject.scene.GetRootGameObjects();
        foreach(GameObject parent in parentsInScene)
        {
            TextObject[] textObjectsInParent = parent.GetComponentsInChildren<TextObject>(true);
            foreach (TextObject textObject in textObjectsInParent)
            {
                textObject.InitTextObject();
            }
        }
        Current = this;
        currentLevel = PlayerPrefs.GetInt("currentLevel");
        PlayerController.Current = GameObject.FindObjectOfType<PlayerController>();
        GameObject.FindObjectOfType<MarketController>().InitializeMarketController();
        dailyReward.InitializeDailyReward();
        currentLevelText.text = (currentLevel +1).ToString();
        nextLevelText.text = (currentLevel +2).ToString();
        UpdateMoneyTexts();
        gameMusicAudioSource = Camera.main.GetComponent<AudioSource>();
         if(AddController.Current.IsReadyInterstitalAd())
        {
            AddController.Current.interstitial.Show();
        }
    }

    public void ShowRewardedAd()
    {
        if(AddController.Current.rewardedAd.IsLoaded())
        {
            AddController.Current.rewardedAd.Show();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameActive)
        {
            PlayerController player = PlayerController.Current;
            float distance =finishLine.transform.position.z - PlayerController.Current.transform.position.z;
            levelProgressBar.value = 1- (distance / maxDistance);
        }
    }

    public void StartLevel()
    {
        AddController.Current.bannerView.Hide();
        maxDistance = finishLine.transform.position.z - PlayerController.Current.transform.position.z;
        PlayerController.Current.ChangeSpeed(PlayerController.Current.runningSpeed);
        startMenu.SetActive(false);
        gameMenu.SetActive(true);
        PlayerController.Current.animator.SetBool("running", true);
        gameActive = true;
    }
    public void RestartLevel()
    {
        LevelLoader.Current.ChangeLevel(SceneManager.GetActiveScene().name);
    }
    public void LoadNextLevel()
    {
        LevelLoader.Current.ChangeLevel("Level " + (currentLevel + 1));
    }
    public void GameOver()
    {
        if(AddController.Current.IsReadyInterstitalAd())
        {
            AddController.Current.interstitial.Show();
        }
        AddController.Current.bannerView.Show();
        UpdateMoneyTexts();
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(gameOverAudioClip);
        gameMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameActive = false;
    }
    public void FinishGame()
    {
        if(AddController.Current.rewardedAd.IsLoaded())
        {
            rewardedAdButton.gameObject.SetActive(true);
        }
        else{
            rewardedAdButton.gameObject.SetActive(false);
        }

        AddController.Current.bannerView.Show();
        GiveMoneyToPlayer(score);
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(victoryAudioClip);
        PlayerPrefs.SetInt("currentLevel", currentLevel + 1);
        finishScoreText.text = score.ToString();
        gameMenu.SetActive(false);
        finishMenu.SetActive(true);
        gameActive = false;
    }
    public void ChangeScore(int increment)
    {
        score += increment;
        scoreText.text = score.ToString();
    }
    public void UpdateMoneyTexts()
    {
        int money = PlayerPrefs.GetInt("money");
        staringMenuMoneyText.text = money.ToString();
        gameOverMenuMoneyText.text = money.ToString();
        finishGameMenuMoneyText.text = money.ToString();
    }

    public void GiveMoneyToPlayer(int increment)
    {
        int money= PlayerPrefs.GetInt("money");
        money = Mathf.Max (0, money + increment);
        PlayerPrefs.SetInt("money", money);
        UpdateMoneyTexts();
    }
}
