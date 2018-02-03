﻿using UnityEngine;
using System.Collections;

/// 2次元レイヤー
public class Layer2D {

	int _width; // 幅
	int _height; // 高さ
	int _outOfRange = -1; // 領域外を指定した時の値
	int[] _values = null; // マップデータ
	/// 幅
	public int Width {
		get { return _width; }
	}
	/// 高さ
	public int Height {
		get { return _height; }
	}

	/// 作成
	public void Create(int width, int height) {
		_width = width;
		_height = height;
		_values = new int[Width * Height];
	}

	/// 座標をインデックスに変換する
	public int ToIdx(int x, int y) {
		return x + (y * Width);
	}

	/// 領域外かどうかチェックする
	public bool IsOutOfRange(int x, int y) {
		if(x < 0 || x >= Width) { return true; }
		if(y < 0 || y >= Height) { return true; }

		// 領域内
		return false;
	}
	/// 値の取得
	// @param x X座標
	// @param y Y座標
	// @return 指定の座標の値（領域外を指定したら_outOfRangeを返す）
	public int Get(int x, int y) {
		if(IsOutOfRange(x, y)) {
			return _outOfRange;
		}

		return _values[y * Width + x];
	}

	/// 値の設定
	// @param x X座標
	// @param y Y座標
	// @param v 設定する値
	public void Set(int x, int y, int v) {
		if(IsOutOfRange(x, y)) {
			// 領域外を指定した
			return;
		}

		_values[y * Width + x] = v;
	}

	/// デバッグ出力
	public void Dump() {
		Debug.Log("[Layer2D] (w,h)=("+Width+","+Height+")");
		for(int y = 0; y < Height; y++) {
			string s = "";
			for(int x = 0; x < Width; x++) {
				s += Get(x, y) + ",";
			}
			Debug.Log(s);
		}
	}
}
