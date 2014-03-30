#pragma strict

// Script by ben0bi
// regarding the ThirdPersonController from the ThirdPersonPlattformer Example Project

// The player can make jumps, doublejumps and walljumps. 
// It will look where the camera looks, and turn smoothly towards it if the camera changes.
// Its "optimized" for 3rd Person FPS type games. (To use with my camera script in a post before)

// The script is in need for the Input Axes: "Horizontal", "Vertical", "Jump" and "Run".
// You need to add "Run" by yourself:
// In Unity: Click on Edit->Project Settings->Input - In the Inspector, name one of the axes to "Run" and 
// let the positive value be "right shift". To add a new axis, just type a greater number into the size 
// property at the top.

// Shooting will happen in another script.

// is the player at all controllable?
var isControllable = true;

var canJump=true;			// can the player jump? if not, he can also not doublejump and walljump
var canDoubleJump=true;		// can the player jump after a jump?
var canWallJump=true;		// can the player jump away from a wall when touching it?
var jumpHeight = 1.5; 		// this is the jump height by pressing it the first time.
var doubleJumpHeight=1.0; 	// this height will be added to verticalspeed one the jump button is pressed the second time.

var walkingSpeed=6.0;		// the normal walking speed.
var runningSpeed=12.0;
var speedSmoothing = 10.0;
var sidewayWalkMultiplier=0.5;

// The gravity for the character
var gravity = 20.0;

private var moveDirection:Vector3 = Vector3.zero;
private var faceDirection:Vector3 =Vector3.zero;
private var moveSpeed=0.0;
private var verticalSpeed=0.0;

// when can the player jump again?
private var jumpRepeatTime = 0.05;
private var jumping=false;
private var doubleJumping=false;
private var lastJumpButtonTime=-10.0;
// When did we touch the wall the first time during this jump (Used for wall jumping)
private var touchWallJumpTime = -1.0;
private var wallJumpTimeout = 0.15;

// Last time we performed a jump
private var lastJumpTime = -1.0;
private var jumpTimeout = 0.15;
// Is the user pressing any keys?
private var isMoving = false;
private var jumpingReachedApex=false;
// Average normal of the last touched geometry
private var wallJumpContactNormal : Vector3;

// The last collision flags returned from controller.Move
private var collisionFlags : CollisionFlags; 

private var jumpButtonPressedTwice=0;

function Awake ()
{
	moveDirection = transform.TransformDirection(Vector3.forward);
}

function Update () 
{
	// kill all inputs if not controllable
	if (!isControllable) {Input.ResetInputAxes();}

	// set the jump button time variable and check for a doublejump. (=jumpButtonPressedTwice==1)
	if (Input.GetButtonDown ("Jump")) 
	{
		if(jumpButtonPressedTwice==-2)	// it was set the second time..
			jumpButtonPressedTwice=1;	// thats a double jump
		else
			jumpButtonPressedTwice=-1;	// set it the first time..
		lastJumpButtonTime = Time.time;
	}
	
	// check if jump button was pressed the second time
	if(Input.GetButtonUp("Jump"))
	{
		if(jumpButtonPressedTwice==-1)	// set it the second time
			jumpButtonPressedTwice=-2;
	}
	
	UpdateSmoothedMovementDirection();

	ApplyGravity ();
	
	// Perform a wall jump logic
	// - Make sure we are jumping against wall etc.
	// - Then apply jump in the right direction)
	if (canWallJump)
		ApplyWallJump();
	
	// Apply jumping logic
	ApplyJumping();
	
	var movement = moveDirection * moveSpeed + Vector3 (0, verticalSpeed, 0);// + inAirVelocity;
	movement *= Time.deltaTime;
	
	// Move the controller
	var controller : CharacterController = GetComponent(CharacterController);
	wallJumpContactNormal = Vector3.zero;
	collisionFlags = controller.Move(movement);
	
	// rotate the character
	// TODO: lerp or slerp it (? wich one, and why?)
	transform.rotation = Quaternion.LookRotation(faceDirection);
	
	// We are in jump mode but just became grounded
	if (IsGrounded())
	{
		//lastGroundedTime = Time.time;
		//inAirVelocity = Vector3.zero;
		if (jumping || doubleJumping)
		{
			jumping = false;
			doubleJumping=false;
			jumpButtonPressedTwice=0;
			SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
		}
	}
}

function UpdateSmoothedMovementDirection()
{
	var cameraTransform = Camera.main.transform;
	var grounded = IsGrounded();
	
	// Forward vector relative to the camera along the x-z plane	
	var forward = cameraTransform.TransformDirection(Vector3.forward);
	forward.y = 0;
	forward = forward.normalized;

	// Right vector relative to the camera
	// Always orthogonal to the forward vector
	var right = Vector3(forward.z, 0, -forward.x);
	// get the input axes
	var vertical = Input.GetAxisRaw("Vertical");
	var horizontal = Input.GetAxisRaw("Horizontal");
	
	//var wasMoving = isMoving;
	isMoving = Mathf.Abs (horizontal) > 0.1 || Mathf.Abs (vertical) > 0.1;
	
	faceDirection = Vector3.Slerp(faceDirection, forward,Time.deltaTime*8);
	var targetDir=forward*vertical+right*horizontal*sidewayWalkMultiplier;
	// Grounded controls
	if (grounded)
	{
		// We store speed and direction seperately,
		// so that when the character stands still we still have a valid forward direction
		// moveDirection is always normalized, and we only update it if there is user input.
		if (faceDirection != Vector3.zero)
		{
			moveDirection = targetDir.normalized;
		}
		
		// Smooth the speed based on the current target direction
		var curSmooth = speedSmoothing * Time.deltaTime;
		
		// Choose target speed
		//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
		var targetSpeed = Mathf.Min(targetDir.magnitude, 1.0);
	
		// Pick speed modifier
		if (Input.GetButton ("Run"))
		{
			targetSpeed *= runningSpeed;
		}else{
			targetSpeed *= walkingSpeed;
		}
		
		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
	}
}

function ApplyGravity ()
{
	if (isControllable)	// don't move player at all if not controllable.
	{		
		// When we reach the apex of the jump we send out a message
		if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0)
		{
			jumpingReachedApex = true;
			SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
		}
		
		if (IsGrounded ())
			verticalSpeed = 0.0;
		else
			verticalSpeed -= gravity * Time.deltaTime;
	}
}

function ApplyWallJump()
{
	if(!jumping)
		return;
	
	// if the a side was hit, set the time
	if(collisionFlags==CollisionFlags.CollidedSides)
	{
		touchWallJumpTime=Time.time;
	}
	
	// The user can trigger a wall jump by hitting the button shortly before or shortly after hitting the wall the first time.
	var mayJump = lastJumpButtonTime > touchWallJumpTime - wallJumpTimeout && lastJumpButtonTime < touchWallJumpTime + wallJumpTimeout;
	if (!mayJump)
		return;
	
	// Prevent jumping too fast after each other
	if (lastJumpTime + jumpRepeatTime > Time.time)
		return;
	
	if (Mathf.Abs(wallJumpContactNormal.y) < 0.2)
	{
		wallJumpContactNormal.y = 0;
		moveDirection = wallJumpContactNormal.normalized;
		// Wall jump gives us at least trotspeed
		moveSpeed = Mathf.Clamp(moveSpeed * 1.5, walkingSpeed, runningSpeed);
		// after a walljump we cannot doublejump
		doubleJumping=true;
		DidJump();
		SendMessage("DidWallJump", null, SendMessageOptions.DontRequireReceiver);
		Camera.main.SendMessage("TurnAround", SendMessageOptions.DontRequireReceiver);
	}else{
	  // cannot walljump again
		moveSpeed = 0;
	}
	
	verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
}


function ApplyJumping()
{
	// Prevent jumping too fast after each other
	if (doubleJumping && lastJumpTime + jumpRepeatTime > Time.time)
		return;
	
	if(jumping && !doubleJumping && jumpButtonPressedTwice==1 && canDoubleJump)
	{
		// Perform a doublejump
		doubleJumping=true;
		if(verticalSpeed>0.0)
			verticalSpeed=0;
		verticalSpeed+=CalculateJumpVerticalSpeed(doubleJumpHeight);
		DidJump();
		SendMessage("DidDoubleJump", SendMessageOptions.DontRequireReceiver);
	}else{	
		if (IsGrounded()) 
		{
			var height=jumpHeight;	
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) 
			{
				// Jump
				// - Only when pressing the button down
				// - With a timeout so you can press the button slightly before landing
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}

function DidJump ()
{
	jumping = true;
	jumpingReachedApex = false;
	lastJumpTime = Time.time;
	//lastJumpStartHeight = transform.position.y;
	touchWallJumpTime = -1;
	lastJumpButtonTime = -10;
}

function CalculateJumpVerticalSpeed (targetJumpHeight : float)
{
	// From the jump height and gravity we deduce the upwards speed 
	// for the character to reach at the apex.
	return Mathf.Sqrt(2 * targetJumpHeight * gravity);
}

// This function responds to the "HidePlayer" message by hiding the player. 
// The message is also 'replied to' by identically-named functions in the collision-handling scripts.
// - Used by the LevelStatus script when the level completed animation is triggered.
function HidePlayer()
{
	GameObject.Find("rootJoint").GetComponent(SkinnedMeshRenderer).enabled = false; // stop rendering the player.
	isControllable = false;	// disable player controls.
}

// This is a complementary function to the above. We don't use it in the tutorial, but it's included for
// the sake of completeness. (I like orthogonal APIs; so sue me!)
function ShowPlayer()
{
	GameObject.Find("rootJoint").GetComponent(SkinnedMeshRenderer).enabled = true; // start rendering the player again.
	isControllable = true;	// allow player to control the character again.
}

// returns true if the player is moving
function IsMoving ()  : boolean
{
	return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5;
}

// returns if the player is on ground or not
function IsGrounded ():boolean {
	return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
}

function OnControllerColliderHit (hit : ControllerColliderHit )
{
//	Debug.DrawRay(hit.point, hit.normal);
	if (hit.moveDirection.y > 0.01) 
		return;
	wallJumpContactNormal = hit.normal;
}