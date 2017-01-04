using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonGenerator : MonoBehaviour {

	public GameObject squarePrefab;

	public GameObject trianglePrefab;

	// Use this for initialization
	void Start () {
		this.reset ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void reset () {
		const int Ratio = 8;
		const int TileSize = 1;
		for (int i = -8; i < 10; i = i + 2) {
			for (int j = -4; j < 6; j = j + 2) {
				// 8割の出現率
				if (Random.Range(1, 11) < Ratio) {
					// 1/2の比率で配置
					GameObject polygon = Random.Range(0, 2) < 1 ? Instantiate(trianglePrefab) : Instantiate(squarePrefab);
					float x = (float)i;
					float y = (float)j;
					polygon.transform.position = new Vector3 (x, y, 0f);
				}
			}
		}
	}
}
