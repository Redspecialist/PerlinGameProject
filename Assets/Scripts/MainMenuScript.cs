using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour {

    public TextMeshProUGUI characterName;
    public BirdObject[] birds;
    private int currBird = 0;
    public float gapTime = .5f;
    public float paceTime = 3f;
    private bool lockControls;
    public TMP_InputField terrainSeed;
    public TMP_InputField biomeSeed;
    public TMP_InputField islandSeed;
    public Image fadeOutPanel;
    public GameObject fadeOutPanelObject;
    public InformationTransfer transfer;

    public void StartGame()
    {
        if (!lockControls)
        {
            lockControls = true;
            StartCoroutine(takeOffSequence());
        }
        
    }
    public void OnGUI()
    {
        terrainSeed.text = Regex.Replace(terrainSeed.text, @"[^A-F0-9]", "");
        terrainSeed.text = terrainSeed.text.Substring(0, Mathf.Min(8, terrainSeed.text.Length));

        biomeSeed.text = Regex.Replace(biomeSeed.text, @"[^A-F0-9]", "");
        biomeSeed.text = biomeSeed.text.Substring(0, Mathf.Min(8, biomeSeed.text.Length));
    
        islandSeed.text = Regex.Replace(islandSeed.text, @"[^A-F0-9]", "");
        islandSeed.text = islandSeed.text.Substring(0, Mathf.Min(8, islandSeed.text.Length));
    }

    IEnumerator takeOffSequence()
    {
        fadeOutPanelObject.SetActive(true);
        for (float currTime = 0; currTime < gapTime;)
        {
            yield return null;
            currTime += Time.deltaTime;
            birds[currBird].animation.SetFloat("StartGame", currTime / gapTime);
        }
       
        for (float currTime = 0; currTime < paceTime;)
        {
            yield return null;
            currTime += Time.deltaTime;
            Color c = fadeOutPanel.color;
            c.a = currTime / paceTime;
            fadeOutPanel.color = c;
        }

        PlayGame();
    }

	public void PlayGame()
    {
        transfer.SetTerrainNoise(terrainSeed.text);
        transfer.SetIslandNoise(islandSeed.text);
        transfer.SetBiomeNoise(biomeSeed.text);
        transfer.SetBirdModel(currBird);
        SceneManager.LoadScene("perlinProject");
        //EditorSceneManager.OpenScene("PerlinProject");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void changeBird(bool dir)
    {
        if (!lockControls)
        {
            //True left, flase right
            currBird = currBird + (dir ? -1 : 1);
            currBird %= birds.Length;
            if (currBird <= -1)
            {
                currBird = birds.Length + currBird;
            }
            LoadBird();
        }
        
    }

    public void resetStartGameMenu()
    {
        if (!lockControls) { 
            currBird = 0;
        LoadBird();
        }
    }

    void LoadBird()
    {
        for(int i = 0; i < birds.Length; i++)
        {
            birds[i].bird.SetActive(false);
        }
        BirdObject bird = birds[currBird];
        characterName.colorGradientPreset = bird.coloring;
        characterName.text = bird.name;
        bird.bird.SetActive(true);
        
    }

}
[System.Serializable]
public class BirdObject
{
    public GameObject bird;
    public string name;
    public TMP_ColorGradient coloring;
    public Animator animation;
}