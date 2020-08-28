using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Board : MonoBehaviour
{
    public Cell[,] cell_board;
    private bool moved;
    private Dictionary<Tile, Vector3> moveDict;
    private Dictionary<Tile, Vector3> startDict;
    public int moveDuration;
    private bool animating;
    private int elapsedFrames;
    private int colorFrames;
    private List<Vector2> validLocations;

    public GameObject highScoreText;
    public GameObject currentScore;
    public GameObject background;
    public GameObject resetButton;
    public GameObject quitButton;
    private int score;
    private int highScore;
    public float saturation;
    public float brightness;
    public float backgroundSpeed;

    private GameObject endPrefab;
    private GameObject endScreen;

    public bool isPaused;
    private bool newHighScore;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("highscore", highScore);
        highScoreText.GetComponent<TextMesh>().text = highScore.ToString();
        newHighScore = false;
        endPrefab = Resources.Load("Prefabs/End") as GameObject;
        elapsedFrames = 0;
        colorFrames = 0;
        isPaused = false;
        moveDict = new Dictionary<Tile, Vector3>();
        startDict = new Dictionary<Tile, Vector3>();
        validLocations = new List<Vector2>();
        moved = true;
        score = 0;
        print("starting");
        background.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(0, saturation, brightness);
        cell_board = new Cell[4, 4];

        for (int x=0; x <= 3; x++)
        {
            for (int y = 0; y <= 3; y++)
            {
                cell_board[x, y] = new Cell();
            }
        }
        UpdatePositionsNew();
    }

    void Update()
    {
        colorFrames = (colorFrames + 1) % ((int)(360*backgroundSpeed) + 1);
        float color = (float)colorFrames / (360 * backgroundSpeed);

        background.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(color, saturation, brightness);

        if (!animating && !isPaused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                print("Moving Up");
                Move(Util.Direction.Up);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                print("Moving Down");
                Move(Util.Direction.Down);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                print("Moving Left");
                Move(Util.Direction.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                print("Moving Right");
                Move(Util.Direction.Right);
            }
        }
        else if(animating)
        {
            elapsedFrames = elapsedFrames + 1;
            float interpolationRatio = (float)elapsedFrames / moveDuration;
            foreach (Tile t in moveDict.Keys)
            {
                t.obj.transform.position = Vector3.Lerp(startDict[t], moveDict[t], interpolationRatio);
                if ((elapsedFrames == moveDuration) && t.IsDeleted())
                {
                    Destroy(t.obj);
                }
                else if (elapsedFrames == moveDuration)
                {
                    t.clearTracking();
                    t.UpdateValue();
                    if(t.obj.transform.position.z == -6)
                    {

                        t.obj.transform.Translate(new Vector3(0, 0, 1),Space.World);
                    }
                }
            }
            if (elapsedFrames == moveDuration)
            {
                animating = false;
                moveDict.Clear();
                startDict.Clear();
                elapsedFrames = 0;

                if (validLocations.Count > 0)
                {
                    var rand = new System.Random();
                    int index = rand.Next(validLocations.Count);
                    var newLoc = validLocations[index];
                    var cell = cell_board[(int)newLoc.x, (int)newLoc.y];
                    cell.SetTile(new Tile());
                    cell.GetTile().SetPosition(realPosition(newLoc));
                    cell.GetTile().UpdateValue();
                    ValidateWithoutAdding();
                }
            }
        }
    }

    public Board()
    {
        cell_board = new Cell[4, 4];
    }

    public void Move(Util.Direction direction)
    {
        moved = false;
        ResolveMovement(direction);
        ResolveMatches(direction);
        ResolveMovement(direction);
        UpdatePositionsNew();
    }

    private void ResolveMatches(Util.Direction direction)
    {
        (int startX, int startY, int endX, int endY, int dirX, int dirY) =
            ((direction == Util.Direction.Up) || (direction == Util.Direction.Right)) ? (3, 3, -1, -1, -1, -1) : (0, 0, 4, 4, 1, 1);
        int xOffset;
        int yOffset;
        Board newBoard = new Board();
        switch (direction)
        {
            case Util.Direction.Up:
                xOffset = 0;
                yOffset = 1;
                break;
            case Util.Direction.Down:
                xOffset = 0;
                yOffset = -1;
                break;
            case Util.Direction.Left:
                xOffset = -1;
                yOffset = 0;
                break;
            case Util.Direction.Right:
                xOffset = 1;
                yOffset = 0;
                break;
            default:
                xOffset = 0;
                yOffset = 0;
                break;
        }
        for (int x = startX; x != endX; x = x + dirX)
        {
            for (int y = startY; y != endY; y = y + dirY)
            {
                int newX = x + xOffset;
                int newY = y + yOffset;
                if((newX < 0)||(newX >= 4)||(newY < 0)||(newY >= 4))
                {
                    ;
                }
                else
                {
                    if (!cell_board[newX, newY].HasTile() || !cell_board[x, y].HasTile())
                    {
                        ;
                    }
                    else if (cell_board[newX, newY].GetTile().value == cell_board[x, y].GetTile().value)
                    {
                        cell_board[newX, newY].GetTile().Increment();
                        score += cell_board[newX, newY].GetTile().value;
                        currentScore.GetComponent<TextMesh>().text = score.ToString();
                        if(score > highScore)
                        {
                            highScore = score;
                            highScoreText.GetComponent<TextMesh>().text = score.ToString();
                            newHighScore = true;
                        }
                        cell_board[newX, newY].GetTile().SetTracking(cell_board[x, y].GetTile());
                        cell_board[x, y].GetTile().SetDeleted();
                        cell_board[x, y].GetTile().obj.transform.Translate(new Vector3(0, 0, -1),Space.World);
                        startDict.Add(cell_board[x, y].GetTile(), cell_board[x, y].GetTile().obj.transform.position);
                        moveDict.Add(cell_board[x, y].GetTile(), realPosition(new Vector2(newX, newY)));
                        cell_board[x, y].ClearCell();
                        moved = true;
                    }
                }
            }
        }
    }

    private void ResolveMovement(Util.Direction direction)
    {
        (int startX, int startY, int endX, int endY, int dirX, int dirY) =
            ((direction == Util.Direction.Up) || (direction == Util.Direction.Right)) ? (3,3,-1,-1,-1,-1) : (0,0,4,4,1,1);

        for(int x = startX; x != endX; x = x + dirX)
        {
            for(int y = startY; y != endY; y = y + dirY)
            {
                int tempX = x;
                int tempY = y;
                var cell = cell_board[x, y];
                if (cell.HasTile())
                {
                    ;
                }
                else if ((direction == Util.Direction.Left) || (direction == Util.Direction.Right))
                {
                    while ((tempX <= 3) && (tempX >= 0))
                    {
                        var tempCell = cell_board[tempX, y];
                        if (tempCell.HasTile() && (cell != tempCell))
                        {
                            cell.SetTile(tempCell.GetTile());
                            tempCell.ClearCell();
                            moved = true;
                            if (cell.GetTile().IsTracking())
                            {
                                cell.GetTile().ResolveTracking(this, x, y);
                            }
                            break;
                        }
                        tempX = tempX + (dirX);
                    }
                }
                else if ((direction == Util.Direction.Up) || (direction == Util.Direction.Down))
                {
                    while ((tempY <= 3) && (tempY >= 0))
                    {
                        var tempCell = cell_board[x, tempY];
                        if (tempCell.HasTile() && (cell != tempCell))
                        {
                            cell.SetTile(tempCell.GetTile());
                            tempCell.ClearCell();
                            moved = true;
                            if (cell.GetTile().IsTracking())
                            {
                                cell.GetTile().ResolveTracking(this, x, y);
                            }
                            break;
                        }
                        tempY = tempY + (dirY);
                    }
                }
            }
        }

    }

    private void UpdatePositions()
    {
        if (!moved)
        {
            return;
        }
        List<Vector2> validLocations = new List<Vector2>();
        for(int x=0; x<=3; x++)
        {
            for(int y=0; y<=3; y++)
            {
                var cell = cell_board[x, y];
                Vector2 pos = new Vector2(x, y);
                if (cell.HasTile())
                {
                    cell.GetTile().SetPosition(realPosition(pos));
                }
                else
                {
                    validLocations.Add(new Vector2(x, y));
                }
            }
        }

        
    }

    private void UpdatePositionsNew()
    {
        if (!moved)
        {
            return;
        }
        animating = true;
        Validate();
        
    }

    private static Vector3 realPosition(Vector2 position)
    {
        var output = new Vector3(-6.95f + 2.3f * position.x, -3.45f + 2.3f * position.y, -5);
        return output;
    }

    private void Validate()
    {

        validLocations.Clear();
        for (int x = 0; x <= 3; x++)
        {
            for (int y = 0; y <= 3; y++)
            {
                var cell = cell_board[x, y];
                Vector2 pos = new Vector2(x, y);
                if (cell.HasTile())
                {
                    moveDict.Add(cell.GetTile(), realPosition(pos));
                    startDict.Add(cell.GetTile(), cell.GetTile().obj.transform.position);
                }
                else
                {
                    validLocations.Add(new Vector2(x, y));
                }
            }
        }

        if (validLocations.Count == 16)
        {
            animating = false;
            var rand = new System.Random();
            int index = rand.Next(validLocations.Count);
            var newLoc = validLocations[index];
            var cell = cell_board[(int)newLoc.x, (int)newLoc.y];
            cell.SetTile(new Tile());
            cell.GetTile().SetPosition(realPosition(newLoc));
            cell.GetTile().UpdateValue();
        }
        if (validLocations.Count == 0)
        {
            print("checking board");
            var validBoard = false;
            for (int x = 0; x <= 3; x++)
            {
                for (int y = 0; y <= 3; y++)
                {
                    var tile = cell_board[x, y].GetTile();
                    if (y < 3)
                    {
                        var checkTile = cell_board[x, y + 1].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (y > 0)
                    {
                        var checkTile = cell_board[x, y - 1].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (x > 0)
                    {
                        var checkTile = cell_board[x - 1, y].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (x < 3)
                    {
                        var checkTile = cell_board[x + 1, y].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                }
            }
            if (!validBoard)
            {
                print("invalid board!");
                isPaused = true;
                EndScreen();
            }
        }
    }

    private void ValidateWithoutAdding()
    {

        validLocations.Clear();
        for (int x = 0; x <= 3; x++)
        {
            for (int y = 0; y <= 3; y++)
            {
                var cell = cell_board[x, y];
                Vector2 pos = new Vector2(x, y);
                if (cell.HasTile())
                {
                    ;
                }
                else
                {
                    validLocations.Add(new Vector2(x, y));
                }
            }
        }

        if (validLocations.Count == 16)
        {
            animating = false;
            var rand = new System.Random();
            int index = rand.Next(validLocations.Count);
            var newLoc = validLocations[index];
            var cell = cell_board[(int)newLoc.x, (int)newLoc.y];
            cell.SetTile(new Tile());
            cell.GetTile().SetPosition(realPosition(newLoc));
            cell.GetTile().UpdateValue();
        }
        if (validLocations.Count == 0)
        {
            print("checking board");
            var validBoard = false;
            for (int x = 0; x <= 3; x++)
            {
                for (int y = 0; y <= 3; y++)
                {
                    var tile = cell_board[x, y].GetTile();
                    if (y < 3)
                    {
                        var checkTile = cell_board[x, y + 1].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (y > 0)
                    {
                        var checkTile = cell_board[x, y - 1].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (x > 0)
                    {
                        var checkTile = cell_board[x - 1, y].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                    if (x < 3)
                    {
                        var checkTile = cell_board[x + 1, y].GetTile();
                        if (checkTile.value == tile.value)
                        {
                            validBoard = true;
                            x = y = 10;
                            break;
                        }
                    }
                }
            }
            if (!validBoard)
            {
                print("invalid board!");
                isPaused = true;
                EndScreen();
            }
        }
    }

    public void EndScreen()
    {
        Destroy(resetButton);
        Destroy(quitButton);
        endScreen = Instantiate(endPrefab);
        if (newHighScore)
        {
            endScreen.transform.Find("Score").GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void UpdateMoveLocation(Tile t, Vector2 pos)
    {
        moveDict[t] = realPosition(pos);
    }

    void OnDestroy()
    {
        PlayerPrefs.SetInt("highscore", highScore);
        PlayerPrefs.Save();
    }
}
