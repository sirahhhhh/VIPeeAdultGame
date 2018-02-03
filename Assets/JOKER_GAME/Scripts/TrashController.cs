using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashController : MonoBehaviour {
    public AStar astarScript = null;
    float trashExistTime = 3.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter Character");

        // ゴミで邪魔できないなら処理しない
        if (!astarScript.hasJammerTrashChara()) return;

        //var charaScript = collision.gameObject.GetComponent<AStar>();
        astarScript.SetCharaStatus();
        astarScript.SetTrashObj(this.gameObject);
    }
}
