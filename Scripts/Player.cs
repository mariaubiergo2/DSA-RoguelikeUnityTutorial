using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MovingObject //ARA ES FILLETA DEL MOVIMENT
{
    public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.
    public int pointsPerFood = 10;     //LO QUE GANYES QUAN PILLES FOOD           //Number of points to add to player food points when picking up a food object.
    public int pointsPerSoda = 20;    //LO QUE GANYES QUAN PILLES SODA            //Number of points to add to player food points when picking up a soda object.
    public int wallDamage = 1;     //DAMAGE QUE LI FOTS A LA WALL               //How much damage a player does to a wall when chopping it.

    public TextMeshProUGUI foodText;                       //UI Text to display current player food total.
    public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
    public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
    public AudioClip eatSound1;                    //1 of 2 Audio clips to play when player collects a food object.
    public AudioClip eatSound2;                    //2 of 2 Audio clips to play when player collects a food object.
    public AudioClip drinkSound1;                //1 of 2 Audio clips to play when player collects a soda object.
    public AudioClip drinkSound2;                //2 of 2 Audio clips to play when player collects a soda object.
    public AudioClip gameOverSound;                //Audio clip to play when player dies.

    private Vector2 touchOrigin = -Vector2.one;


    private Animator animator; //STORE REFERENCE               //Used to store a reference to the Player's animator component.
    private int food;          //STORE LEVEL              //Used to store player food points total during level.


    //Start overrides the Start function of MovingObject
    protected override void Start() //ARA MODIFIQUEM
    {
        //Get a component reference to the Player's animator component
        animator = GetComponent<Animator>();

        //Get the current food point total stored in GameManager.instance between levels.
        food = GameManager2.instance.playerFoodPoints; //TO STORE

        foodText.text = "Food: " + food;

        //Call the Start function of the MovingObject base class.
        base.Start();
    }


    //This function is called when the behaviour becomes disabled or inactive.
    private void OnDisable() //CALLED QUAN ESTAS DISABLED PERO STORE LEVEL
    {
        //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
        GameManager2.instance.playerFoodPoints=food;
    }


    private void Update() //MIRAR SI ES EL TORN
    {
        //If it's not the player's turn, exit the function.
        if (!GameManager2.instance.playersTurn) return;

        int horizontal = 0;      //Used to store the horizontal move direction.
        int vertical = 0;        //Used to store the vertical move direction.

    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

        //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
        horizontal = (int)(Input.GetAxisRaw("Horizontal"));

        //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
        vertical = (int)(Input.GetAxisRaw("Vertical"));

        //Check if moving horizontally, if so set vertical to zero.
        if (horizontal != 0) //RES DE DIAGONAL
        {
            vertical = 0;
        }

    #else
        if(Input.touchCount > 0)
        {
            touchOrigin myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1;
            }
        }
    #endif
        //Check if we have a non-zero value for horizontal or vertical
        if (horizontal != 0 || vertical != 0)
        {
            //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
            //Pass in horizontal and vertical as parameters to specify the direction to move Player in.
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    //AttemptMove overrides the AttemptMove function in the base class MovingObject
    //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
    protected override void AttemptMove<T>(int xDir, int yDir) 
    {
        //Every time player moves, subtract from food points total.
        food--; //CADA COP QUE ET MOUS PERDS MENJAR

        foodText.text = "Food: " + food;

        //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        base.AttemptMove<T>(xDir, yDir);

        //Hit allows us to reference the result of the Linecast done in Move.
        RaycastHit2D hit; //RESULT
        //If Move returns true, meaning Player was able to move into an empty space.
        if (Move(xDir, yDir, out hit))
        {
            //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        //If Move returns true, meaning Player was able to move into an empty space.
        if (Move(xDir, yDir, out hit))
        {
            //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
        }

        //Since the player has moved and lost food points, check if the game has ended.
        CheckIfGameOver();

        //Set the playersTurn boolean of GameManager to false now that players turn is over.
        GameManager2.instance.playersTurn = false;
    }


    //OnCantMove overrides the abstract function OnCantMove in MovingObject.
    //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
    protected override void OnCantMove<T>(T component) //FER UNA ACCIO SI VA CONTRA UNA APRETA
    {
        //Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;

        //Call the DamageWall function of the Wall we are hitting.
        hitWall.DamageWall(wallDamage);

        //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
        animator.SetTrigger("playerChop"); //CANVIAR DESTAT
    }


    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other) //INTERACT WITH OTHER OBJECTS
    {
        //Check if the tag of the trigger collided with is Exit.
        if (other.tag == "Exit") //CHECK THE TAG
        {
            //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
            Invoke("Restart", restartLevelDelay); //1 S PAUSE

            //Disable the player object since level is over.
            enabled = false;
        }

        //Check if the tag of the trigger collided with is Food.
        else if (other.tag == "Food")
        {
            //Add pointsPerFood to the players current food total.
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood+ " Food: "+food;

            //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

            //Disable the food object the player collided with.
            other.gameObject.SetActive(false);
        }

        //Check if the tag of the trigger collided with is Soda.
        else if (other.tag == "Soda")
        {
            //Add pointsPerSoda to players food points total
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;

            //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

            //Disable the soda object the player collided with.
            other.gameObject.SetActive(false);
        }
    }


    //Restart reloads the scene when called.
    private void Restart() //IS COLIDES WITH THE EXIT OBJECT
    {
        //Load the last scene loaded, in this case Main, the only scene in the game.
        SceneManager.LoadScene(0);
        //BoardManager.LoadScene(0);
        //Application.LoadLevel(Application.loadedLevel);
    }


    //LoseFood is called when an enemy attacks the player.
    //It takes a parameter loss which specifies how many points to lose.
    public void LoseFood(int loss)
    {
        //Set the trigger for the player animator to transition to the playerHit animation.
        animator.SetTrigger("playerHit");

        //Subtract lost food points from the players total.
        food -= loss;

        foodText.text = "-" + loss + "Food: " + food;

        //Check to see if game has ended.
        CheckIfGameOver();
    }


    //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
    private void CheckIfGameOver()
    {
        //Check if food point total is less than or equal to zero.
        if (food <= 0)
        {
            //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
            SoundManager.instance.PlaySingle(gameOverSound);

            //Stop the background music.
            SoundManager.instance.musicSource.Stop();

            //Call the GameOver function of GameManager.
            GameManager2.instance.GameOver();
        }
    }
}
