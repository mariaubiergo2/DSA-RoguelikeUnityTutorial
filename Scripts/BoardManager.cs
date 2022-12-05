using System.Collections;
using UnityEngine;
using System.Collections.Generic; //to use lists
using System; //Using serialized to modify how variables will appear
using Random = UnityEngine.Random; //no ambiguitats perque hi ha dos que es diuen així

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count (int min, int max) //to set de values
        {
            minimum = min;
            maximum = max;
        }
    }

    public int columns = 8; //Dimensions of the gameboard
    public int rows = 8;

    public Count wallCount = new Count(5, 9); //random range: min of 5 walls/level and max 9 walls/level
    public Count foodCount = new Count(1, 5); //same for food

    public GameObject exit; //There is only one. to hold the prefabs
    public GameObject[] floorTiles; //to pass multiple objects and then chose one
    public GameObject[] wallTiles; //aqui dins hi hauran els diferents prefabs to choose between in the inspector
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outWallTiles;

    private Transform boardHolder; //to keep hierarchy, it is the father of all the game objects
    private List<Vector3> gridPositions = new List<Vector3>(); //track all different pos in the game. veure si un objecte esta alla o no

    void InitialiseList() 
    {
        gridPositions.Clear();
        //el 1,1 esta abaix a l'esquerra
        for (int x = 1; x<columns-1; x++) //fill the  grids
        {
            for (int y = 1; y<rows-1; y++) //el -1 es per deixar un marge
            {
                gridPositions.Add(new Vector3(x, y, 0f)); //a list of possible possitions of walls, enemies...
            }
        }  
    }

    void BoardSetup () //posar la outer wall i el terra
    {
        boardHolder = new GameObject("Board").transform;

        for (int x = -1; x < columns + 1; x++) //ara va des del -1,-1 que es la cantonada esquerra bottom i fa el edge
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)]; //declaring a variable that is creates indexes
                
                if (x == -1 || x == columns || y == -1 || y == rows) //if we are in the border, per pillar-ne un especific
                    toInstantiate = outWallTiles[Random.Range(0, outWallTiles.Length)];

                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject; 
                //0f es pq es 2D
                //identity es pq no rota

                instance.transform.SetParent(boardHolder);
            }
        }
    }
    //9.22 https://learn.unity.com/tutorial/level-generation?uv=5.x&projectId=5c514a00edbc2a0020694718#5c7f8528edbc2a002053b6f6

    
    Vector3 RandomPosition() //genera posicions random
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex); //per evitar que dos objectes vagin al mateix lloc
        return randomPosition;
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)//per ara si que situarlos
    {
        //Choose a random number of objects to instantiate within the minimum and maximum limits
        int objectCount = Random.Range(minimum, maximum + 1); //quants objectes posarem

        //Instantiate objects until the randomly chosen limit objectCount is reached
        for (int i = 0; i < objectCount; i++)
        {
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();

            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity); //posem el tile en la posicio que diem
        }
    }

    //es la unica publica per setejar el board
    //SetupScene initializes our level and calls the previous functions to lay out the game board
    public void SetupScene(int level)
    {
        //Creates the outer walls and floor.
        BoardSetup();

        //Reset our list of gridpositions.
        InitialiseList();

        //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

        //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

        //Determine number of enemies based on current level number, based on a logarithmic progression
        int enemyCount = (int)Mathf.Log(level, 2f); //posem el numero de enemies en funcio del nivell aquiiii

        //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount); 

        //Instantiate the exit tile in the upper right hand corner of our game board
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity); //sempre posa exit adalt a la dreta
    }

   
}

