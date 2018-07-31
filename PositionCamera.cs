using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCamera : MonoBehaviour {

    void Start() {
        float xPos = (Board.MAX_COL - 1f) / 2f;
        float yPos = (Board.MAX_ROW - 1f) / 2f;
        //float zoom = (Map.ROWS >= Map.COLS) ? Map.ROWS / 2f : Map.COLS / 3f;

        this.transform.position = new Vector3(xPos, -yPos, -10f);
        //GetComponent<Camera>().orthographicSize = zoom;
    }
}
