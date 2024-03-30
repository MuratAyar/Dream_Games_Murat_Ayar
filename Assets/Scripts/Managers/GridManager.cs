using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject upperLeftCornerPrefab;
    public GameObject upperRightCornerPrefab;
    public GameObject bottomLeftCornerPrefab;
    public GameObject bottomRightCornerPrefab;

    public GameObject leftSidePrefab;
    public GameObject rightSidePrefab;
    public GameObject topSidePrefab;
    public GameObject bottomSidePrefab;

    public GameObject bodyPrefab;


    public GameObject cubeBlue;
    public GameObject cubeBlueTnt;
    public GameObject cubeGreen;
    public GameObject cubeGreenTnt;
    public GameObject cubeYellow;
    public GameObject cubeYellowTnt;
    public GameObject cubeRed;
    public GameObject cubeRedTnt;

    public GameObject box;
    public GameObject vase;
    public GameObject vaseDamaged;
    public GameObject stone;
    public GameObject tnt;

    private BoardManager boardManager;
    public GameObject boardPrefab;

    public GameObject background;
    public GameObject GameplayUI;

    void Start()
    {
        
    }


    public void GenerateGrid(int width, int height, List<string> gridData, int currentMoveCount)
    {
        GenerateGridBackground(width, height);
        GameObject boardObject = Instantiate(boardPrefab, transform.position, Quaternion.identity);
        boardObject.transform.SetParent(GameplayUI.transform);

        boardObject.name = boardPrefab.name;

        GenerateItems(width, height, gridData);

        boardManager = boardObject.GetComponent<BoardManager>();
        boardManager.UpdateCoordinateDataStart(width, height, currentMoveCount);
    }



    private void GenerateItems(int width, int height, List<string> gridData)
    {
        GameObject board = GameplayUI.transform.Find("Board").gameObject;
        board.SetActive(true);


        if (board == null)
        {
            Debug.LogError("Board GameObject not found in the scene.");
            return;
        }

        Vector2 screenMiddle = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 bottomLeftCellPosition = screenMiddle - new Vector2((width - 1) / 2f, (height - 1) / 2f) * 50;

        int dataIndex = 0;
        for (int row = height - 1; row >= 0; row--)
        {
            for (int col = 0; col < width; col++)
            {
                GameObject prefab = GetPrefabForGridData(gridData[dataIndex]);
                if (prefab != null)
                {
                    Vector3 cellPosition = bottomLeftCellPosition + new Vector2(col, height - 1 - row) * 50;

                    cellPosition.z = row;

                    GameObject instantiatedCube = Instantiate(prefab, cellPosition, Quaternion.identity);
                    instantiatedCube.transform.parent = board.transform;
                    instantiatedCube.transform.localScale = prefab.transform.localScale;

                }

                dataIndex++;
                if (dataIndex >= gridData.Count)
                    return;
            }
        }
    }

    private GameObject GetPrefabForGridData(string gridDatum)
    {
        switch (gridDatum)
        {
            case "r": return cubeRed;
            case "g": return cubeGreen;
            case "b": return cubeBlue;
            case "y": return cubeYellow;
            case "rand":
                string randomColor = Random.Range(0, 4) switch
                {
                    0 => "r",
                    1 => "g",
                    2 => "b",
                    3 => "y",
                    _ => "r", 
                };
                return GetPrefabForGridData(randomColor);
            case "t": return tnt;
            case "bo": return box;
            case "s": return stone;
            case "v":
                return Random.Range(0, 2) == 0 ? vase : vaseDamaged;
            default: return null;
        }
    }

    private void GenerateGridBackground(int width, int height)
    {
        
        Vector2 screenMiddle = new Vector2(Screen.width / 2f, Screen.height / 2f);


        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float offsetX = col - (width - 1) / 2f; 
                float offsetY = (height - 1) / 2f - row;
                Vector3 cellPosition = screenMiddle + new Vector2(offsetX, offsetY) * 50;

                GameObject prefab = GetPrefabForGridCell(row, col, width, height);
                if (prefab != null)
                {
                    Instantiate(prefab, cellPosition, Quaternion.identity, background.transform);
                }
            }
        }
    }

    private GameObject GetPrefabForGridCell(int row, int col, int width, int height)
    {
        if ((row == 0 && col == 0) || (row == 0 && col == width - 1) || (row == height - 1 && col == 0) || (row == height - 1 && col == width - 1))
        {
            if (row == 0 && col == 0) return upperLeftCornerPrefab;
            if (row == 0 && col == width - 1) return upperRightCornerPrefab;
            if (row == height - 1 && col == 0) return bottomLeftCornerPrefab;
            if (row == height - 1 && col == width - 1) return bottomRightCornerPrefab;
        }
        else if (row == 0 || row == height - 1 || col == 0 || col == width - 1)
        {
            if (row == 0) return topSidePrefab;
            if (row == height - 1) return bottomSidePrefab;
            if (col == 0) return leftSidePrefab;
            if (col == width - 1) return rightSidePrefab;
        }
        else
        {
            return bodyPrefab;
        }

        return null;
    }
}