using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour //Abstract perque esta incomplete
{
    public float moveTime = 0.1f;  //mouret en segons          //Time it will take object to move, in seconds.
    public LayerMask blockingLayer; //ON VEUREM LES COLLISIONS ELS VAM POSAR A WALL LAYERS           //Layer on which collision will be checked.


    private BoxCollider2D boxCollider;         //The BoxCollider2D component attached to this object.
    private Rigidbody2D rb2D; //REFERENCE               //The Rigidbody2D component attached to this object.
    private float inverseMoveTime;   //MOVEMENT CALCULATIONS         //Used to make movement more efficient.


    //Protected, virtual functions can be overridden by inheriting classes.
    protected virtual void Start() //PER OVERWRITTEN BY INHERITING CLASSES PERQUE TINGUIN UNA DIFFERENT IMPLEMENTATION AT START
    {
        //Get a component reference to this object's BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

        //Get a component reference to this object's Rigidbody2D
        rb2D = GetComponent<Rigidbody2D>();

        //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
        inverseMoveTime = 1f / moveTime; //PODEM MULTIPLICAR PQ SIGUI MES MILLOR A LA COMPILACIO
    }

    //Move returns true if it is able to move and false if not. 
    //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) //RETORA UN BOOL TURE IF MOCIMENT
    {
        //Store start position to move from, based on objects current transform position.
        Vector2 start = transform.position; //ON ESTAS, EN VECTOR2 JA SET PASSA A 2d

        // Calculate end position based on the direction parameters passed in when calling Move.
        Vector2 end = start + new Vector2(xDir, yDir);

        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        boxCollider.enabled = false; //DISABLE ATTACHED PQ NO ENS CHOQUEM A NOSALTRES MATEIXES

        //Cast a line from start point to end point checking collision on blockingLayer.
        hit = Physics2D.Linecast(start, end, blockingLayer); //CHECK COLLISION

        //Re-enable boxCollider after linecast
        boxCollider.enabled = true;

        //Check if anything was hit
        if (hit.transform == null) //AVAILABLE
        {
            //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
            StartCoroutine(SmoothMovement(end));

            //Return true to say that Move was successful
            return true; //ENS HEM MOGUT
        }

        //If something was hit, return false, Move was unsuccesful.
        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end) //PER MOURE DUN LLOC A UN ALTRE. END ES ON ANEM
    {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while (sqrRemainingDistance > float.Epsilon) //CHECKS QUE ENS MOUREM MES QUE ZERO
        {
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime); //PROPORCIONAL AL END BASED ON THE MOVETIME
            //MOVETOWARDS(CURRENTPOSITION, ON VOLEM ANAR, PUNT QUE ESTA A AQUELLA DISTANCIA)

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition(newPostion); //PER ANARHI

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude; //RECALCULAR ON ENS HEM ANAT

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null; //ESPERAR UN FRAME BEFORE WE REEVALUATE
        }
    }


    //The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
    //AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component // AMB QUI INTERECCIONEM. EN EL CAS DE PLAYERS <> WALLS. EN EL CAS DENEMIESS <> PLAYERS
    {
        //Hit will store whatever our linecast hits when Move is called.
        RaycastHit2D hit; 

        //Set canMove to true if Move was successful, false if failed.
        bool canMove = Move(xDir, yDir, out hit);

        //Check if nothing was hit by linecast
        if (hit.transform == null) //NO HAS CHOCAT AMB RES
            //If nothing was hit, return and don't execute further code.
            return;

        //Get a component reference to the component of type T attached to the object that was hit
        T hitComponent = hit.transform.GetComponent<T>(); //SABER QUE HAS CHOCAT

        //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
        if (!canMove && hitComponent != null)

            //Call the OnCantMove function and pass it hitComponent as a parameter.
            OnCantMove(hitComponent);
    }


    //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
    //OnCantMove will be overriden by functions in the inheriting classes.
    protected abstract void OnCantMove<T>(T component) //PROTECTED ABSTRACT QUE ES POT REESCRIURE
        where T : Component;

}
