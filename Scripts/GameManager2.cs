using System.Collections;
using System.Collections.Generic; //USE LISTS TO KEEP TRACK OF ENEMIES
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager2: MonoBehaviour
{
    public float levelStartDelay = 2f;
    public float turnDelay = .1f; //THE GAME WAITS THIS BETWEEN TURNS
    public int playerFoodPoints = 100;

    public static GameManager2 instance = null;     //PER FER SINGLETON
    //Static instance of GameManager which allows it to be accessed by any other script.
    //STATIC: BELONG TO THE CLASS ITSELF
    [HideInInspector] public bool playersTurn = true; //LA VARIABLE ES PUBLICA PERO WONT BE DISPLAYED
    //public bool playersTurn = true;

    private TextMeshProUGUI levelText;                                    //Text to display current level number.
    private GameObject levelImage;                            //Image to block out level as levels are being set up, background for levelText.
    private BoardManager boardScript;  //Store a reference to our BoardManager which will set up the level.
    private int level = 1;     //on testejarem que els enemies appear                         //Current level number, expressed in game as "Day 1".
    private List<Enemy> enemies; //KEEP TRACK
    private bool enemiesMoving;
    private bool doingSetup = true;                            //Boolean to check if we're setting up board, prevent Player from moving during setup.


    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

        //    //if not, set instance to this
            instance = this;

        ////If instance already exists and it's not this:
        else if (instance != this)

        //    //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        ////Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject); //NOT DESTROY THE THINGS TO KEEP BETWEEN SCENES
        enemies = new List<Enemy>();

        ////Get a component reference to the attached BoardManager script
        boardScript = GetComponent<BoardManager>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    //This is called each time a scene is loaded.
    void OnLevelWasLoaded(int index)
    {
        //Add one to our level number.
        level++;
        //Call InitGame to initialize our level.
        InitGame();
    }

    //This is called each time a scene is loaded.
    //void OnSceneLoaded(int index)
    //void OnLevelWasLoaded(int index)
    void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //Add one to our level number.
        instance.level++;
        //Call InitGame to initialize our level.
        instance.InitGame();
    }

    //Initializes the game for each level.
    void InitGame()
    {
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<TextMeshProUGUI>();

        //Set the text of levelText to the string "Day" and append the current level number.
        levelText.text = "Day " + level;
        levelImage.SetActive(true);

        //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
        Invoke("HideLevelImage", levelStartDelay);

        //Call the SetupScene function of the BoardManager script, pass it current level number.
        enemies.Clear(); //bc the manager is not reestarted
        boardScript.SetupScene(level); //fer que faci el nivell 3 

    }

    public void GameOver() //
    {
        levelText.text = "After " + level + " days, you morir-te de gana chavalote"; 
        levelImage.SetActive(true);
        enabled = false; //DISABLE GAME MANAGER
    }

    //Hides black image used between levels
    void HideLevelImage()
    {
        //Disable the levelImage gameObject.
        levelImage.SetActive(false);

        //Set doingSetup to false allowing player to move again.
        doingSetup = false;
    }


    void Update() //MIRAR QUE PASSA
    {
        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
        if (playersTurn || enemiesMoving || doingSetup)

            //If any of these are true, return and do not start MoveEnemies.
            return;

        //Start moving enemies.
        StartCoroutine(MoveEnemies());
    }

    //Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy script)
    {
        //Add Enemy to List enemies.
        enemies.Add(script);
    }


    //Coroutine to move enemies in sequence.
    IEnumerator MoveEnemies()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        //Wait for turnDelay seconds, defaults to .1 (100 ms).
        yield return new WaitForSeconds(turnDelay);

        //If there are no enemies spawned (IE in first level):
        if (enemies.Count == 0)
        {
            //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
            yield return new WaitForSeconds(turnDelay);
        }

        //Loop through List of Enemy objects.
        for (int i = 0; i < enemies.Count; i++) //FERLIS MOURE A CADA ENEMY
        {
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemies[i].MoveEnemy();

            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        //Once Enemies are done moving, set playersTurn to true so player can move.
        playersTurn = true;

        //Enemies are done moving, set enemiesMoving to false.
        enemiesMoving = false;
    }

}
