using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBootScript : MonoBehaviour {

    public GameObject[] prefabs;
    public GameObject parent;
    public GameController controller;
	// Use this for initialization
	public void Start () {
        GameObject setting = prefabs[AssetInfo.getCharacterNumber()];

        Animator hit = Instantiate(setting, parent.transform).GetComponent<Animator>();
        controller.switchBoard = hit;
	}
	
	
}
