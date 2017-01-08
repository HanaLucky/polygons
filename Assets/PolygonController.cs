using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonController : MonoBehaviour {
	// spriteを変更する ref.http://qiita.com/motsat/items/927a4d2682765555b80d
	SpriteRenderer mainSpriteRendere;
	// やったね！
	public Sprite yay;
	// やったね！（まばたき)
	public Sprite yayBlink;
	// かなしい
	public Sprite sad;
	// 退屈だ
	public Sprite meh;

	private enum Mood {Yay, Sad, Meh};

	// ご近所リスト
	List<GameObject> colList = new List<GameObject>();

	// dragged
	private bool isDragged = false;

	// mood
	private Mood mood;

	// Use this for initialization
	void Start () {
		mainSpriteRendere = gameObject.GetComponent<SpriteRenderer> ();
	}


	void OnTriggerEnter2D(Collider2D other) {
		// 近所さがし
		if (!this.colList.Contains(other.gameObject)) {
			this.colList.Add (other.gameObject);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (this.colList.Contains(other.gameObject)) {
			this.colList.Remove (other.gameObject);
		}
	}


	// ポリゴンドラッグ時の[最大サイズ]、[初期サイズ]、[拡大速度]
	private float maxScale = 2.0f;
	private float initialScale = 1.5f;
	private float acceleration4PolygonScale = 0.1f;

	// Update is called once per frame
	void Update () {
		// ドラッグ中の処理
		if (this.isDragged) {
			// ドラッグ中は少し大きく
			if (this.transform.localScale.x < maxScale) {
				this.transform.localScale += new Vector3 (acceleration4PolygonScale, acceleration4PolygonScale, 0);
			}
			return;
		} else {
			// ドラッグ中は少し大きく
			if (this.transform.localScale.x > initialScale) {
				this.transform.localScale -= new Vector3 (acceleration4PolygonScale, acceleration4PolygonScale, 0);
			}
		}

		// ご近所探し
		int same = 0;
		int neighbors = 0;
		for (int i = 0; i < colList.Count; i++) {
			neighbors++;
			if (gameObject.tag == colList [i].tag) {
				same++;
			}
		}
		float ratio = (float) same / neighbors;
		//		Debug.Log("same / neighbors = " + same + " / " + neighbors + " = " + ratio);

		// 状態判定
		if (neighbors > 0 && ratio < 0.33f) {
			// unhappy
			this.mood = Mood.Sad;
			mainSpriteRendere.sprite = sad;
			// ゆらゆら (http://albatrus.com/main/unity/7461) 
			transform.Rotate (new Vector3 (0.0f, 0.0f, Mathf.Sin (Time.time * 4.0f)));

		} else if (neighbors == 0 || ratio > 0.99f) {
			// bored
			this.mood = Mood.Meh;
			mainSpriteRendere.sprite = meh;
			// 姿勢はまっすぐ ref.https://ookumaneko.wordpress.com/2015/10/01/unity%E3%83%A1%E3%83%A2-transform%E3%81%AB%E3%83%AA%E3%82%BB%E3%83%83%E3%83%88%E5%87%A6%E7%90%86%E3%82%92%E8%BF%BD%E5%8A%A0%E3%81%97%E3%81%A6%E3%81%BF%E3%82%8B/
			transform.localRotation = Quaternion.identity;
		} else {
			this.mood = Mood.Yay;
			mainSpriteRendere.sprite = yay;
			// 姿勢はまっすぐ
			transform.localRotation = Quaternion.identity;
		}

		// Debug.Log ("same="+same + " neighbors="+neighbors + " order=" + this.gameObject.GetComponent<Renderer> ().sortingOrder);
	}

	// もといた位置
	float potentialX = 0.0f;
	float potentialY = 0.0f;

	void OnMouseDown() {
		// 回転を正位置に戻す（宙ぶらりんからの戻りを考慮して）
		transform.localRotation = Quaternion.identity;

		// もといた位置
		potentialX = transform.position.x;
		potentialY = transform.position.y;

	}

	// 宙ぶらりん角度？
	private float dangle = 0.0f;

	// 加速度
	private float dangleVel = 0.0f;

	void OnMouseDrag () {
		// unhappyの時だけ移動可能
		if (this.mood == Mood.Meh || this.mood == Mood.Yay ) {
			Debug.Log ("i don't want to move. i feel like " + mood.ToString());
			return;
		}
		// dragged
		this.isDragged = true;

		// ドラッグ中は幸せな予感
		this.mainSpriteRendere.sprite = yay;

		// 移動
		Vector3 objectPointInScreen = Camera.main.WorldToScreenPoint(this.transform.position);

		Vector3 mousePointInScreen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectPointInScreen.z);

		Vector3 mousePointInWorld = Camera.main.ScreenToWorldPoint(mousePointInScreen);
		mousePointInWorld.z = this.transform.position.z;
		this.transform.position = mousePointInWorld;

		// 移動中は最前面に表示する
		this.gameObject.GetComponent<Renderer> ().sortingOrder = 32767;

		// 宙吊り
		// 移動量 ref.http://phisz.blog.fc2.com/blog-entry-25.html
		float mouse_x_delta = Input.GetAxis("Mouse X");
		dangle += mouse_x_delta * 5.0f;
		transform.Rotate (new Vector3 (0.0f, 0.0f, -dangle));
		dangleVel += dangle * (-0.2f);
		dangle += dangleVel;
		dangle *= 0.9f;

	}

	void OnMouseUp() {
		// 最前面に表示していたものを元に戻す
		this.gameObject.GetComponent<Renderer> ().sortingOrder = 1;

		// 宙吊り解除
		dangle = 0.0f;
		dangleVel = 0.0f;

		// TODO: その位置に向かわせる。3平方の定理的なあれでいけそうなきがする。

		// dragged解除
		this.isDragged = false;
	}
}
