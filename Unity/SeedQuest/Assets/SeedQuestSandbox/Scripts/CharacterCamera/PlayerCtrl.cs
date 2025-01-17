﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using UnityEngine.EventSystems;

public enum PlayerControlMode { Keyboard, Touch, Click };

public class PlayerCtrl : MonoBehaviour {

    static public Transform PlayerTransform = null;

    public float moveKeySpeed = 10f;
    public float moveTouchSpeed = 1f;
    public PlayerControlMode mode = PlayerControlMode.Click;
    public Texture2D cursorTextureWalking;
    public Texture2D cursorTextureDefault;

    public void Awake() {
        PlayerCtrl.PlayerTransform = transform;
    }

    public void Update() {
        MovePlayer();
        CheckIfWalkable();
    }

    public void MovePlayer() {
        if (mode == PlayerControlMode.Keyboard)
            MoveWithKeys();
        else if (mode == PlayerControlMode.Touch)
            MoveWithTouchSwipe();
        else if (mode == PlayerControlMode.Click)
            MoveWithClick();
    } 

    public void MoveWithClick() {
        if (PauseManager.isPaused || PauseManager.isInteracting)
            return;


        if(Input.GetMouseButtonDown(0)) {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Camera camera = IsometricCamera.Camera;

            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 100.0f)) {

                //NavMeshAgent agent = GetComponent<NavMeshAgent>();
                //agent.SetDestination(hit.point);

                NavMeshHit navHit;
                int walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
                if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, walkableMask))
                {
                    NavMeshAgent agent = GetComponent<NavMeshAgent>();
                    agent.SetDestination(hit.point);

                    MarkerManager.GenerateMarker(hit.point + new Vector3(0, 0.1f, 0), Quaternion.identity);
                }
            }
            
        } 
    }

    public void CheckIfWalkable()
    {
        if (mode != PlayerControlMode.Click)
            return;

        Camera c = Camera.main;
        RaycastHit hit;
        Ray ray = c.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f)) {
            NavMeshHit navHit;
            int walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
            if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, walkableMask)) {
                //Cursor.SetCursor(cursorTextureWalking, Vector2.zero, CursorMode.Auto);
                CursorActionUI.Show = true;
            }
            else {
                //Cursor.SetCursor(cursorTextureDefault, Vector2.zero, CursorMode.Auto);
                CursorActionUI.Show = false;
            }
        }
        else {
            //Cursor.SetCursor(cursorTextureDefault, Vector2.zero, CursorMode.Auto);
            CursorActionUI.Show = false;
        }
    }

    public void MoveWithKeys() {
        float move = Input.GetAxis("Horizontal") * moveKeySpeed * Time.deltaTime;
        transform.Translate(move, 0, 0);
    }

    private Vector2 touchStartPos = Vector2.zero;
    private Vector3 startPos = Vector3.zero;
    private Vector3 moveVec = Vector3.zero;
    private float touchSpeed = 0.5f;
    private float swipeSpeed = 5f;
    private float swipeDist = 5.0f;
    private float touchPixelTol = 50; // check prev dx?
    private bool isSwiping = false;
    private float swipeFraction = 0.0f;

    public void MoveWithTouchSwipe() {

        if(Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) {
                isSwiping = false;
                swipeFraction = 0.0f;

                touchStartPos = touch.position;
                startPos = transform.position;
            }
            else if(touch.phase == TouchPhase.Moved) {
                isSwiping = false;
                float dx = touch.position.x - touchStartPos.x;
                if (Mathf.Abs(dx) < touchPixelTol)
                    return;
                Debug.Log("Touch -- Start:" + touchStartPos.x + " End:" + touch.position.x + " dx:" + dx);

                float move = dx * touchSpeed * Time.deltaTime;
                move = Mathf.Clamp(move, -swipeDist, swipeDist);
                moveVec = new Vector3(move, 0, 0);
                transform.position = startPos + moveVec;

                swipeFraction = Mathf.Abs(move / swipeDist);
            }
            else if(touch.phase == TouchPhase.Ended) {
                int sgn = ((touch.position.x - touchStartPos.x) > 0) ? 1 : -1;
                float move = sgn * swipeDist;
                moveVec = new Vector3(move, 0, 0);
                
                string debug = " SF: " + swipeFraction;
                swipeFraction = swipeFraction + swipeSpeed * Time.deltaTime;
                swipeFraction = Mathf.Min(swipeFraction, 1.0f);
                debug = debug + "SF2: " + swipeFraction;
                transform.position = Vector3.Lerp(startPos, startPos + moveVec, swipeFraction);
                isSwiping = true;
                Debug.Log("Touch Ended: Start:" + touchStartPos.x + " End:" + touch.position.x + debug); 

            }
        }
        else if (isSwiping) {
            string debug = " SF: " + swipeFraction;
            swipeFraction = swipeFraction + swipeSpeed * Time.deltaTime;
            swipeFraction = Mathf.Min(swipeFraction, 1.0f);
            debug = debug + "SF2: " + swipeFraction;
            transform.position = Vector3.Lerp(startPos, startPos + moveVec, swipeFraction);
            Debug.Log("Swiping -- Touch Ended: Start:" + startPos.x + " End:" + (startPos + moveVec).x + debug); 

            if (swipeFraction >= 1.0f)
                isSwiping = false;
        }


    }

}
