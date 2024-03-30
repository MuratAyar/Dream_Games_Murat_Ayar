using UnityEngine;

public class RaycastReview : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

            if (hit.collider != null)
            {
                //Debug.Log("Hit object name: " + hit.collider.gameObject.name);
                //Debug.Log("Hit object tag: " + hit.collider.gameObject.tag);
                //Debug.Log("Hit point: " + hit.point);
                //sDebug.Log("Hit normal: " + hit.normal);

                if (hit.collider.CompareTag("Cube"))
                {
                    //Debug.Log("Clicked on cube: " + hit.collider.gameObject.name);

                    BoardManager boardManager = hit.collider.GetComponentInParent<BoardManager>();
                    //boardManager.UpdateCoordinateDataStart();

                    
                    Vector2 clickedPosition = hit.collider.transform.position;
                    //Debug.Log("Clicked object: " + hit.collider.name);

                    //clickedPosition = clickedPosition - new Vector2(0,450);
                    //Debug.Log("Clicked cube position: " + clickedPosition);

                    if (boardManager != null)
                    {
                        //Debug.Log("clicked to: " + clickedPosition);
                        Vector2Int coordinateData = boardManager.GetXYPositionFromCoordinate(clickedPosition);
                        //Debug.Log("Clicked cube's coordinate: " + coordinateData);

                        
                        if(boardManager.NumberOfAdjacent(coordinateData.x, coordinateData.y) >= 2)
                        {
                            boardManager.PopCubes(coordinateData);
                        }
                        
                    }
                    else
                    {
                        //Debug.LogError("CubeManager component not found on clicked cube.");
                    }
                }
                else if (hit.collider.CompareTag("TntCube"))
                {
                    //Debug.Log("Clicked on cube: " + hit.collider.gameObject.name);

                    BoardManager boardManager = hit.collider.GetComponentInParent<BoardManager>();
                    //boardManager.UpdateCoordinateDataStart();


                    Vector2 clickedPosition = hit.collider.transform.position;
                    //Debug.Log("Clicked object: " + hit.collider.name);

                    //clickedPosition = clickedPosition - new Vector2(0,450);
                    //Debug.Log("Clicked cube position: " + clickedPosition);

                    if (boardManager != null)
                    {
                        //Debug.Log("clicked to: " + clickedPosition);
                        Vector2Int coordinateData = boardManager.GetXYPositionFromCoordinate(clickedPosition);
                        //Debug.Log("Clicked cube's coordinate: " + coordinateData);

                        boardManager.ConvertTntStateToTnt(coordinateData);

                    }
                    else
                    {
                        //Debug.LogError("CubeManager component not found on clicked cube.");
                    }
                }
                else if (hit.collider.CompareTag("Tnt"))
                {
                    //Debug.Log("Clicked on cube: " + hit.collider.gameObject.name);

                    BoardManager boardManager = hit.collider.GetComponentInParent<BoardManager>();
                    //boardManager.UpdateCoordinateDataStart();


                    Vector2 clickedPosition = hit.collider.transform.position;
                    //Debug.Log("Clicked object: " + hit.collider.name);

                    //clickedPosition = clickedPosition - new Vector2(0,450);
                    //Debug.Log("Clicked cube position: " + clickedPosition);

                    if (boardManager != null)
                    {
                        //Debug.Log("clicked to: " + clickedPosition);
                        Vector2Int coordinateData = boardManager.GetXYPositionFromCoordinate(clickedPosition);
                        //Debug.Log("Clicked cube's coordinate: " + coordinateData);

                        boardManager.BlowUpTnt(coordinateData);

                    }
                    else
                    {
                        //Debug.LogError("CubeManager component not found on clicked cube.");
                    }
                }
            }
            else
            {
                //Debug.Log("No object hit by the raycast.");
            }
        }
    }
}