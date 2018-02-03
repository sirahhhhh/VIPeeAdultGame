using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Novel;

/// A-star algorithm
public class AStar : MonoBehaviour {
    public GameObject trash;

    struct Point2 {
		public int x;
		public int y;
		public Point2(int x=0, int y=0) {
			this.x = x;
			this.y = y;
		}

		public void Set(int x, int y) {
			this.x = x;
			this.y = y;
		}
	}

	/// A-starノード.
	class ANode {
		enum eStatus {
			None,
			Open,
			Closed,
		}
		/// ステータス
		eStatus _status = eStatus.None;
		/// 実コスト
		int _cost = 0;
		/// ヒューリスティック・コスト
		int _heuristic = 0;
		/// 親ノード
		ANode _parent = null;
		/// 座標
		int _x = 0;
		int _y = 0;
		public int X {
			get { return _x; }
		}
		public int Y {
			get { return _y; }
		}
		public int Cost {
			get { return _cost; }
		}

		/// コンストラクタ.
		public ANode(int x, int y) {
			_x = x;
			_y = y;
		}
		/// スコアを計算する.
		public int GetScore() {
			return _cost + _heuristic;
		}
		/// ヒューリスティック・コストの計算.
		public void CalcHeuristic(bool allowdiag, int xgoal, int ygoal) {

			if(allowdiag) {
				// 斜め移動あり
				var dx = (int)Mathf.Abs (xgoal - X);
				var dy = (int)Mathf.Abs (ygoal - Y);
				// 大きい方をコストにする
				_heuristic =  dx > dy ? dx : dy;
			}
			else {
				// 縦横移動のみ
				var dx = Mathf.Abs (xgoal - X);
				var dy = Mathf.Abs (ygoal - Y);
				_heuristic = (int)(dx + dy);
			}
			//Dump();
		}
		/// ステータスがNoneかどうか.
		public bool IsNone() {
			return _status == eStatus.None;
		}
		/// ステータスをOpenにする.
		public void Open(ANode parent, int cost) {
			//Debug.Log (string.Format("Open: ({0},{1})", X, Y));
			_status = eStatus.Open;
			_cost   = cost;
			_parent = parent;
		}
		/// ステータスをClosedにする.
		public void Close() {
			//Debug.Log (string.Format ("Closed: ({0},{1})", X, Y));
			_status = eStatus.Closed;
		}
		/// パスを取得する
		public void GetPath(List<Point2> pList) {
			pList.Add(new Point2(X, Y));
			if(_parent != null) {
				_parent.GetPath(pList);
			}
		}
		public void Dump() {
			Debug.Log (string.Format("({0},{1})[{2}] cost={3} heuris={4} score={5}", X, Y, _status, _cost, _heuristic, GetScore()));
		}
		public void DumpRecursive() {
			Dump ();
			if(_parent != null) {
				// 再帰的にダンプする.
				_parent.DumpRecursive();
			}
		}
	}

	/// A-starノード管理.
	class ANodeMgr {
		/// 地形レイヤー.
		Layer2D _layer;
		/// 斜め移動を許可するかどうか.
		bool _allowdiag = true;
		/// オープンリスト.
		List<ANode> _openList = null;
		/// ノードインスタンス管理.
		Dictionary<int,ANode> _pool = null;
		/// ゴール座標.
		int _xgoal = 0;
		int _ygoal = 0;
		public ANodeMgr(Layer2D layer, int xgoal, int ygoal, bool allowdiag=true) {
			_layer = layer;
			_allowdiag = allowdiag;
			_openList = new List<ANode>();
			_pool = new Dictionary<int, ANode>();
			_xgoal = xgoal;
			_ygoal = ygoal;
		}
		/// ノード生成する.
		public ANode GetNode(int x, int y) {
			var idx = _layer.ToIdx(x, y);
			if(_pool.ContainsKey(idx)) {
				// 既に存在しているのでプーリングから取得.
				return _pool[idx];
			}

			// ないので新規作成.
			var node = new ANode(x, y);
			_pool[idx] = node;
			// ヒューリスティック・コストを計算する.
			node.CalcHeuristic(_allowdiag, _xgoal, _ygoal);
			return node;
		}
		/// ノードをオープンリストに追加する.
		public void AddOpenList(ANode node) {
			_openList.Add(node);
		}
		/// ノードをオープンリストから削除する.
		public void RemoveOpenList(ANode node) {
			_openList.Remove(node);
		}
		/// 指定の座標にあるノードをオープンする.
		public ANode OpenNode(int x, int y, int cost, ANode parent) {
			// 座標をチェック.
			if(_layer.IsOutOfRange(x, y)) {
				// 領域外.
				return null;
			}
			if(_layer.Get(x, y) > 6) {
				// 通過できない.
				return null;
			}
			// ノードを取得する.
			var node = GetNode(x, y);
			if(node.IsNone() == false) {
				// 既にOpenしているので何もしない
				return null;
			}

			// Openする.
			node.Open(parent, cost);
			AddOpenList(node);

			return node;
		}

		/// 周りをOpenする.
		public void OpenAround(ANode parent) {
			var xbase = parent.X; // 基準座標(X).
			var ybase = parent.Y; // 基準座標(Y).
			var cost = parent.Cost; // コスト.
			cost += 1; // 一歩進むので+1する.
			if(_allowdiag) {
				// 8方向を開く.
				for(int j = 0; j < 3; j++) {
					for(int i = 0; i < 3; i++) {
						var x = xbase + i - 1; // -1～1
						var y = ybase + j - 1; // -1～1
						OpenNode(x, y, cost, parent);
					}
				}
			}
			else {
				// 4方向を開く.
				var x = xbase;
				var y = ybase;
				OpenNode (x-1, y,   cost, parent); // 右.
				OpenNode (x,   y-1, cost, parent); // 上.
				OpenNode (x+1, y,   cost, parent); // 左.
				OpenNode (x,   y+1, cost, parent); // 下.
			}
		}

		/// 最小スコアのノードを取得する.
		public ANode SearchMinScoreNodeFromOpenList() {
			// 最小スコア
			int min = 9999;
			// 最小実コスト
			int minCost = 9999;
			ANode minNode = null;
			foreach(ANode node in _openList) {
				int score = node.GetScore();
				if(score > min) {
					// スコアが大きい
					continue;
				}
				if(score == min && node.Cost >= minCost) {
					// スコアが同じときは実コストも比較する
					continue;
				}

				// 最小値更新.
				min = score;
				minCost = node.Cost;
				minNode = node;
			}
			return minNode;
		}
	}

	/// チップ上のX座標を取得する.
	float GetChipX(int i) {
		Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        string dirPath = "Levels/ST-Office-I01";
        //string dirPath = "Levels/base";
        string fileName = "ST-Office-I01_5"; 
        //string fileName = "base_0";
        var spr = Util.GetSprite(dirPath, fileName);
		var sprW = spr.bounds.size.x;

		return min.x + (sprW * i) + sprW/2;
	}

	/// チップ上のy座標を取得する.
	float GetChipY(int j) {
		Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        string dirPath = "Levels/ST-Office-I01";
        //string dirPath = "Levels/base";
        string fileName = "ST-Office-I01_5"; 
        //string fileName = "base_0";
        var spr = Util.GetSprite(dirPath, fileName);
		var sprH = spr.bounds.size.y;

		return max.y - (sprH * j) - sprH/2;
	}

	/// ランダムな座標を取得する.
	Point2 GetRandomPosition(Layer2D layer) {
		Point2 p;
		while(true) {
			p.x = Random.Range(0, layer.Width);
			p.y = Random.Range(0, layer.Height);
			if(layer.Get(p.x, p.y) == 6) {
				// 通過可能
				break;
			}
		}
		return p;
	}

	// 状態.
	enum eState {
        SetUp,       // 初期処理
        Prepare,     // 準備
		Exec_Astar,  // 実行中.
        Calc,
		Walk_Start, // 移動開始.
        Walking, // 移動中.
        End,  // おしまい.
	}

    // キャラクタのステータス
    enum eChara_Status
    {
        Fine,
        Trash,
    }

    // キャラクタの向き
    enum eChara_Dir
    {
        DOWN = 0,
        LEFT,
        UP,
        RIGHT,
        DIR_END,
    }
	eState _state = eState.SetUp;
    eChara_Status charaStatus = eChara_Status.Fine;
    eChara_Dir charaDir = eChara_Dir.DOWN;

    TMXLoader tmx;
    Layer2D layer;
    List<Point2> pList;
    int listIndex = 0;
    Token player = null;
    Vector3 hitObjPos;
    GameObject putTrash;
    GameObject jammaerTrashObj = null;

    //bool hasFinishAtarCalc = false; // Astarの経路算出終了したか
    bool hasWalkChara = false;      // キャラが移動できるか
    bool hasCallWalkCoroutine = false; // キャラ移動コルーチンが呼ばれているか
    float trashJammerTime = 3.0f; // ゴミの足止め時間

    IEnumerator Start () {

        // 地形データのロード.
        tmx = new TMXLoader();
        tmx.Load("Levels/001");
        layer = tmx.GetLayer(0);
        //layer.Dump();

        // タイルの配置.
        for (int j = 0; j < layer.Height; j++) {
			for(int i = 0; i < layer.Width; i++) {
				var v = layer.Get(i, j);
				var x = GetChipX(i);
				var y = GetChipY(j);
				Tile.Add(v, x, y);
			}
		}
        yield return new WaitForSeconds(0.1f);

        //var pList = new List<Point2>();
        pList = new List<Point2>();

        // Astar計算へ
        _state = eState.Prepare;
        yield break;
    }
	
	void Update () {
        InputMouse();   // マウス入力
        InputKeys();    // キー入力

        // キャラクタ処理
        switch(_state)
        {
            // 準備
            case eState.Prepare:
                _state = eState.Exec_Astar;
                break;

            // Astar計算
            case eState.Exec_Astar:
                StartCoroutine(StartAstar());
                _state = eState.Calc;
                break;
            
            // 移動中
            case eState.Walking:
                // 移動コルーチンが呼ばれているなら終了
                if (hasCallWalkCoroutine) break;
                // キャラステータスが正常でなければ移動しない
                if (charaStatus != eChara_Status.Fine) break;

                StartCoroutine(WalkPlayer());
                hasCallWalkCoroutine = true;
                break;
        }
	}

    // ゴミで邪魔できるか
    public bool hasJammerTrashChara()
    {
        var ret = true;
        
        if(charaStatus == eChara_Status.Trash)
        {
            ret = false;
        }

        return ret;
    }

    // キャラステータス設定
    public void SetCharaStatus()
    {
        charaStatus = eChara_Status.Trash;
        StartCoroutine(trashJammer());
    }

    // お邪魔中のゴミObjを保存(暫定的処理
    public void SetTrashObj(GameObject a_obj)
    {
        jammaerTrashObj = a_obj;
    }

    // キー入力
    private void InputKeys()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // マウス入力
    private void InputMouse()
    {
        Vector3 cursorPosition = Input.mousePosition;
        Vector3 screenPointPos = Camera.main.ScreenToWorldPoint(cursorPosition);

        // 左ボタン押下されているなら
        if(Input.GetMouseButtonDown(0))
        {
            var cellCollider2D = Physics2D.OverlapPoint(screenPointPos);
            if(cellCollider2D)
            {
                var hitObj = Physics2D.Raycast(screenPointPos, -Vector2.up);
                if(hitObj)
                {
                    // ゴミが既にあるなら処理しない
                    if (hitObj.collider.gameObject.tag == "Trash") return;

                    hitObjPos = hitObj.collider.gameObject.transform.localPosition;
                    //Debug.Log("hit obj is " + hitObj.collider.gameObject.name);

                    // ゴミばらまく
                    CreateTrash();
                }
            }
        }
    }

    // ゴミばら撒く
    private void CreateTrash()
    {
        putTrash = (GameObject)Instantiate(trash, hitObjPos, Quaternion.identity);
        putTrash.SetActive(true);
    }

    // キャラクタの向きを決定する
    private void DetermineCharaDir(Vector3 a_NowPos, Vector3 a_TargetPos)
    {
        var diffVec3 = a_TargetPos - a_NowPos;
        eChara_Dir detDir = eChara_Dir.DOWN;
        float compUnit = 0.001f;
        if (diffVec3.x >= compUnit) detDir = eChara_Dir.RIGHT;
        else if (diffVec3.x <= -compUnit) detDir = eChara_Dir.LEFT;
        else if (diffVec3.y >= compUnit) detDir = eChara_Dir.UP;
        else if (diffVec3.y <= -compUnit) detDir = eChara_Dir.DOWN;

        charaDir = detDir;
    }

    // ゴミの妨害
    IEnumerator trashJammer()
    {
        yield return new WaitForSeconds(trashJammerTime);

        // キャラ状態を正常にしてコルーチン終了
        charaStatus = eChara_Status.Fine;
        // ゴミ消去
        if(jammaerTrashObj) Destroy(jammaerTrashObj);
        yield break;
    }

    // キャラクタの移動
    IEnumerator WalkPlayer()
    {
        // 移動先の座標を取得
        var x = GetChipX(pList[listIndex].x);
        var y = GetChipY(pList[listIndex].y);
        //player.X = x;
        //player.Y = y;

        Vector3 playerNowPos = new Vector3(player.X, player.Y, 0.0f);
        Vector3 playerTargetPos = new Vector3(x, y, 0.0f);
        DetermineCharaDir(playerNowPos, playerTargetPos);

        // 移動しきるフレーム数のカウント
        int whileCnt = 0;
        const int LimitCnt = 5;

        // 移動差分算出
        float dd = 0.0f;
        if ((charaDir == eChara_Dir.DOWN) || (charaDir == eChara_Dir.UP))
        {
            dd = Mathf.Abs(y - player.Y);
        }
        else if ((charaDir == eChara_Dir.LEFT) || (charaDir == eChara_Dir.RIGHT))
        {
            dd = Mathf.Abs(x - player.X);
        }
        dd /= LimitCnt;
        while (true)
        {
            playerNowPos.x = player.X;
            playerNowPos.y = player.Y;
            var retVec3 = Vector3.MoveTowards( playerNowPos, playerTargetPos, dd);
            player.X = retVec3.x;
            player.Y = retVec3.y;

            
            if(whileCnt >= LimitCnt) { break; }
            ++whileCnt;
            yield return new WaitForSeconds(0.1f);
        }
        player.X = x;
        player.Y = y;
        ++listIndex;

        // リスト最後の最後までインデックスが到達
        // =>移動終了
        if (listIndex >= pList.Count)
        {
            // おしまい
            _state = eState.End;
            listIndex = 0;
        }

        // 移動インターバル
        // 移動時間はここで変える
        yield return new WaitForSeconds(0.2f);

        // コルーチン終了処理
        hasCallWalkCoroutine = false;
        yield break;
    }

	void OnGUI() {
		switch(_state) {
		case eState.Exec_Astar:
			Util.GUILabel(160, 160, 128, 32, "経路計算中...");
			break;
		case eState.Walking:
			Util.GUILabel(160, 160, 128, 32, "移動中");
			break;
		case eState.End:
                if (GUI.Button(new Rect(160, 160, 128, 32), "もう一回")) {
				    Tile.parent = null;
                    SceneManager.LoadScene("Main");
                    Destroy(putTrash);
                }
                int padd = 48;
                if (GUI.Button(new Rect(160, 160 + padd, 128, 32), "やめる"))
                {
                    Destroy(putTrash);
                    // JOKERスクリプトのタイトル画面へ
                    NovelSingleton.StatusManager.callJoker("wide/title", "");
                }
            break;
		}
	}

    // Astarのコルーチン化
    IEnumerator StartAstar()
    {
        // スタート地点.
        Point2 pStart = GetRandomPosition(layer);
        player = Util.CreateToken(GetChipX(pStart.x), GetChipY(pStart.y), "", "sample_fantasy_chara_02_40", "Player");
        player.SortingLayer = "Chara";
        // ゴール.
        Point2 pGoal = GetRandomPosition(layer);
        var goal = Util.CreateToken(GetChipX(pGoal.x), GetChipY(pGoal.y), "", "goal", "Goal");
        goal.SortingLayer = "Chara";
        // 斜め移動不可
        var allowdiag = false;
        var mgr = new ANodeMgr(layer, pGoal.x, pGoal.y, allowdiag);

        // スタート地点のノード取得
        // スタート地点なのでコストは「0」
        ANode node = mgr.OpenNode(pStart.x, pStart.y, 0, null);
        mgr.AddOpenList(node);

        // 試行回数。1000回超えたら強制中断
        int cnt = 0;
        while (cnt < 1000)
        {
            mgr.RemoveOpenList(node);
            // 周囲を開く
            mgr.OpenAround(node);
            // 最小スコアのノードを探す.
            node = mgr.SearchMinScoreNodeFromOpenList();
            if (node == null)
            {
                // 袋小路なのでおしまい.
                Debug.Log("Not found path.");

                PrepareWalkChara();
                _state = eState.Walking;
                break;
            }
            if (node.X == pGoal.x && node.Y == pGoal.y)
            {
                // ゴールにたどり着いた.
                Debug.Log("Success.");
                mgr.RemoveOpenList(node);
                //node.DumpRecursive();
                // パスを取得する
                node.GetPath(pList);
                // 反転する
                pList.Reverse();

                PrepareWalkChara();
                _state = eState.Walking;
                break;
            }

            ++cnt;
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    // キャラ移動の準備処理
    private void PrepareWalkChara()
    {
        // Astar算出終了
        //hasFinishAtarCalc = true;
        // キャラ移動可
        hasWalkChara = true;
    }
}
