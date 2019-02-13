﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//The Main Startup Class. (Nothing is really required from this class.)
public class PlayerController : PlayerPackage.PartyHandler
{
    private void Start()
    {
        VariablesStart();
        StartingVariables();
        InteractStart();
    }
    private void Update()
    {
        LevelBounds();
        CameraAdjust();
        PartySwitcher();
        InteractUpdate();
        MovePlayer();
        Actions();
    }
    private void LateUpdate()
    {
        CameraRotator();
    }
}
/// <summary>
/// All the scripts that handle the character being played as.
/// </summary>
namespace PlayerPackage
{
    /// <summary>
    /// Handles the party characters.
    /// </summary>
    public class PartyHandler : PlayerStats
    {
        BotReciever partyMember;
        /// <summary>
        /// Handles the switching of party members.
        /// </summary>
        protected void PartySwitcher()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                partyMember = game.bot[0];
                if (partyMember && partyMember.isPartyMember)
                {
                    cam = null;
                    Destroy(partyMember.GetComponent<BoxCollider>());
                    Destroy(partyMember.GetComponent<CapsuleCollider>());

                    gameObject.AddComponent<BotReciever>();
                    GetComponent<BotReciever>().points = partyMember.points;
                    GetComponent<BotReciever>().WaypointPassed = 0;
                    GetComponent<BotReciever>().range = 20;

                    partyMember.gameObject.AddComponent<PlayerController>();
                    Destroy(partyMember.botAI);
                    partyMember.bot.enabled = true;
                    Destroy(partyMember);
                    GetComponent<BotReciever>().target = FindObjectOfType<PlayerController>().transform;
                    Destroy(this);
                }
            }
        }
    }

    /// <summary>
    /// (Currently Empty)
    /// </summary>
    public class PlayerStats : PlayerStart
    {

    }

    /// <summary>
    /// (Currently Empty)
    /// </summary>
    public class PlayerStart : PlayerInteract
    {
        protected void StartingVariables()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            switch (currentScene)
            {
                case "Gameplay Test":

                    break;
            }
        }
    }

    /// <summary>
    /// (Currently Empty)
    /// </summary>
    public class PlayerInteract : PlayerMovement
    {
        protected void InteractStart()
        {

        }
        protected void InteractUpdate()
        {

        }
        protected void ButtonPrompt()
        {
            //buttonPrompt.transform.LookAt(cam.transform.position);
        }
    }

    /// <summary>
    /// Handles which state the character should be in.
    /// </summary>
    public class PlayerMovement : CollisionDetection
    {
        /// <summary>
        /// Checks which state the character should be in.
        /// </summary>
        protected void MovePlayer()
        {
            if (player)
            {
                if (player.isGrounded)
                {
                    anim.applyRootMotion = true;
                    collisionDetected = false;
                    moveType = "Normal";
                }
                switch (moveType)
                {
                    case "Normal":
                        player.enabled = true;
                        anim.SetBool("Climbing", false);
                        NormalMovement();
                        break;

                    case "Hanging":
                        player.enabled = false;
                        if (game.button_Jump)
                        {
                            moveType = "Normal";
                            anim.SetBool("Hanging", false);
                        }
                        if (game.moveY > 0.5f)
                        {

                            anim.SetBool("Climbing", true);
                            if (playerPosition == Vector3.zero)
                            {
                                playerPosition = transform.position;
                            }
                            playerDestination = new Vector3(playerPosition.x, playerPosition.y + 10, playerPosition.z);
                            anim.SetBool("Hanging", false);
                        }
                        if (playerPosition != Vector3.zero)
                        {
                            transform.position = Vector3.MoveTowards(playerPosition, playerDestination, 1);
                            if (transform.position == playerDestination)
                            {
                                moveType = "Normal";
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Handles standard movement and animation.
        /// </summary>
        protected void NormalMovement()
        {
            #region Move direction
            float yspeed = moveDirection.y;
            if (canMove)
            {
                moveDirection = transform.forward * game.moveY + (transform.right * game.moveX);
                moveDirection = moveDirection.normalized * moveSpeed;
            }
            moveDirection.y = yspeed;
            #endregion
            moveDirection.y = moveDirection.y + (Physics.gravity.y * Time.deltaTime * gravity);
            if (player)
            {
                player.Move(moveDirection * Time.deltaTime);
            }
            switch (viewType)
            {
                case "ThirdPerson":
                    #region Rotation
                    if (canMove)
                    {
                        if (game.moveX != 0 || game.moveY != 0)
                        {
                            player.transform.rotation = Quaternion.Euler(0, rotator + 30, 0);
                            Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 90, moveDirection.z));
                            theRotation = newRotation;
                            skeleton.rotation = Quaternion.Slerp(skeleton.rotation, newRotation, player_rotateSpeed * Time.deltaTime);
                        }
                    }
                    #endregion
                    break;
            }
            #region Animator
            if (canMove)
            {
                anim.SetBool("OnGround", player.isGrounded);
                anim.SetFloat("Speed", Mathf.Abs(game.moveX) + Mathf.Abs(game.moveY));
            }
            #endregion
        }
    }

    /// <summary>
    /// What to do under certain conditions when the player collides with something.
    /// </summary>
    public class CollisionDetection : PlayerActions
    {
        /// <summary>
        /// When a collider set as a trigger touches the player, this function is called.
        /// </summary>
        /// <param name="interaction">The collider in question that triggers this event.</param>
        protected void OnTriggerEnter(Collider interaction)
        {
            if (interaction != null)
            {
                Interactable entity = interaction.GetComponentInParent<Interactable>();
                if (entity)
                {
                    switch (entity.type)
                    {
                        default:
                            SetFocus(entity);
                            break;

                        case "Item":
                            if (interaction != null)
                            {
                                SetFocus(entity);
                                interaction.transform.parent.SendMessage("Item");
                            }
                            break;
                    }
                }
            }
            switch (interaction.name)
            {
                case "Ledge":
                    if (!collisionDetected && !player.isGrounded)
                    {
                        anim.SetBool("Hanging", true);
                        collisionDetected = true;
                        moveType = "Hanging";

                    }
                    break;
                case "Sprite Light":
                    interaction.enabled = false;
                    interaction.GetComponent<SpriteAI>().triggered = true;
                    break;

            }
            switch (interaction.tag)
            {
                case "NPC":
                    buttonPrompt.transform.position = new Vector3(interaction.transform.position.x, interaction.transform.position.y + 1.8f, interaction.transform.position.z + 0.5f);
                    break;
            }
        }

        /// <summary>
        /// When a collider set as a trigger is no longer touching the player, this function is called.
        /// </summary>
        /// <param name="interaction">The collider in question that triggers this event.</param>
        protected void OnTriggerExit(Collider interaction)
        {
            if (interaction != null)
            {
                Interactable entity = interaction.GetComponentInParent<Interactable>();
                if (entity)
                {
                    switch (entity.type)
                    {
                        default:
                            RemoveFocus();
                            break;

                        case "Item":
                            RemoveFocus();
                            entity.SendMessage("Item");
                            break;
                    }
                    switch (interaction.tag)
                    {
                        case "NPC":
                            buttonPrompt.transform.position = new Vector3();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Brings the script attached to the object the player collided with into focus.
        /// </summary>
        /// <param name="newFocus"></param>
        private void SetFocus(Interactable newFocus)
        {
            focus = newFocus;

            switch (focus.type)
            {
                default:
                    focus.hasInteracted = true;
                    break;

                case "Door":
                    break;


                case "Chest":
                    break;
            }
        }

        /// <summary>
        /// Brings the script attached to the object the player collided with out of focus.
        /// </summary>
        private void RemoveFocus()
        {
            if (focus)
            {
                focus.hasInteracted = false;
                if (focus.type == "Door")
                {
                    //focus.SendMessage("DoorClose");
                }
                focus = null;
            }
        }
    }

    /// <summary>
    /// Handles the actions that take place when certain buttons are pressed.
    /// </summary>
    public class PlayerActions : Camera
    {
        /// <summary>
        /// An update method for all the actions scarf makes.
        /// </summary>
        protected void Actions()
        {
            MenuPrompt();
            if (game)
            {
                Jump(game.button_Jump);
                Interact(game.button_Action);
            }
        }

        /// <summary>
        /// When you press the jump button, this function is called.
        /// </summary>
        /// <param name="button"></param>
        private void Jump(bool button)
        {
            if (player)
            {
                if (player.isGrounded)
                {
                    moveDirection.y = 0;
                    if (button)
                    {
                        moveDirection.y = jumpForce;
                    }
                    anim.SetBool("OnGround", true);
                }
                else
                {
                    anim.SetBool("OnGround", false);
                }
            }
        }

        /// <summary>
        /// When you press the action button, then this function is called.
        /// </summary>
        /// <param name="button">assign a button input to the action.</param>
        private void Interact(bool button)
        {
            if (focus != null)
            {
                if (button)
                {
                    switch (focus.type)
                    {
                        case "Chest":
                            focus.hasInteracted = !focus.hasInteracted;
                            break;

                        case "Door":
                            focus.hasInteracted = true;
                            break;

                        case "Save Point":
                            focus.hasInteracted = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the text of the action command.
        /// </summary>
        private void MenuPrompt()
        {
            if (focus)
            {
                switch (focus.type)
                {
                    default:
                        actionText = "-";
                        break;

                    case "Chest":
                        actionText = "Open";
                        break;

                    case "Door":
                        actionText = "Open";
                        break;
                }
            }
            else
            {
                actionText = "-";
            }
            FindObjectOfType<MenuChooser>().menuItems[6].GetComponentInChildren<Text>().text = actionText;
        }
    }

    /// <summary>
    /// Camera handler.
    /// </summary>
    public class Camera : BugHandler
    {
        /// <summary>
        /// Makes sure that the starting state of the camera is correct.
        /// </summary>
        protected void CameraAdjust()
        {
            if (!canMove)
            {
                if (currentYaw != 90)
                {
                    currentYaw -= 1 * 5;
                }
                else
                {
                    elapsed += Time.deltaTime;
                    if (!canMove)
                    {
                        if (elapsed >= timerspeed)
                        {
                            GameObject.Find("KH UI").GetComponent<Canvas>().enabled = true;
                            FindObjectOfType<MenuChooser>().enabled = true;
                            FindObjectOfType<MenuChooser>().enabled = true;
                            canMove = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the rotation and switching of perspectives.
        /// </summary>
        protected void CameraRotator()
        {
            if (game)
            {
                horizontal = game.cameraX * cam_rotateSpeed_X * Time.deltaTime;
                rotator = currentYaw + 150;
                if (canMove)
                {
                    currentYaw += game.cameraX * cam_rotateSpeed_X * Time.deltaTime;
                }
                switch (viewType)
                {
                    case "FirstPerson":
                        if (game.button_Select)
                        {
                            cam.parent = GameObject.Find("Player 1").transform;
                            viewType = "ThirdPerson";
                            cam.localPosition = new Vector3(44.453f, 2.56f, 33.1f);
                        }
                        FirstPerson();
                        break;

                    case "ThirdPerson":
                        if (game.button_Select)
                        {
                            cam.parent = lookObject.transform;
                            viewType = "FirstPerson";
                        }
                        ThirdPerson();
                        break;
                }
            }
        }

        /// <summary>
        /// How the camera behaves when in a firstperson perspective.
        /// </summary>
        private void FirstPerson()
        {
            float vertical = game.cameraX * cam_rotateSpeed_Y * 2 * Time.deltaTime;
            float horizontal = -game.cameraY * cam_rotateSpeed_Y * 2 * Time.deltaTime;

            cam.localRotation = Quaternion.Euler(cam.localRotation.x, lookObject.localRotation.y, cam.localRotation.z);
            lookObject.Rotate(horizontal, 0, 0);

            transform.Rotate(0, vertical, 0);
            cam.Rotate(0, horizontal, 0);
            cam.localPosition = Vector3.zero;
        }

        /// <summary>
        /// How the camera behaves in a thirdperson perspective.
        /// </summary>
        private void ThirdPerson()
        {
            currentZoom -= game.cameraY * 3.5f * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, 2f, 4f);

            cam.transform.position = player.transform.position - new Vector3(0, -1, -2) * currentZoom;
            cam.transform.LookAt(player.transform.position + Vector3.up * 2f);
            cam.transform.RotateAround(player.transform.position, Vector3.up, currentYaw);
        }
    }

    /// <summary>
    /// In glitch situations, this is where the functions are at.
    /// </summary>
    public class BugHandler : Variables
    {
        /// <summary>
        /// What to do when the character goes out of bounds.
        /// </summary>
        protected void LevelBounds()
        {
            if (transform.localPosition.y < -15)
            {
                transform.position = GameObject.Find("Player 1").transform.position;
                health = health - 45;
            }
        }
    }

    /// <summary>
    /// All the different variables are stored here.
    /// </summary>
    public class Variables : MonoBehaviour
    {
        protected void VariablesStart()
        {
            pause = FindObjectOfType<MenuChooser>();
            cam = UnityEngine.Camera.main.transform;
            anim = GetComponent<Animator>();
            game = FindObjectOfType<GameManager>();

            buttonPrompt = GameObject.Find("Prompt");

            #region Camera and player rotation 
            lookObject = GameObject.Find("Look Object").transform;
            skeleton = GetComponentInChildren<SkinnedMeshRenderer>().transform;
            offset = transform.position - cam.position;
            #endregion
            #region Character controller settings.
            if (GetComponent<CharacterController>())
            {
                player = GetComponent<CharacterController>();
            }
            else
            {
                player = gameObject.AddComponent<CharacterController>();
            }
            player.height = 2;
            player.radius = 0.5f;
            player.center = new Vector3(0, 1.1f, 0);
            #endregion
        }
        #region Variables
        #region Stats
        [HideInInspector] public int healthMax = 100;
        [HideInInspector] public int magicMax = 100;
        [HideInInspector] protected int exp;

        public int magic = 50;
        public int health = 70;
        public int level = 1;
        #endregion
        #region Modes
        /// <summary>
        /// Which perspective you are playing from.
        /// </summary>
        protected string viewType = "ThirdPerson";

        /// <summary>
        /// Determines if the player can move or not.
        /// </summary>
        [HideInInspector] public bool canMove = true;
        #endregion
        #region Timing
        protected float timerspeed = 0f;
        protected float elapsed = -0.5f;
        #endregion
        #region Actions
        /// <summary>
        /// The text that appears on the command menu when something can be interacted with.
        /// </summary>
        protected string actionText = "-";

        /// <summary>
        /// If a state requires for a character to move to another state this will be the start point.
        /// </summary>
        protected Vector3 playerPosition;

        /// <summary>
        /// If a state requires for a character to move to another state this will be the finish point.
        /// </summary>
        protected Vector3 playerDestination;
        #endregion
        #region Physics
        /// <summary>
        /// Determines how intense gravity is.
        /// </summary>
        protected float gravity = 3;

        /// <summary>
        /// When the controls need to change for a specific situation, this controls that.
        /// </summary>
        public string moveType = "Normal";
        #endregion
        #region Movement
        /// <summary>
        /// Determines what direction the player is moving or wether the player is jumping.
        /// </summary>
        protected Vector3 moveDirection;

        /// <summary>
        /// Determines how fast the player is going.
        /// </summary>
        protected float moveSpeed = 10;

        /// <summary>
        /// Determines how high you go when you jump.
        /// </summary>
        protected float jumpForce = 14;
        #endregion
        #region Components
        /// <summary>
        /// The armature object.
        /// </summary>
        protected Transform skeleton;

        /// <summary>
        /// Handles the rotation of the camera in thirdperson mode.
        /// </summary>
        protected float rotator = 150;

        /// <summary>
        /// Grabs variables from the pause menu script.
        /// </summary>
        protected MenuChooser pause;

        /// <summary>
        /// Camera component.
        /// </summary>
        protected Transform cam;

        /// <summary>
        /// Grabs variables from the game manager.
        /// </summary>
        protected GameManager game;

        /// <summary>
        /// The animator attached to the character.
        /// </summary>
        [HideInInspector] public Animator anim;

        /// <summary>
        /// The component that moves the player around, handles the collisions and also creates gravity.
        /// </summary>
        [HideInInspector] public CharacterController player;
        #endregion
        #region Rotation Math
        #region Rotation
        /// <summary>
        /// The handler for the actual angle the player rotates at.
        /// </summary>
        protected Quaternion theRotation;

        /// <summary>
        /// Horizontal axis for the camera to face.
        /// </summary>
        protected float horizontal;

        /// <summary>
        /// How fast the player is allowed to return.
        /// </summary>
        protected float player_rotateSpeed = 10;
        #endregion
        #region First Person
        /// <summary>
        /// Positions the camera when in first person mode.
        /// </summary>
        protected Transform lookObject;
        #endregion
        #region Third Person
        /// <summary>
        /// The camera position relative to the player and the camera rotation.
        /// </summary>
        protected Vector3 offset;

        /// <summary>
        /// The speed at which the camera moves on the horizontal axis.
        /// </summary>
        protected float cam_rotateSpeed_X = 180;

        /// <summary>
        /// The speed at which the camera moves on the vertical axis.
        /// </summary>
        protected float cam_rotateSpeed_Y = 80;

        /// <summary>
        /// The rotation of the camera in thirdperson mode.
        /// </summary>
        [HideInInspector] public float currentYaw = 210f; //210

        /// <summary>
        /// The distance the camera is set to in thirdperson mode.
        /// </summary>
        protected float currentZoom = 2f;
        #endregion
        #endregion
        #region Collision Based
        /// <summary>
        /// Collision detection mostly meant for player movement. 
        /// </summary>
        protected bool collisionDetected = true;

        /// <summary>
        /// The interactable object that is currently in focus.
        /// </summary>
        public Interactable focus;

        /// <summary>
        /// When an action can be called this button object will hover next to the object in question.
        /// </summary>
        protected GameObject buttonPrompt;
        #endregion
        #endregion
    }
}