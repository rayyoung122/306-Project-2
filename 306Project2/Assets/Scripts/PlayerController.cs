﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;
using Yarn.Unity.Example;
using System;
using System.Text.RegularExpressions;

/**
* Represents a player. Controls player movement, dialogue triggering, and camera movement. 
**/
public class PlayerController : MonoBehaviour {

    // Player body.
    private Rigidbody2D playerBody;
    // Speed of the player's movements.
    private int speed = 8;
    // The confidence bar to render the confidence.
    public Slider confidenceBar;
    // Stores the number of lives.
    private GameObject storeLives;

    // Highlights interactable objects when the player is near them.
    private GameObject glow;
    // The items a player has.
    private List<GameObject> power = new List<GameObject>();
    // Stores the score between scenes.
    public GameObject scoreTransfer;

    // Shows popups.
    public PopupManager popupManager;

    // Variables for camera movement.
    private bool isTransitioning = false;
    private bool isTeleporting = false;
    public Camera cam;
    private Vector2 newCameraPosition;
    private Collider2D currentRoom;
    private float teleportX;
    private float teleportY;

    // Confidence level (score).
    private float score = 0;
    // Number of lives the player has (energy).
    private int numLives = 0;
    // The text displaying the score.
    public Text scoreText;

    // Animates the player.
    private Animator anim;
    
    // The previous position of the player.
    private Vector2 previousPos;
    
    // For dialogue.
    // Interaction radius until you can pick up an item.
    public float interactionRadius = 2.0f;

    public AchievementManager achievementManager;

    // If the player is currently in a zone that triggers dialogue.
    private bool inNPCZone = false;
    // The zone, if the player is in a zone.
    private Collider2D currentNPCZone;

    public GameObject miniMapPlayer;

    private GameObject miniMap;
    private GameObject mapBorder;

    private int currentLevel;
    private float startOfLevelScore;

    private SaveManager saveManager;
 
    private string playerName;

    private bool zoom;


    // Use this for initialization.
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("SaveManager") != null)
        {
            saveManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveManager>();
        }

        if(GameObject.FindGameObjectWithTag("PlayerNameObjectTransfer") == null){
            playerName = "TestName";
        }else{
            playerName = GameObject.FindGameObjectWithTag("PlayerNameObjectTransfer").GetComponent<PlayerNameObjectTransfer>().GetPlayerName();
        }

        if (GameObject.FindGameObjectWithTag("AchievementManager") != null) {
            achievementManager = GameObject.FindObjectOfType<AchievementManager>();
        }

        //Gets level dependent on scene
        if (SceneManager.GetActiveScene().name.Equals("Level-1")) {
            currentLevel = 1;
        }
        else if (SceneManager.GetActiveScene().name.Equals("Level-2"))
        {
            currentLevel = 2;
        }
        else if (SceneManager.GetActiveScene().name.Equals("Level-3"))
        {
            currentLevel = 3;
        }

        // Get the components.
        anim = GetComponent<Animator>();
        playerBody = gameObject.GetComponent<Rigidbody2D>();
        storeLives = GameObject.FindGameObjectWithTag("StoreLives");

        if (GameObject.FindGameObjectWithTag("MiniMapBorder") != null)
        {

            mapBorder = GameObject.FindGameObjectWithTag("MiniMapBorder");
        }
            

        //Prevents rotation of main character.
        playerBody.freezeRotation = true;
        
        // Get each life container.
        for (int i = storeLives.transform.childCount - 1; i >= 0; i--) {
            if (storeLives.transform.GetChild(i).gameObject.name.Equals("Life")) {
                // Add these to the list of life containers.
                power.Add(storeLives.transform.GetChild(i).gameObject);
                // Store the number of lives.
                numLives++;
            }

        }

        // Hide the interactable object glow.
        glow = GameObject.Find("Glow");
        glow.SetActive(false);

        // Update all score text.
        UpdateScoreText();

        //Minimap reference
        if (GameObject.FindGameObjectWithTag("minimap") != null)
        {
            miniMap = GameObject.FindGameObjectWithTag("minimap");
        }
    }

    // Update is called once per frame.
    // This checks for conversations, room transitions, and item interactions.
    void FixedUpdate()
    {
        if (miniMapPlayer != null)
        {
            miniMapPlayer.transform.position = transform.position;
        }

        // Remove all player control when we're in dialogue
        if (FindObjectOfType<DialogueRunner>().isDialogueRunning == true) {
            return;
        }

        // If we're not transitioning between rooms we can move.
        if (!isTransitioning) {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");

            Vector2 movement = new Vector2(x, y);


            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                Debug.Log("ZOOM");
                if (!zoom) {
                    speed = 16;
                    zoom = true;
                } else {
                    speed = 8;
                    zoom = false;
                }
            }


            // Velocity is movement of character * speed.
            playerBody.velocity = (movement * speed);

            //update character Animation to face the right direction
            anim.SetFloat("MoveX",Input.GetAxisRaw("Horizontal"));
            anim.SetFloat("MoveY",Input.GetAxisRaw("Vertical"));

        }
        else {
            // If we are moving between rooms.
            TransitionCamera();
        }

        // Detect if we want to start a conversation
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Start the conversation if so.
            CheckForNearbyNPC();
        }

        // Test achievement
		if (Input.GetKeyDown(KeyCode.F))
		{
            achievementManager.EarnAchievement("Paying respects");
		}

    }

    // Gets the player's last position to get their last direction before minigame starts.
    private IEnumerator CalcLastPos() {

        yield return new WaitForSeconds(0.01f);
        previousPos = gameObject.transform.position;

    }

    // Getter method for the player's previous position.
    public Vector3 GetPreviousPos() {
        return previousPos;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
    }

    private void OnCollisionStay2D(Collision2D collision) {
    }

    // Changes the player's confidence. Confidence is a percentage value between 0-100.
    public void ChangeConfidence(double damagePercent) {
        confidenceBar.value = confidenceBar.value + (float)(damagePercent / 100.0);
        if (confidenceBar.value == 1) {
            achievementManager.EarnAchievement("Maxed Out");
        }
        UpdateScoreText();

    }

    // This is used by yarn to change confidence after dialogue interactions.
    [YarnCommand("change_confidence")]
    public void YarnChangeConfidence(string percent) {
        double doublePercent = Double.Parse(percent);
        ChangeConfidence(doublePercent);

        if (doublePercent > 0) {
             StartCoroutine(popupManager.changeConfidence(percent));
         } else {
            StartCoroutine(popupManager.showWarning("-" + percent + ". Try to confront ignorant people or support others to increase your confidence next time!"));
         }
    }

    // Decreases the player's lives.
    public void LoseOnePower() {
        bool gameOver = true;

        // Loop to remove a life if one is still available.
        foreach (GameObject power in power) {
            // If there is a life to disable.
            if (power.GetComponent<Image>().enabled == true) {
                power.GetComponent<Image>().enabled = false;
                numLives--;
                if (numLives > 0)
                {
                    gameOver = false;
       
                }
                UpdateScoreText();
                break;
            }
        }

        // If the game is over.
        if (gameOver) {
            ChangeLevel("EndOfLevelScene");

        }
    }

    // Transfers the score between scenes.
    public void TransferScore()
    {

        GameObject[] transferObjects = GameObject.FindGameObjectsWithTag("scoreTransferObject");
        for(int i = 0; i< transferObjects.Length;i++)
        {
            Destroy(transferObjects[i]);
        }

        Vector3 pos = new Vector3(0, 0, 0);
        GameObject scoreTransferObject = Instantiate(scoreTransfer, pos, Quaternion.identity);
        DontDestroyOnLoad(scoreTransferObject);
        scoreTransferObject.GetComponent<ScoreTransferScript>().setScore(scoreText.text);

    }

    // Transfers the achievements between scenes.
    public void TransferAchievements()
    {
        Debug.Log("Transferring achievements");

        // Transfer the AchievementManager
        achievementManager.SetStartup(true);
        GameObject achievementManagerTransfer = GameObject.FindGameObjectWithTag("AchievementManager");
        DontDestroyOnLoad(achievementManagerTransfer);

        // Make the AchievementMenu active so you can set it to the top of the hierarchy, then transfer it.
        achievementManager.achievementMenu.SetActive(true);
        GameObject achievementMenuTransfer = GameObject.FindGameObjectWithTag("AchievementMenu");
        achievementMenuTransfer.transform.SetParent(null);
        DontDestroyOnLoad(achievementMenuTransfer);
        
    }

    // Visually increases one energy.
    public void GainOnePower() {
        // Check each energy icon.
        foreach (GameObject power in power) {
            // If there is one available.
            if (power.GetComponent<Image>().enabled == false) {
                power.GetComponent<Image>().enabled = true;
                numLives++;
                UpdateScoreText();
                break;
            }

        }
    }

    // On exit, hide the interactable item glow.
    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("Item") || collider.gameObject.tag.Equals("NPC")) {
            currentNPCZone = null;
            glow.SetActive(false);
            inNPCZone = false;
        }
    }

    // Triggers camera transition when the player enters a room.
    void OnTriggerEnter2D(Collider2D collider)
    {

        // If there is no current room, set it.
        if (currentRoom == null) {
            currentRoom = collider;
        }

        // If the player enters a collision area that is a new room, set the new camera position to the center of the new room.
        if (collider.tag == "Room" && !isTransitioning && currentRoom != collider)
        {
            isTransitioning = true;
            newCameraPosition = new Vector3(collider.transform.position.x, collider.transform.position.y, cam.transform.position.z);
            // Set the new room.
            currentRoom = collider;

            //Removes fog of war from miniMap
            if (miniMap != null)
            {
                MiniMap m = miniMap.GetComponent<MiniMap>();
                m.RemoveFogOfWar(collider);

            }


            //Teleports player to previous entry pos of room if they fail minigame
            StartCoroutine(CalcLastPos());
        }
        else if (collider.tag == "Door" && !isTransitioning) {
            // A door, which teleports the player.
            Collider2D newRoom = collider.GetComponent<Door>().linksToRoom;
            // Teleport the player and also set the new room.
            isTransitioning = true;
            isTeleporting = true;
            currentRoom = newRoom;
            newCameraPosition = new Vector3(newRoom.transform.position.x, newRoom.transform.position.y, cam.transform.position.z);

            // Set the position of the player to be teleported.
            teleportX = collider.GetComponent<Door>().playerX;
            teleportY = collider.GetComponent<Door>().playerY;

            //Removes fog of war from miniMap
            if (miniMap != null)
            {
                MiniMap m = miniMap.GetComponent<MiniMap>();
                m.RemoveFogOfWar(collider);

            }

            //Teleports player to previous entry pos of room if they fail minigame
            StartCoroutine(CalcLastPos());

        } else if (collider.tag == "NPC" || collider.tag == "Item" && !isTransitioning) {
            // If this is an NPC or item, show the interactable item glow.
            inNPCZone = true;
            currentNPCZone = collider;
            Vector2 temp = collider.gameObject.transform.position;
            glow.transform.position = temp;
            glow.SetActive(true);
            // achievementManager.EarnAchievement("Glow");
        }  else if (collider.tag == "EventZone" && !isTransitioning) {
            // Start any dialogue that is automatically triggered.
            SetMotionToZero();
            FindObjectOfType<DialogueRunner>().StartDialogue(collider.GetComponent<GoodNPC>().talkToNode);
        }

    }

    // Used to stop the player moving when the minigame starts
    public void SetMotionToZero() {
        playerBody.velocity = new Vector2(0,0);
        playerBody.rotation = 0;
        playerBody.angularVelocity = 0;
    }

    // Calculates the score.
    public int CalculateScore()  {
        // Score calulated from confidence + the number of lives left.
        int score = (int)(confidenceBar.value * 100) + ((int)numLives * 50);
        return score;
    }

    public void UpdateScoreText() {
        scoreText.text = "Score: " + CalculateScore().ToString();
    }
    
    // Move the camera to the center of the new room.
    void TransitionCamera() {
        cam.transform.position = newCameraPosition;
        isTransitioning = false;
        if (isTeleporting) {

            Vector3 vector = new Vector3(teleportX, teleportY, playerBody.transform.position.z);

            playerBody.transform.position = vector;
            isTeleporting = false;



        }



    }

    /** Find all DialogueParticipants. Filter them to those that have a Yarn start node and are in range; 
     * then start a conversation with the first one
     */
    public void CheckForNearbyNPC() {
         if (inNPCZone) {
            SetMotionToZero();
            // Kick off the dialogue at this node.
            FindObjectOfType<DialogueRunner>().StartDialogue(currentNPCZone.GetComponent<GoodNPC>().talkToNode);
        }
    }

    // Changes the scene to a different level.
    [YarnCommand("transition")]
    public void ChangeLevel(string destination) {
        if (destination.Equals("Level-1") ) {
            achievementManager.EarnAchievement("Back to the Past");
        }

        TransferScore();
        TransferAchievements();
        Debug.Log(destination);
        SceneManager.LoadScene(destination);
    }

    // Shows the player on game start.
    [YarnCommand("move_player")]
    public void Show(string destination) {
        Vector2 start = new Vector2(0, -2);
        gameObject.transform.position = start;
        cam.transform.position = new Vector2(1, -1);

        //Activates minimap on start of game
        mapBorder.SetActive(true);

    }

    // Repels player from walls when they are not allowed to go there yet.
    [YarnCommand("repel")]
    public void RepelPlayer() {
        // Calculates the opposite direction to the NPC from the player's input.
        Vector2 direction = (transform.position).normalized;

        transform.Translate(-direction);
    }

    // Repels player from walls when they are not allowed to go there yet.
    [YarnCommand("repelWen")]
    public void RepelWenPlayer()
    {
        // Calculates the opposite direction to the NPC from the player's input.
        Vector2 direction = new Vector2(2, 0);

        transform.Translate(direction);
    }



    [YarnCommand("passLevel")]
    public void passlevel(string level)
    {

        TransferScore();
        TransferAchievements();

        Regex regexObj = new Regex(@"[^\d]");
        string score = regexObj.Replace(scoreText.text, "");

        startOfLevelScore = float.Parse(score);

        // Earn an achievement based on the level that was completed.
        if (SceneManager.GetActiveScene().name.Equals("Tutorial")) {
            achievementManager.EarnAchievement("Back to the Past");
    
            if (numLives == 3) {
                 achievementManager.EarnAchievement("School Ace");
            }

        } else if (SceneManager.GetActiveScene().name.Equals("Level-1")) {
            achievementManager.EarnAchievement("Teacher's Pet");

        }  else if (SceneManager.GetActiveScene().name.Equals("Level-2")){
            achievementManager.EarnAchievement("Graduation Nation");

        } else if (SceneManager.GetActiveScene().name.Equals("Level-3")){
            achievementManager.EarnAchievement("Completionist");
        }


        //Dont save if level change is from tutorial
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            saveManager.SaveLevel(startOfLevelScore, currentLevel, playerName);
        }

    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public int GetLevel()
    {

        return currentLevel;
    }

}