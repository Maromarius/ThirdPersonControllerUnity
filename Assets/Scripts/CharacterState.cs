using UnityEngine;
using System.Collections;

public class CharacterState : MonoBehaviour {

	//GUI Componenets
	private Texture energy_background_gui;
	private Texture energy_liquid_gui;
	private Texture energy_frame_gui;
	private GUISkin logskin;

	//GUI Dimensions
	private float level_width = Screen.width*0.15f;
	private float level_height;

	//Changeable variables
	//Energy Related
	private float maximum_energy = 100;
	private float jump_energy = 2;
	private float run_energy = 0.1f;
	private float teleport_energy = 5;
	private float shoot_energy = 5;
	private float proj_charge_energy = 1;
	private float energy_after_exhausted = 100;

	//Time Records
	private float shoot_record;
	private float teleport_record;
	private float exhausted_start;
	private float exhausted_time = 5;//Character is exhausted for 5 seconds
	private float exhausted_replenish_time = 3;

	//Cooldown
	private float energy_replenish_rate = 0.05f;
	private float teleport_cooldown = 2;
	private float shoot_cooldown = 1;

	//Private State Variables
	private float energy;

	//Character state related variables
	enum State{aliveAndWell, exhausted};
	private State state = State.aliveAndWell;


	// Use this for initialization
	void Start () {

		//if(networkView.isMine){
			//Get GUI Texture from resource folder
			//energy_background_gui = Resources.Load("Textures/energy_bar_background") as Texture;
			//energy_liquid_gui = Resources.Load("Textures/energy") as Texture;
			//energy_frame_gui = Resources.Load("Textures/energybar") as Texture;

			energy = maximum_energy;
			level_height = level_width * 0.4f;

			//Set records
			shoot_record = Time.time;
			teleport_record = Time.time;

			//Load elements from Resources folder
			//logskin = Resources.Load("GUISkins/LogSkin") as GUISkin;
		//}
	}
	
	// Update is called once per frame
	void Update () {
		//if(networkView.isMine){
			if(state == State.aliveAndWell){
				//ENERGY REPLENISH
				energy +=energy_replenish_rate;
				if(energy>maximum_energy) energy = maximum_energy;
			}else if(state == State.exhausted){
				if((Time.time - exhausted_start) >= (exhausted_time - exhausted_replenish_time)){	
					if((Time.time - exhausted_start) >= exhausted_time ){	
						state = State.aliveAndWell;
					}else{//Still replenishing energy
						float percentage = (Time.time - exhausted_start - (exhausted_time - exhausted_replenish_time))/exhausted_replenish_time;
						energy = percentage *energy_after_exhausted;
					}

				}
			}
		//}
	}
	/*
	void OnGUI(){
		if(networkView.isMine){
			//ENERGY GUI COMPONENT

			if (energy_background_gui) {
				GUI.DrawTexture(new Rect(Screen.width/2-Screen.width/4, Screen.height/2 - Screen.height/7.5f, Screen.width/2, Screen.width/2), energy_background_gui, ScaleMode.StretchToFill, true, 10.0F);
			}
			// draw the filled-in part:
			GUI.BeginGroup (new Rect (Screen.width/2-Screen.width/4, Screen.height/2 - Screen.height/7.5f, (energy/maximum_energy)*Screen.width/2, Screen.width/2));
			GUI.DrawTexture(new Rect(0,0, Screen.width/2, Screen.width/2), energy_liquid_gui, ScaleMode.StretchToFill, true, 10.0F);
			GUI.EndGroup ();

			GUI.skin = logskin;
			TextAnchor prevAlignment = logskin.box.alignment;
			int prevFontSize = logskin.box.fontSize;
			logskin.box.alignment = TextAnchor.UpperCenter;
			logskin.box.fontSize = Screen.height/40;
			if(state == State.aliveAndWell){
				GUI.Box(new Rect(Screen.width/2-140, Screen.height*5/6+(level_height/1.1f), 280, level_height), (int)energy +"/"+ (int)maximum_energy);
			}else if(state == State.exhausted){
				GUI.Box(new Rect(Screen.width/2-140, Screen.height*5/6+(level_height/1.1f), 280, level_height), "Exhausted");
			}
			logskin.box.alignment = prevAlignment;
			logskin.box.fontSize = prevFontSize;
			GUI.skin = null;

			if (energy_frame_gui) {
				GUI.DrawTexture(new Rect(Screen.width/2-Screen.width/4, Screen.height/2 - Screen.height/7.5f, Screen.width/2, Screen.width/2), energy_frame_gui, ScaleMode.StretchToFill, true, 10.0F);
			}
			//---------------------------
		}
	}
*/


//PUBLIC FUNCTIONS RELATED TO ENERGY CHANGES

	//Reduces current energy when called (if enough energy)
	public bool jump(){
		if(state == State.aliveAndWell){
			energy -= jump_energy;

			//Update state is energy below 0
			if(energy <= 0) {
				setExhaustedState();
			}

			return true;
		}
		return false;
	}

	//Reduces current energy when called (if enough energy)
	public bool run(){
		if(state == State.aliveAndWell){
			energy -= run_energy;

			//Update state is energy below 0
			if(energy <= 0) {
				setExhaustedState();
			}
			return true;
		}
		return false;
	}

	public bool teleport(){
		if(state == State.aliveAndWell){
			if(Time.time >= teleport_record + teleport_cooldown*Time.deltaTime){
				energy -= teleport_energy;
				teleport_record = Time.time;

				//Update state is energy below 0
				if(energy <= 0) {
					setExhaustedState();
				}

				return true;
			}
		}
		return false;
	}

	public bool shoot(){
		if(state == State.aliveAndWell){
			if(Time.time >= shoot_record + shoot_cooldown){
				if (Debug.isDebugBuild) 
					Debug.Log("Shooting");
				energy -= shoot_energy;
				shoot_record = Time.time;

				//Update state is energy below 0
				if(energy <= 0) {
					setExhaustedState();
				}

				return true;
			}
		}
		return false;
	}

	public bool chargeBullet(){
		if(state == State.aliveAndWell){
			energy -= proj_charge_energy;

			//Update state is energy below 0
			if(energy <= 0){
				setExhaustedState();
			}
		}
		return false;
	}

	//Character gets hit by bullet
	public void getHit(float damage){
		energy -= damage;

		//Update state is energy below 0
		if(energy <= 0) {
			setExhaustedState();
		}
	}

	/// <summary>
	/// Checks if the state of the character == exhausted
	/// </summary>
	/// <returns><c>true</c> if exhauted, <c>false</c> otherwise.</returns>
	public bool isExhauted(){
		if (state == State.exhausted)
			return true;
		return false;
	}

	//PRIVATE FUNCTIONS

	/// <summary>
	/// Sets the state of character to --> exhausted.
	/// </summary>
	private void setExhaustedState(){
		state = State.exhausted;
		energy = 0;
		//Set exhauted timer:
		exhausted_start = Time.time;
	}

}
