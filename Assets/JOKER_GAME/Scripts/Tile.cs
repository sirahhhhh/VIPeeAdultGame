using UnityEngine;
using System.Collections;

/// タイル管理
public class Tile : Token {

	public static TokenMgr<Tile> parent = null;
	public static Tile Add(int id, float x, float y) {
		if(parent == null) {
			parent = new TokenMgr<Tile>("Tile");
		}
		var t = parent.Add(x, y);
		t.Create(id);
		return t;
	}

	public void Create(int id) {
        //string dirPath = "Levels/base";
        //string filePath = "base_";
        string dirPath = "Levels/ST-Office-I01";
        string filePath = "ST-Office-I01_";
        var spr = Util.GetSprite(dirPath, filePath + (id-1));
		SetSprite(spr);
	}

	void Start () {
	}
	
	void Update () {
	}
}
