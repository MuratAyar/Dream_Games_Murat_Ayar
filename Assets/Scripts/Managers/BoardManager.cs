using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


public class BoardManager : MonoBehaviour
{
    private GameObject background, victoryPopup, defeatPopup, mainMenuUI, gamePlayUI, showLevelsUI, settingsPopup, exitPopup, infoPopup, Board;
    private GameObject boardObject;

    public GameObject completeMark;

    public float cellSizeX = 1.42f;
    public float cellSizeY = 1.62f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;

    public GameObject cubeBlue;
    public GameObject cubeGreen;
    public GameObject cubeYellow;
    public GameObject cubeRed;

    public GameObject cubeBlueTnt;
    public GameObject cubeGreenTnt;
    public GameObject cubeYellowTnt;
    public GameObject cubeRedTnt;

    public GameObject tnt;
    public GameObject vaseDamaged;

    private int colNum;
    private int rowNum;
    private int moveCount;

    Transform boxShapeLeftTransform, stoneShapeLeftTransform, vaseShapeLeftTransform;

    public Dictionary<Vector2Int, Vector3> coordinateData = new Dictionary<Vector2Int, Vector3>();
    public Dictionary<Vector2Int, GameObject> coordinateObjectData = new Dictionary<Vector2Int, GameObject>();

    bool isLevelFinished = false;

    void Start()
    {
        UpdateCoordinateDataStart(colNum, rowNum, moveCount);
    }

    public void ResetGame()
    {
        coordinateObjectData.Clear();
        isLevelFinished = false;
        GameObject gameplayUI = GameObject.Find("GameplayUI");
        
        Transform stonesLeftTransform = gameplayUI.transform.Find("Header/Image/GoalsLeft/StonesLeft");
        Transform boxesLeftTransform = gameplayUI.transform.Find("Header/Image/GoalsLeft/BoxesLeft");
        Transform vasesLeftTransform = gameplayUI.transform.Find("Header/Image/GoalsLeft/VasesLeft");

        if (stonesLeftTransform != null)
        {
            foreach (Transform child in stonesLeftTransform)
            {
                if (child.name == "completeMark(Clone)")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        if (boxesLeftTransform != null)
        {
            foreach (Transform child in boxesLeftTransform)
            {
                if (child.name == "completeMark(Clone)")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        if (vasesLeftTransform != null)
        {
            foreach (Transform child in vasesLeftTransform)
            {
                if (child.name == "completeMark(Clone)")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
    private void checkGameFinish()
    {
        UpdateCoordinateDataStart(colNum, rowNum, moveCount);
        UpdateGoalsLeftText();
        isLevelFinished = true;
        foreach (var entry in coordinateObjectData)
        {
            if (entry.Value.CompareTag("Box") || entry.Value.CompareTag("Stone") || entry.Value.CompareTag("Vase") || entry.Value.CompareTag("VaseDamaged"))
            {
                isLevelFinished = false;
                break;
            }
        }

        if (isLevelFinished)
        {
            Debug.Log("Oyun bitti!");
            GameObject gameplayUI = GameObject.Find("GameplayUI");
            GameObject victoryPopup = gameplayUI.transform.Find("VictoryPopup").gameObject;
            GameObject background = gameplayUI.transform.Find("Background").gameObject;
            GameObject board = gameplayUI.transform.Find("Board").gameObject;
            SetUIActive(victoryPopup, true);
            SetUIActive(background, false);
            SetUIActive(board, false);
            //SetUIActive(victoryPopup, true);
            /*
            boxShapeLeftTransform.gameObject.SetActive(true);
            stoneShapeLeftTransform.gameObject.SetActive(true);
            vaseShapeLeftTransform.gameObject.SetActive(true);
            */
            ResetGame();
        }
        bool defeat = isDefeat();
        if (defeat)
        {
            Debug.Log("Defeated, out of moves :(");
            GameObject gameplayUI = GameObject.Find("GameplayUI");
            GameObject defeatPopup = gameplayUI.transform.Find("DefeatPopup").gameObject;
            GameObject background = gameplayUI.transform.Find("Background").gameObject;
            GameObject board = gameplayUI.transform.Find("Board").gameObject;
            SetUIActive(background, false);
            SetUIActive(board, false);
            SetUIActive(defeatPopup, true);
        }
    }

    public void UpdateCoordinateDataStart(int width, int height, int currentMoveCount)
    {
        moveCount = currentMoveCount;
        colNum = width;
        rowNum = height;

        isLevelFinished = false;
        if ((coordinateObjectData == null || coordinateObjectData.Count == 0) && (coordinateData == null || coordinateData.Count == 0))
        {
            //Debug.Log("UpdateCoordinateDataStart  IS RUNNING FOR FIRST TIME");
            GameObject gamePlayUI = GameObject.Find("GameplayUI");
            if (gamePlayUI != null)
            {
                Transform board = gamePlayUI.transform.Find("Board");
                if (board != null)
                {
                    int index = 0;
                    foreach (Transform child in board)
                    {
                        GameObject obj = child.gameObject;
                        float x = obj.transform.position.x;
                        float y = obj.transform.position.y;
                        float z = obj.transform.position.z;
                        Vector2Int position = new Vector2Int(index % width, index / width);
                        coordinateData[position] = new Vector3(x, y, z);
                        coordinateObjectData[position] = obj;
                        index++;
                    }
                }
            }
            
        }
        else
        {
            //Debug.Log("UpdateCoordinateDataStart  IS RUNNING FOR LATER...");
            GameObject gamePlayUI = GameObject.Find("GameplayUI");
            if (gamePlayUI != null)
            {
                Transform board = gamePlayUI.transform.Find("Board");
                if (board != null)
                {
                    foreach (Transform child in board)
                    {
                        GameObject obj = child.gameObject;


                        if (obj.activeSelf)
                        {
                            float x = obj.transform.position.x;
                            float y = obj.transform.position.y;
                            float z = obj.transform.position.z;
                            Vector3 position = new Vector3(x, y, z);

                            foreach (var entry in coordinateData)
                            {

                                if (entry.Value == position)
                                {
                                    coordinateObjectData[entry.Key] = obj;

                                }
                            }
                        }
                    }
                }
            }
        }
        ConvertAdjacentCubesToTntState();
        PrintCoordinateData();
        UpdateMoveLeftText();
        UpdateGoalsLeftText();
    }

    public void PrintCoordinateData()
    {
        foreach (var entry in coordinateData)
        {
            Vector2Int position = entry.Key;
            Vector2 coordinates = entry.Value;
            //Debug.Log("CoordinateData[" + position.x + "," + position.y + "] = (" + coordinates.x + ", " + coordinates.y + ", object: " + coordinateObjectData[position]);
        }
    }

    //COUNTING ADJACENT CUBES ------------------------------
    public int NumberOfAdjacent(int col, int row)
    {
        Vector2Int position = new Vector2Int(col, row);
        GameObject cube = coordinateObjectData[position];
        string color = GetCubeColor(cube);

        if (cube == null || color == null)
        {
            return 0;
        }

        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        int count = CountAdjacentCubes(position, color, visitedPositions);

        return count;
    }

    private int CountAdjacentCubes(Vector2Int position, string color, HashSet<Vector2Int> visitedPositions)
    {
        visitedPositions.Add(position);

        int count = 1; 

        Vector2Int[] directions = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = position + direction;

            if (coordinateObjectData.ContainsKey(neighborPosition) && !visitedPositions.Contains(neighborPosition))
            {
                GameObject neighborCube = coordinateObjectData[neighborPosition];

                if (GetCubeColor(neighborCube) == color)
                {
                    count += CountAdjacentCubes(neighborPosition, color, visitedPositions);
                }
            }
        }

        return count;
    }

    // GET CUBE COLOR -------------------------------
    private string GetCubeColor(GameObject cube)
    {
        string prefabName = cube.name;
        string[] parts = prefabName.Split('_');
        string[] parts2 = parts[1].Split('(');
        string name = parts2[0];
        return name;
    }

    public Vector3 GetCoordinateDataFromXYPosition(Vector2Int xyPosition)
    {
        return coordinateData[xyPosition];
    }

    public Vector2Int GetXYPositionFromCoordinate(Vector2 xyPosition)
    {
        foreach (var entry in coordinateData)
        {
            Vector2 entryValue = new Vector2(entry.Value.x, entry.Value.y);
            if (Mathf.Approximately(entryValue.x, xyPosition.x) && Mathf.Approximately(entryValue.y, xyPosition.y))
            {
                return entry.Key;
            }
        }
        
        return new Vector2Int(-1, -1);
    }

    public Vector2Int GetCubePosition(GameObject cube)
    {
        foreach (var entry in coordinateObjectData)
        {
            if (entry.Value == cube)
            {
                return entry.Key;
            }
        }
        return new Vector2Int(-1, -1);
    }

    private int PopAdjacentCubes(Vector2Int position, string color, HashSet<Vector2Int> visitedPositions)
    {
        visitedPositions.Add(position);

        int count = 1;

        Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = position + direction;

            if (coordinateObjectData.ContainsKey(neighborPosition) && !visitedPositions.Contains(neighborPosition))
            {
                GameObject neighborCube = coordinateObjectData[neighborPosition];

                if (GetCubeColor(neighborCube) == color)
                {
                    //Debug.Log("Popping neighbor cube at position: " + neighborPosition);

                    count += PopAdjacentCubes(neighborPosition, color, visitedPositions);

                    Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                    neighborCube.transform.SetParent(newParent);
                    Destroy(neighborCube);

                    coordinateObjectData[neighborPosition] = null;

                    CheckForObstacleAndDamage(neighborPosition);
                }
            }
        }

        return count;
    }

    public void PopCubes(Vector2Int position)
    {
        //Debug.Log("Popping cube at position: " + position);

        if (coordinateObjectData.TryGetValue(position, out GameObject cube))
        {
            string color = GetCubeColor(cube);

            if (cube == null || color == null)
            {
                //Debug.LogError("Cube or color is null.");
                return;
            }

            HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

            int count = PopAdjacentCubes(position, color, visitedPositions);

            //checkForObstacleAndDamage(position);

            Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
            cube.transform.SetParent(newParent);
            Destroy(cube);

            coordinateObjectData[position] = null;

            CheckForObstacleAndDamage(position);

            UpdateGameBoard();
            UpdateCoordinateDataStart(colNum, rowNum, moveCount - 1);
            //PrintCoordinateData();
            checkGameFinish();
        }
    }

    public void UpdateGameBoard()
    {
        for (int row = 0; row < rowNum - 1; row++)
        {
            for (int col = 0; col < colNum; col++)
            {
                Vector2Int currentPosition = new Vector2Int(col, row);

                if (coordinateObjectData[currentPosition] == null)
                {
                    Vector2Int upperPosition = FindUpperPosition(currentPosition);

                    if (upperPosition != Vector2Int.zero)
                    {
                        GameObject cubeToMove = coordinateObjectData[upperPosition];

                        Vector3 newPosition = coordinateData[new Vector2Int(currentPosition.x, currentPosition.y)];
                        //Debug.Log("Moving cube to position: " + newPosition);
                        cubeToMove.transform.position = newPosition;

                        coordinateObjectData[currentPosition] = cubeToMove;
                        coordinateObjectData[upperPosition] = null;
                    }
                }
            }
        }
        RandomCubesFall();
    }

    private Vector2Int FindUpperPosition(Vector2Int currentPosition)
    {
        for (int row = currentPosition.y + 1; row < rowNum; row++)
        {
            Vector2Int position = new Vector2Int(currentPosition.x, row);
            if (coordinateObjectData[position] != null)
            {
                return position;
            }
        }
        return Vector2Int.zero; 
    }

    private void RandomCubesFall()
    {
        GameObject gamePlayUI = GameObject.Find("GameplayUI");

        if (gamePlayUI != null)
        {
            Transform board = gamePlayUI.transform.Find("Board");

            if (board != null)
            {
                for (int row = 0; row < rowNum; row++)
                {
                    for (int col = 0; col < colNum; col++) 
                    {
                        Vector2Int currentPosition = new Vector2Int(col, row);

                        if (coordinateObjectData[currentPosition] == null)
                        {
                            GameObject cubePrefab = GetRandomCubePrefab();

                            Vector3 xyzValOfTransferPosition = coordinateData[new Vector2Int(currentPosition.x, currentPosition.y)];
                            GameObject newCube = Instantiate(cubePrefab, xyzValOfTransferPosition, Quaternion.identity, board);
                            //Debug.Log("FALLED TO Y: " + xyzValOfTransferPosition.y + " X: " + xyzValOfTransferPosition.x + " COLOR: " + newCube.name);

                            coordinateObjectData[currentPosition] = newCube;
                        }
                    }
                }
            }
        }
    }

    private GameObject GetRandomCubePrefab()
    {
        GameObject[] cubePrefabs = { cubeYellow, cubeGreen, cubeRed, cubeBlue };

        int randomIndex = UnityEngine.Random.Range(0, cubePrefabs.Length);
        return cubePrefabs[randomIndex];
    }

    //--------------------------------------------------------- TNT --------------
    private Boolean isTntState(Vector2Int position)
    {
        int adjacentNumber = NumberOfAdjacent(position.x, position.y);
        if(adjacentNumber >= 5)
        {
            return true;
        }
        return false;
    }


    private int ConvertToTntState(Vector2Int position, string color, HashSet<Vector2Int> visitedPositions)
    {
        visitedPositions.Add(position);

        int count = 0; 

        Vector2Int[] directions = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = position + direction;

            if (coordinateObjectData.ContainsKey(neighborPosition) && !visitedPositions.Contains(neighborPosition))
            {
                GameObject neighborCube = coordinateObjectData[neighborPosition];

                if (neighborCube != null && neighborCube.CompareTag("Cube") && GetCubeColor(neighborCube) == color)
                {
                    count += ConvertToTntState(neighborPosition, color, visitedPositions);
                }
            }
        }

        GameObject cube = coordinateObjectData[position];
        GameObject tntStatePrefab = GetTntStatePrefabForColor(color); 

        GameObject tntStateObject = Instantiate(tntStatePrefab, cube.transform.position, Quaternion.identity, cube.transform.parent);
        tntStateObject.name += "(Color)";

        Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
        cube.transform.SetParent(newParent);
        Destroy(cube); 
        coordinateObjectData[position] = tntStateObject;

        return count + 1; 
    }

    public void ConvertAdjacentCubesToTntState()
    {
        List<Vector2Int> cubesToConvert = new List<Vector2Int>(); 
        List<Vector2Int> cubesToConvertTntToNormal = new List<Vector2Int>(); 

        HashSet<Vector2Int> visitedTntStatePositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
        int totalCount = 0; 

       
        foreach (var entry in coordinateObjectData)
        {
            if (entry.Value.CompareTag("TntCube"))
            {
                Vector2Int position = entry.Key;
                GameObject cube = entry.Value;

                if (cube != null)
                {
                    cubesToConvertTntToNormal.Add(position);
                }
            }
        }
        foreach(Vector2Int position in cubesToConvertTntToNormal)
        {
            ConvertTntStateToNormal(position);
        }


        foreach (var entry in coordinateObjectData)
        {
            if (entry.Value.CompareTag("Cube"))
            {
                Vector2Int position = entry.Key;
                GameObject cube = entry.Value;

                if (cube != null)
                {
                    string color = GetCubeColor(cube);
                    if (color != null && isTntState(position) && !visitedPositions.Contains(position))
                    {
                        cubesToConvert.Add(position);
                    }
                }
            }
        }
        foreach (Vector2Int position in cubesToConvert)
        {
            if (!visitedPositions.Contains(position))
            {
                int count = ConvertToTntState(position, GetCubeColor(coordinateObjectData[position]), visitedPositions);
                totalCount += count;
                //Debug.Log("Converted " + count + " adjacent cubes to TNT with color " + GetCubeColor(coordinateObjectData[position]));
            }
        }

        //Debug.Log("Total cubes converted to TNT: " + totalCount);
    }

    private void ConvertTntStateToNormal(Vector2Int position)
    {
        GameObject currentTntState = coordinateObjectData[position];
        GameObject cubePrefab = GetCubePrefabForColor(GetCubeColor(currentTntState));

        GameObject cubeObject = Instantiate(cubePrefab, currentTntState.transform.position, Quaternion.identity, currentTntState.transform.parent);


        Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform;
        currentTntState.transform.SetParent(newParent);
        Destroy(currentTntState);

        coordinateObjectData[position] = cubeObject;

    }

    private GameObject GetCubePrefabForColor(string color)
    {
        switch (color)
        {
            case "blueTnt":
                return cubeBlue;
            case "greenTnt":
                return cubeGreen;
            case "yellowTnt":
                return cubeYellow;
            case "redTnt":
                return cubeRed;
        }
        return null;
    }

    private GameObject GetTntStatePrefabForColor(string color)
    {
        switch (color)
        {
            case "blue":
               return cubeBlueTnt;
            case "green":
                return cubeGreenTnt;
            case "yellow":
                return cubeYellowTnt;
            case "red":
                return cubeRedTnt;
        }
        return null;
    }

    public void ConvertTntStateToTnt(Vector2Int position)
    {
        GameObject clickedTntStatePrefab = coordinateObjectData[position];
        
        DestroyAllAdjacentAndSameColorTntStateCubes(position);

        UpdateGameBoard();
        UpdateCoordinateDataStart(colNum, rowNum, moveCount - 1);
    }

    public void DestroyAllAdjacentAndSameColorTntStateCubes(Vector2Int position)
    {
        GameObject clickedTntStatePrefab = coordinateObjectData[position];
        string color = GetCubeColor(clickedTntStatePrefab);

        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        DestroyAdjacentAndSameColorTntStateCubes(position, color, visitedPositions);

        Transform newParent = GameObject.Find("GameplayUI/Board").transform; 

        GameObject tntPrefab = tnt;
        GameObject tntObject = Instantiate(tntPrefab, GetCoordinateDataFromXYPosition(position), Quaternion.identity, newParent);

        coordinateObjectData[position] = tntObject;
    }

    private void DestroyAdjacentAndSameColorTntStateCubes(Vector2Int position, string color, HashSet<Vector2Int> visitedPositions)
    {
        visitedPositions.Add(position);

        Vector2Int[] directions = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = position + direction;

            if (coordinateObjectData.ContainsKey(neighborPosition))
            {
                GameObject neighborTntStatePrefab = coordinateObjectData[neighborPosition];

                if (neighborTntStatePrefab != null && neighborTntStatePrefab.CompareTag("TntCube") && GetCubeColor(neighborTntStatePrefab) == color)
                {
                    if (!visitedPositions.Contains(neighborPosition))
                    {
                        DestroyAdjacentAndSameColorTntStateCubes(neighborPosition, color, visitedPositions);
                    }

                    Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                    neighborTntStatePrefab.transform.SetParent(newParent);
                    Destroy(neighborTntStatePrefab);

                    coordinateObjectData[neighborPosition] = null;
                }
            }
        }
    }

    private void CheckForObstacleAndDamage(Vector2Int position)
    {
        Vector2Int[] directions = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = position + direction;

            if (coordinateObjectData.ContainsKey(neighborPosition) && coordinateObjectData[neighborPosition] != null && (coordinateObjectData[neighborPosition].CompareTag("Box") || coordinateObjectData[neighborPosition].CompareTag("VaseDamaged")))
            {
                GameObject deletingBox = coordinateObjectData[neighborPosition];

                Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                deletingBox.transform.SetParent(newParent);
                Destroy(deletingBox);

                coordinateObjectData[neighborPosition] = null;
            }
            else if (coordinateObjectData.ContainsKey(neighborPosition) && coordinateObjectData[neighborPosition] != null && coordinateObjectData[neighborPosition].CompareTag("Vase"))
            {
                GameObject deletingBox = coordinateObjectData[neighborPosition];
                Transform deletingObjectParent = deletingBox.transform.parent;
                coordinateObjectData[neighborPosition] = null;
                Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 

                GameObject newVase = Instantiate(vaseDamaged, deletingBox.transform.position, Quaternion.identity, deletingObjectParent);
                deletingBox.transform.SetParent(newParent);

                Destroy(deletingBox);
                coordinateObjectData[neighborPosition] = newVase;
            }
            
        }
    }

    public void BlowUpTnt(Vector2Int position)
    {
        Vector2Int[] neighborDirections = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        foreach(Vector2Int direction in neighborDirections)
        {
            Vector2Int neighborPosition = position + direction;
            if(coordinateObjectData.ContainsKey(neighborPosition) && coordinateObjectData[neighborPosition] != null && coordinateObjectData[neighborPosition].CompareTag("Tnt"))
            {
                BlowUpTntCombo(position);
            }
        }

        int explosionRange = 2;
        for (int x = -explosionRange; x <= explosionRange; x++)
        {
            for (int y = -explosionRange; y <= explosionRange; y++)
            {
                Vector2Int targetPosition = position + new Vector2Int(x, y);
                if (coordinateObjectData.ContainsKey(targetPosition) && coordinateObjectData[targetPosition] != null)
                {
                    GameObject deletingObject = coordinateObjectData[targetPosition];

                    if (deletingObject.CompareTag("TntCube"))
                    {
                        BlowUpSingleTnt(targetPosition);
                    }
                    else
                    {
                        Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                        deletingObject.transform.SetParent(newParent);

                        Destroy(deletingObject);
                        coordinateObjectData[targetPosition] = null;
                    }

                }
            }
        }

        UpdateGameBoard();
        UpdateCoordinateDataStart(colNum, rowNum, moveCount - 1);
        checkGameFinish();
    }

    private void BlowUpSingleTnt(Vector2Int position)
    {
        int explosionRange = 2;
        for (int x = -explosionRange; x <= explosionRange; x++)
        {
            for (int y = -explosionRange; y <= explosionRange; y++)
            {
                Vector2Int targetPosition = position + new Vector2Int(x, y);
                if (coordinateObjectData.ContainsKey(targetPosition) && coordinateObjectData[targetPosition] != null)
                {
                    GameObject deletingObject = coordinateObjectData[targetPosition];

                    if (deletingObject.CompareTag("TntCube"))
                    {

                    }
                    else
                    {
                        Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                        deletingObject.transform.SetParent(newParent);

                        Destroy(deletingObject);
                        coordinateObjectData[targetPosition] = null;
                    }
                }
            }
        }
    }

    private void BlowUpTntCombo(Vector2Int position)
    {
        int explosionRange = 3; 

        for (int x = -explosionRange; x <= explosionRange; x++)
        {
            for (int y = -explosionRange; y <= explosionRange; y++)
            {
                Vector2Int targetPosition = position + new Vector2Int(x, y);
                if (coordinateObjectData.ContainsKey(targetPosition) && coordinateObjectData[targetPosition] != null)
                {
                    GameObject deletingObject = coordinateObjectData[targetPosition];

                    if (deletingObject.CompareTag("TntCube"))
                    {
                        BlowUpSingleTnt(targetPosition);
                    }
                    else
                    {
                        Transform newParent = GameObject.Find("GameplayUI/GarbageCollector").transform; 
                        deletingObject.transform.SetParent(newParent);

                        Destroy(deletingObject);
                        coordinateObjectData[targetPosition] = null;
                    }
                }
            }
        }
    }

    void SetUIActive(GameObject uiObject, bool active)
    {
        if (uiObject != null)
        {
            uiObject.SetActive(active);
        }
    }

    void UpdateMoveLeftText()
    {
        GameObject gameplayUI = GameObject.Find("GameplayUI");

        if (gameplayUI != null)
        {
            Transform moveLeftTransform = gameplayUI.transform.Find("Header/Image/MoveLeftText");

            if (moveLeftTransform != null)
            {
                TextMeshProUGUI moveLeftText = moveLeftTransform.GetComponent<TextMeshProUGUI>();

                if (moveLeftText != null)
                {
                    moveLeftText.text = moveCount.ToString();
                }
            }
        }
    }

    void UpdateGoalsLeftText()
    {
        bool isBox = false;
        bool isVase = false;
        bool isStone = false;

        int boxCount = 0;
        int vaseCount = 0;
        int stoneCount = 0;

        GameObject gameplayUI = GameObject.Find("GameplayUI");

        if (gameplayUI != null)
        {
            Transform goalsLeftTransform = gameplayUI.transform.Find("Header/Image/GoalsLeft");
            foreach(var entry in coordinateObjectData)
            {
                if (entry.Value.CompareTag("Box"))
                {
                    boxCount += 1;
                    isBox = true;
                }else if (entry.Value.CompareTag("Vase") || entry.Value.CompareTag("VaseDamaged"))
                {
                    vaseCount += 1;
                    isVase = true;
                }
                else if (entry.Value.CompareTag("Stone"))
                {
                    stoneCount += 1;
                    isStone = true;
                }
            }

            Transform boxLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft/Text (TMP)");
            Transform stoneLeftTransform = goalsLeftTransform.transform.Find("StonesLeft/Text (TMP) (1)");
            Transform vaseLeftTransform = goalsLeftTransform.transform.Find("VasesLeft/Text (TMP)");

            if(isBox && !isStone && !isVase)
            {
                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");
                boxShapeLeftTransform.transform.position = goalsLeftTransform.transform.position;

                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                stoneShapeLeftTransform.gameObject.SetActive(false);
                vaseShapeLeftTransform.gameObject.SetActive(false);
            }else if (isBox && isStone && !isVase)
            {
                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");
                boxShapeLeftTransform.transform.position = goalsLeftTransform.transform.position + new Vector3(-50, 0, 0);

                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                stoneShapeLeftTransform.transform.position = goalsLeftTransform.transform.position + new Vector3(50, 0, 0);

                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                vaseShapeLeftTransform.gameObject.SetActive(false);
            }
            else if (!isBox && isStone && !isVase)
            {
                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");

                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                stoneShapeLeftTransform.transform.position = goalsLeftTransform.transform.position;

                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                boxShapeLeftTransform.gameObject.SetActive(false);
                vaseShapeLeftTransform.gameObject.SetActive(false);
            }
            else if (!isBox && !isStone && isVase)
            {
                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                vaseShapeLeftTransform.transform.position = goalsLeftTransform.transform.position;

                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");
                stoneShapeLeftTransform.gameObject.SetActive(false);
                boxShapeLeftTransform.gameObject.SetActive(false);
            }
            else if (isBox && isStone && isVase)
            {
                
                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                vaseShapeLeftTransform.transform.position = goalsLeftTransform.transform.position + new Vector3(-50, 15, 0);

                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                stoneShapeLeftTransform.transform.position = goalsLeftTransform.transform.position + new Vector3(-50, 15, 0);

                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");
                boxShapeLeftTransform.transform.position = goalsLeftTransform.transform.position + new Vector3(0, -25, 0);
                
            }
            //-------------------------------------------------------------------------------------------------
            if (boxCount == 0)
            {
                boxShapeLeftTransform = goalsLeftTransform.transform.Find("BoxesLeft");
                GameObject completeMarkInstance = Instantiate(completeMark, boxLeftTransform.transform.position, Quaternion.identity);
                completeMarkInstance.transform.SetParent(boxShapeLeftTransform);
                boxLeftTransform.gameObject.SetActive(false);
            }
            else
            {
                TextMeshProUGUI boxLeftText = boxLeftTransform.GetComponent<TextMeshProUGUI>();
                boxLeftText.text = boxCount.ToString();
            }

            if (stoneCount == 0)
            {
                Debug.Log("stoneCount = 0");
                stoneShapeLeftTransform = goalsLeftTransform.transform.Find("StonesLeft");
                GameObject completeMarkInstance = Instantiate(completeMark, stoneLeftTransform.transform.position, Quaternion.identity);
                completeMarkInstance.transform.SetParent(stoneShapeLeftTransform);
                completeMarkInstance.gameObject.SetActive(true);
                //.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("stoneCount != 0");
                TextMeshProUGUI stoneLeftText = stoneLeftTransform.GetComponent<TextMeshProUGUI>();
                stoneLeftText.text = stoneCount.ToString();
                stoneLeftTransform.gameObject.SetActive(true);
            }

            if (vaseCount == 0)
            {
                Debug.Log("vaseCount = 0");
                vaseShapeLeftTransform = goalsLeftTransform.transform.Find("VasesLeft");
                GameObject completeMarkInstance = Instantiate(completeMark, vaseLeftTransform.transform.position, Quaternion.identity);
                completeMarkInstance.transform.SetParent(vaseShapeLeftTransform);
                completeMarkInstance.gameObject.SetActive(true);
                //.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("vaseCount != 0");
                TextMeshProUGUI vaseLeftText = vaseLeftTransform.GetComponent<TextMeshProUGUI>();
                vaseLeftText.text = vaseCount.ToString();
                vaseLeftTransform.gameObject.SetActive(true);
            }

        }
    }

    private bool isDefeat()
    {
        if(moveCount <= 0)
        {
            return true;
        }
        return false;
    }
}
