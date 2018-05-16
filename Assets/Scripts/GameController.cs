using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public Transform player;
    public MMOCharacterController moveControlls;
    public Animator switchBoard;
    public GameObject darkScreen;
    public Text loseText;
    public float FadeTime;
    public float maxAlpha;

    // Use this for initialization
    void Start () {
        loseText.text = "";
	}
	
	// Update is called once per frame
	void Update () {

        
        float angle = player.rotation.eulerAngles.x;

        if(angle > 180)
        {
            angle = (360 - angle) * -1;
        }
        switchBoard.SetFloat("Angle", angle * -1);

	}

    void OnControllerColliderHit(ControllerColliderHit hit)
    {

        switchBoard.SetTrigger("Collision");
        moveControlls.KillCharacter();
        StartCoroutine(fadeOutDeath());
    }
    void OnColliderHit(ControllerColliderHit hit)
    {

        switchBoard.SetTrigger("Collision");
        moveControlls.KillCharacter();
        StartCoroutine(fadeOutDeath());
    }

    public IEnumerator fadeOutDeath()
    {
        Color c = darkScreen.GetComponent<Image>().color;
        float time = 0;

        yield return new WaitForSecondsRealtime(.01f);

        float start = Time.unscaledTime;
        loseText.text = "GAME OVER";
        Color dC = loseText.color;

        float deathAlphaStart = dC.a;
        float deathRedStart = dC.r;
        

        while (time < FadeTime)
        {

            time = Time.unscaledTime - start;
            float alpha = time / FadeTime * maxAlpha;
            c.a = alpha;
            dC.r = deathRedStart + (255 - deathRedStart) * time / FadeTime;
            dC.a = deathAlphaStart + (1 - deathAlphaStart) * time / FadeTime;

            loseText.color = dC;

            darkScreen.GetComponent<Image>().color = c;
            yield return new WaitForSecondsRealtime(.01f);

        }
        //rb.isKinematic = false;
        //SceneManager.LoadScene(0);
        Application.Quit();

    }
}
