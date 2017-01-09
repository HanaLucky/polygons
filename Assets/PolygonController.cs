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

	// ムード
	public enum Mood {Yay, Sad, Meh};

	// ご近所リスト
	List<GameObject> colList = new List<GameObject>();

	// dragged
	private bool isDragged = false;

	// mood
	private Mood mood;

	// polygon generator
	GameObject polygonGenerator;

	private bool gameIsClear = false;

	// Use this for initialization
	void Start () {
		this.mainSpriteRendere = gameObject.GetComponent<SpriteRenderer> ();
		this.polygonGenerator = GameObject.Find ("PolygonGenerator");
	}
		
	/// <summary>
	/// Raises the trigger enter2 d event.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerEnter2D(Collider2D other) {
		// 近所さがし
		if (!this.colList.Contains(other.gameObject)) {
			this.colList.Add (other.gameObject);
		}
	}

	/// <summary>
	/// Raises the trigger exit2 d event.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerExit2D(Collider2D other) {
		if (this.colList.Contains(other.gameObject)) {
			this.colList.Remove (other.gameObject);
		}
	}

	// ポリゴンドラッグ時の[最大サイズ]、[初期サイズ]、[拡大速度]
	private float maxScale = 2.5f;
	private float initialScale = 1.5f;
	private float acceleration4PolygonScale = 0.2f;
	private float doneAnimFrame = 30.0f;

	// Update is called once per frame
	void Update () {
		// TODO clear effect
//		if (this.gameIsClear) {
//			// Clear Effect
//			if (doneAnimFrame > 0) {
//				doneAnimFrame--;
//				float opacity = ((doneAnimFrame % 15f) / 15f) * 0.2f;
//				Debug.Log ("doneAnimFrame = " + doneAnimFrame + "  opacity = " + opacity);
//				Camera.main.backgroundColor = new Vector4 (255f, 255f, 255f, opacity);
//			} else {
//				Camera.main.backgroundColor = Color.black;
//			}
//			return;
//		}

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

		this.setMood ();

		// Debug.Log ("same="+same + " neighbors="+neighbors + " order=" + this.gameObject.GetComponent<Renderer> ().sortingOrder);
	}

	// もといた位置
	private float potentialX = 0.0f;
	private float potentialY = 0.0f;

	/// <summary>
	/// Raises the mouse down event.
	/// </summary>
	void OnMouseDown() {
		// 回転を正位置に戻す（宙ぶらりんからの戻りを考慮して）
		transform.localRotation = Quaternion.identity;
		potentialX = transform.position.x;
		potentialY = transform.position.y;

	}

	// 宙ぶらりん角度？
	private float dangle = 0.0f;

	// 加速度
	private float dangleVel = 0.0f;

	/// <summary>
	/// Raises the mouse drag event.
	/// </summary>
	void OnMouseDrag () {
		// unhappyの時だけ移動可能
		if (this.mood == Mood.Meh || this.mood == Mood.Yay ) {
			//Debug.Log ("i don't want to move. i feel like " + mood.ToString());
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

	/// <summary>
	/// Raises the mouse up event.
	/// </summary>
	void OnMouseUp() {
		// 最前面に表示していたものを元に戻す
		this.gameObject.GetComponent<Renderer> ().sortingOrder = 1;

		// 宙吊り解除
		dangle = 0.0f;
		dangleVel = 0.0f;

		// 回転を正位置に戻す（宙ぶらりんからの戻りを考慮して）
		transform.localRotation = Quaternion.identity;

		// 移動可能判定
		// 行き先を計算
		float gotoX = Mathf.Round(transform.position.x);
		float gotoY = Mathf.Round(transform.position.y);

		if (Mathf.Abs(gotoX % 2) == 1) {
			gotoX += 1.0f;
		}
		if (Mathf.Abs(gotoY % 2) == 1) {
			gotoY += 1.0f;
		}

		if (this.ExistsPolygon (transform, gotoX, gotoY)) {
			// すでにそこにいたら、もといた場所へ戻る
			transform.position = new Vector3 (potentialX, potentialY, transform.position.z);
		} else {
			// だれもいなかったら、そこへGo
			transform.position = new Vector3 (gotoX, gotoY, transform.position.z);
		}

		// クリア判定の為
		this.setMood();

		// クリア判定
		this.gameIsClear = this.gameClear();

		// dragged解除
		this.isDragged = false;
	}

	/// <summary>
	/// Existses the polygon.
	/// </summary>
	/// <returns><c>true</c>, if polygon was existsed, <c>false</c> otherwise.</returns>
	/// <param name="transform">Transform.</param>
	/// <param name="gotoX">Goto x.</param>
	/// <param name="gotoY">Goto y.</param>
	private bool ExistsPolygon (Transform transform, float gotoX, float gotoY) {
		List<GameObject> polygons = this.polygonGenerator.GetComponent<PolygonGenerator> ().polygons;

		for (int i = 0; i < polygons.Count; i++) {
			if (polygons[i].transform.position.x == gotoX
				&& polygons[i].transform.position.y == gotoY) {
					return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Sets the mood.
	/// </summary>
	private void setMood () {
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

	}

	/// <summary>
	/// Games the clear.
	/// </summary>
	/// <returns><c>true</c>, if clear was gamed, <c>false</c> otherwise.</returns>
	private bool gameClear() {
		List<GameObject> polygons = this.polygonGenerator.GetComponent<PolygonGenerator> ().polygons;
		for (int i = 0; i < polygons.Count; i++) {
			PolygonController polygon = polygons [i].GetComponent<PolygonController> ();
			if (polygon.mood == Mood.Sad) {
				return false;
			}
		}
		return true;
	}
}
