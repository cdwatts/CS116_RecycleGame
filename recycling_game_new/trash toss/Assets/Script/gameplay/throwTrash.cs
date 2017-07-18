﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class throwTrash : lerpable
{
    public float digestionTime;
    private int destroyTime = 3;

    //DO NOT TOUCH THESE THEY ARE USED FOR BARSCRIPT TO GET THE INFO NECESSARY
    public static bool correctCollision;
    public static GameObject tagHolder;
    public static int count = 0;
    // You can do whatever to these

    private Vector3 lastMousePosition;
    private Vector3 newMousePosition;
    private Vector2 distance;
    private Vector3 distance2;
    private Rigidbody2D rb;
    private bool moveByBelt;
    private bool moveBySwipe;
    private bool startCounting;
    private float time;
    GameObject temp;

    //  Not thrown for now.
    private GameObject throwingTarget = null; 
	//  Pointers to the target objects
    GameObject compost;
    GameObject landfill;
    GameObject recycle;

    /*
    Animator compostanim;
    Animator landfillanim;
    Animator recycleanim;
    */

    public override void Start()
    {
        base.Start();
		//  Reset these variables every step
        moveByBelt = true; //  move the object down if true, basically
        moveBySwipe = false; //  set to true after the finger is released
        startCounting = false; //  countdown til automatated item destruction when true
        time = 0; //  presumably the time passed since the counter was activated
        //starts idle animations
        compost = GameObject.Find("composite bin");
        //compostanim = compost.GetComponent<Animator> ();

		landfill = GameObject.Find("landfill bin");
        //landfillanim = landfill.GetComponent<Animator> ();

        recycle = GameObject.Find("recycle bin");
    }


    public override void Update()
    {
		base.Update();
	//  This is the shared update cycle of all throwable trash objects
		if (isLerping ()) {
			//  Do nothing in terms of physics.
			//  Let the lerp handle it.

		} else if (moveBySwipe) {
			//  The buffer is the drag distance that is tolerated before anything happens
			float distanceBuffer = 0.2f;
			float horizontalSensitivity = 0.2f;
			//  Presumably distance2 contains the direction of the swipe, 
			//  and the decimal controls the speed.
			//transform.Translate (distance2 * .1f);
			//  Convert the drag vector into a discrete direction
			Debug.Log("DistL: " + distance);
			if (Mathf.Abs (distance2.x) > horizontalSensitivity) {
				//  horizontal > vertical
				if (distance2.x > distanceBuffer) {
					//right
					throwAt(compost);
				} else if (distance2.x < -distanceBuffer) {
					//left
					throwAt(recycle);
				}
			} else {
				//  v > h
				if (distance2.y > distanceBuffer) {
					//down
					throwAt (landfill);
				} else if (distance2.y < -distanceBuffer) {
					//up
					throwAt (landfill);
				} else {
					//  Neither swiped up nor down. Do neither
					moveByBelt = true;
					moveBySwipe = false;
				}
			}

		} else if (moveByBelt) {
			//  Literally move the item downward if it is on the belt
			transform.Translate (Vector3.down * difficultySettings.moveSpeed);
		} 
		//  Check to see if the object should be destroyed yet
        timeOut(destroyTime);
    }

	public void throwAt(GameObject targetObj){
		//  Throws the current trash object at the specified gameObject
		setLerpTarget(targetObj.transform.position);
		throwingTarget = targetObj;
	}

	public override void lerpTargetReached(){
		base.lerpTargetReached (); //  turns the lerp mechanic off
		//  Now activate the throwing target as if there was a collision.
		if (null != throwingTarget) {
			checkForGoal (throwingTarget);
			//  and then clean up
			throwingTarget = null;
		}
	}

    void OnMouseDown()
    {
        lastMousePosition = Input.mousePosition;
    }


    void OnMouseUp()
    {
		// disable collider so player cannot swipe twice (so much for that)

        moveByBelt = false;
        newMousePosition = Input.mousePosition;
		//  If just distance is used, the objects move incredibly fast
		//  but players control speed of object
        distance = newMousePosition - lastMousePosition;

        float xsquare = distance.x * distance.x;
        float ysquare = distance.y * distance.y;
		//  so dist2 extracts just direction
        distance2 = distance / (Mathf.Sqrt(xsquare + ysquare));

        moveBySwipe = true;
        startCounting = true;
    }


    private void timeOut(float timer)
    {
		//  Throw the object in the landfill after the item counter diminishes
        //bool exist = false;

        if (startCounting)
            time += Time.deltaTime;
        if (time > timer)
        {
            difficultySettings.landfillCounter++;
            Destroy(gameObject);
        }
    }

    //Displays floating text
   /* public static void showText(string text, Vector2 pos)
    {

        var newInstance = new GameObject("damage");
        v//ar textPop = newInstance.AddComponent<popText>();
        textPop.position = pos;
        textPop.myText = text;

    }*/

    // bin collisions
    void OnTriggerEnter2D(Collider2D coll)
    {
        count = 0;
        checkForGoal(coll.gameObject);
    }

	//  bin collisions
	public bool checkForGoal(GameObject other){
        //  checks for if the current trash scored a point and performs the following logic if so.
        //  returns true on success
        correctCollision = false;
        print(count + " first check");
        print(difficultySettings.score + " SCORE");
        temp = gameObject;
        //gotta trick this lil' so I can have my tags and use them too
        if (gameObject.tag == "Plastic" || gameObject.tag == "Paper" || 
            gameObject.tag == "Metal" || gameObject.tag == "Glass")
        {
            temp =  (GameObject)Instantiate(gameObject);
            //print("Before Change  " + gameObject.tag);
            temp.tag = "recycle";
            //print("After Change  " + gameObject.tag);
            //print (temp.tag);
        }
        //count++;
        if (other.tag == gameObject.tag)
		{
			difficultySettings.playRecord.Add(gameObject.name.Substring(0, gameObject.name.Length - 7));
             if (gameObject.tag == "composite")
             {
                //PopTextController.createFloatingText("$", other.transform);
                difficultySettings.digestionTime_com = digestionTime;
                //tagHolder = gameObject;
                tagHolder = (GameObject)Instantiate(gameObject);
                correctCollision = true;
             }
            Destroy(gameObject);
            difficultySettings.score += 1;
            PopTextController.createFloatingText("$", other.transform);
            print(difficultySettings.score);
            return true;
		}
        else if(other.tag == temp.tag)
        {
            print(count + " second check");
            difficultySettings.playRecord.Add(gameObject.name.Substring(0, gameObject.name.Length - 7));
            if (gameObject.tag == "recycle" || temp.tag == "recycle")
            {
                
                difficultySettings.digestionTime_rec = digestionTime;
                tagHolder = (GameObject)Instantiate(gameObject);
                //tagHolder = gameObject;
                correctCollision = true;
            }
            print(difficultySettings.score);
            Destroy(gameObject);
            /*if (count <= 1)
            {
                difficultySettings.score += 1;
            }*/
            difficultySettings.score += 1;
            if (gameObject.tag == "Paper" || gameObject.tag == "Glass")
            {
                PopTextController.createFloatingText("$", other.transform);
            }
            else if (gameObject.tag == "Plastic")
            {
                PopTextController.createFloatingText("$$", other.transform);
            }
            else if (gameObject.tag == "Metal")
            {
                PopTextController.createFloatingText("$$$", other.transform);
            }
            return true;
        }
            
            
            /*
		else if (other.tag == "landfill" & !difficultySettings.isTutorial)
		{
			difficultySettings.landfillCounter++;
			Destroy(gameObject);
			return true;
		} */
        //  Destroy in all cases, regardless of success
        difficultySettings.landfillCounter++;
        print(difficultySettings.landfillCounter + " LANDFILL NUMBER");
		Destroy(gameObject); //  added
		return false;
    }

        
}