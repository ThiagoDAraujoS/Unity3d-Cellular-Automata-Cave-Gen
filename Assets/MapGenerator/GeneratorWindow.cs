///-----------------------------------------------------------------
///   Class:          CaveMapGenerator
///   Description:    Unity window class, It arranges every button, sliders and displays needed for this tool to work properlly
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;

namespace CaveMapGenerator
{
	/// <summary>
	/// This class is used by unity to generate the tool's window
	/// the main purpose of this class is to manage all interactive elements (buttons, sliders..)
	/// it also sends informations across other modules.
	/// </summary>
	public class GeneratorWindow : EditorWindow
	{
		/// <summary>
		/// This object process the buffers and generates a new cave
		/// </summary>
		public Generator Generator { get; private set; }

		/// <summary>
		/// This object exports the object into a new game object
		/// </summary>
		public Loader Loader { get; private set; }

		/// <summary>
		/// This object controls the display screen inside the window
		/// </summary>
		public Display Display { get; private set; }

		/// <summary>
		/// Variable used for the ui to hold important UI data
		/// </summary>
		private int
			newMapSize = 128,
			refinementSteps = 10,
			maxPillarSize = 20;

		private AnimationCurve
			spreadCurve = new AnimationCurve(
				new Keyframe(0.0f, 0.534f),
				new Keyframe(0.662f, 0.487f),
				new Keyframe(1f, 0.8f));

		/// <summary>
		/// Variable used for the ui to hold important UI data
		/// </summary>
		private float
			initialDensity = 0.4f;

		private int
			minThreshold = 4,
			maxThreshold = 4;

		/// <summary>
		/// Base colors used for the UI
		/// </summary>
		private Color
			colorMapExportColor = new Color(090f / 255f, 176f / 255f, 131f / 255f),
			colorDisplaySetup = new Color(237f / 255f, 118f / 255f, 072f / 255f),
			colorMapSetUpColor = new Color(206f / 255f, 255f / 255f, 129f / 255f),
			colorMapGeneration = new Color(255f / 255f, 192f / 255f, 071f / 255f);

		/// <summary>
		/// This method happens when the window is opened
		/// </summary>
		[MenuItem("Window/Cave Generator")]
		public static void ShowWindow()
		{
			GeneratorWindow window = (GeneratorWindow)EditorWindow.GetWindow(typeof(GeneratorWindow));
			if ( window.Generator == null || window.Display == null || window.Loader == null )
				window.InitializeTool();

		}

		/// <summary>
		/// initialize every module nescessary for this tool
		/// </summary>
		private void InitializeTool()
		{
			wantsMouseMove = true;
			Generator = new Generator();
			Loader = new Loader();
			Display = new Display(512);
		}

		/// <summary>
		/// Draw a label
		/// </summary>
		/// <param name="color">label color</param>
		/// <param name="name">lavel tag</param>
		/// <param name="tooltip">label tooltip</param>
		private void DrawLabel( ref float heightPadding, Color color, string name, string tooltip )
		{
			heightPadding += 2;
			GUI.color = color;
			EditorGUI.LabelField(new Rect(8, heightPadding, position.width, 26), new GUIContent(name, tooltip));
			heightPadding += 1;
		}

		/// <summary>
		/// Draw the map display and hook the click events into it
		/// </summary>
		private void DrawMapDisplay( ref float heightPadding )
		{
			//move down a bit
			heightPadding += 26;

			//draw the display ui and return its port size
			Rect displayPort = Display.GUIDisplay(heightPadding, position.width);

			//if the user click the display
			if ( ( Event.current.type == EventType.MouseDown ) &&
			    ( displayPort.Contains(Event.current.mousePosition) ) )
			{
				//get that cell he clicked in
				float ratio = Generator.Size / displayPort.width;
				int mouseX = Mathf.CeilToInt(( Event.current.mousePosition.x - displayPort.x ) * ratio);
				int mouseY = Mathf.CeilToInt(( Event.current.mousePosition.y - displayPort.y ) * ratio);

				//run the flood routine
				Generator.RemoveIsle(new Coordinate(Generator.Size - mouseY, mouseX));

				//update the display
				Display.UpdateDisplay(Generator.GetMapBufferCopy(), Generator.GetHeightBufferCopy());

				//redraw the entire menu
				Repaint();
			}

			//move down until the end of the display
			heightPadding += position.width - 22;
		}

		/// <summary>
		/// Draw a full sized button, with preset size especifications
		/// </summary>
		/// <param name="buttonColor">button's color</param>
		/// <param name="name">button's tag</param>
		/// <param name="tooltip">button's tooltip</param>
		/// <param name="buttonAction">button's action</param>
		private void DrawButton( ref float heightPadding, Color buttonColor, string name, string tooltip, Action buttonAction )
		{
			//displace down a bit
			heightPadding += 24;

			//paint the button with its preselected color
			GUI.color = buttonColor;

			//hook the interaction method
			if ( GUI.Button(new Rect(1, heightPadding, position.width - 2, 26), new GUIContent(name, tooltip)) )
				buttonAction();

			//displace down a bit further
			heightPadding += 4;
		}

		/// <summary>
		/// Draw an arraw of adjacent buttons
		/// </summary>
		/// <typeparam name="T">the type of info stored in each button</typeparam>
		/// <param name="name">the label name</param>
		/// <param name="tooltip">a tooltip</param>
		/// <param name="buttonColor">color for all the buttons</param>
		/// <param name="action">what those buttons do</param>
		/// <param name="values">the values stored inside those buttons</param>
		private void DrawButtonArray<T>( ref float heightPadding, string name, string tooltip, Color buttonColor, Action<T> action, params T[] values )
		{
			//Draw the label naming the button array
			GUI.color = Color.white;
			heightPadding += 1;
			EditorGUI.LabelField(
			    new Rect(1, heightPadding, position.width - 3, 26),
			    new GUIContent(name, tooltip));

			//draw the buttons
			GUI.color = buttonColor;

			//move down a bit
			heightPadding += 16;

			//find the real size of a button
			float buttonSize = position.width / values.Length;

			//for each options create a button
			for ( int i = 0; i < values.Length; i++ )

				//Hook the button events
				if ( GUI.Button(new Rect(buttonSize * i + 1, heightPadding, buttonSize - 2, 26), new GUIContent(values[i].ToString(), tooltip)) )
					action(values[i]);

			//displace down a bit further
			heightPadding += 4;
		}

		/// <summary>
		/// Call the Threshold GUI slider function, and attach it to the variables of the generator class
		/// </summary>
		/// <param name="color">slider color</param>
		private void DrawDualSlider( ref float heightPadding, string name, string tooltip, Color color, float limitMin, float limitMax, ref float min, ref float max )
		{
			//displace down a bit
			heightPadding += 17;

			//change color
			GUI.color = color;

			//hook the slider
			EditorGUI.MinMaxSlider(
				new Rect(12, heightPadding, position.width - 40, 18),
				new GUIContent(name, tooltip),
				ref min, ref max, limitMin, limitMax);
			heightPadding -= 3;
		}

		/// <summary>
		/// Draw a menu sized int slider 
		/// </summary>
		/// <param name="color">color of the slider</param>
		/// <param name="name">name of the slider's label</param>
		/// <param name="tooltip">slider's tooltip</param>
		/// <param name="variable">the variable atached to the slider</param>
		/// <param name="min">min value</param>
		/// <param name="max">max value</param>
		private void DrawSlider( ref float heightPadding, Color color, string name, string tooltip, ref int variable, int min, int max )
		{
			//displace down
			heightPadding += 24;

			//change colors
			GUI.color = color;

			//hook slider
			variable = EditorGUI.IntSlider(
			    new Rect(12, heightPadding, position.width - 20, 18),
			    new GUIContent(name, tooltip), variable, min, max);

			//displace down
			heightPadding -= 4;
		}

		/// <summary>
		/// Draw a menu sized float slider 
		/// </summary>
		/// <param name="color">color of the slider</param>
		/// <param name="name">name of the slider's label</param>
		/// <param name="tooltip">slider's tooltip</param>
		/// <param name="variable">the variable atached to the slider</param>
		/// <param name="min">min value</param>
		/// <param name="max">max value</param>
		private void DrawSlider( ref float heightPadding, Color color, string name, string tooltip, ref float variable, float min, float max )
		{
			//displace down
			heightPadding += 24;

			//change colors
			GUI.color = color;

			//hook slider
			variable = EditorGUI.Slider(
			    new Rect(12, heightPadding, position.width - 20, 18),
			    new GUIContent(name, tooltip), variable, min, max);

			//displace down
			heightPadding -= 4;
		}

		private void DrawCurve( ref float heightPadding, Color color, string name, string tooltip, ref AnimationCurve variable )
		{
			//displace down
			heightPadding += 24;

			//change colors
			GUI.color = color;

			//hook curve block
			if ( variable == null )
				variable = new AnimationCurve();

			variable = EditorGUI.CurveField(
				new Rect(12, heightPadding, position.width - 20, 18),
				new GUIContent(name, tooltip),
				variable,
				color,
				new Rect(0, 0, 1, 1));

			//displace down
			heightPadding += 4;
		}

		private void UpdateDisplay()
		{
			Display.UpdateDisplay(Generator.GetMapBufferCopy(), Generator.GetHeightBufferCopy());
		}

		/// <summary>
		/// Base structural method, it holds the windows infos, buttons and layout
		/// </summary>
		private void OnGUI()
		{
			//if the game is playing ignore this block
			if ( EditorApplication.isPlaying )
				return;

			//if its not playing and one of the main components are null, initialize the tool
			else if ( Generator == null || Display == null || Loader == null )
				InitializeTool();

			//this variable is used to controll the y position of every element in this panel
			float heightDisplace = 0;

			//RESOLUTION
			//draw an array of buttons to display different resolution to the map display
			DrawButtonArray(ref heightDisplace, "Display Resolution", "Change the display image's resolution", colorDisplaySetup, ( resolution ) => {
				Display.Resolution = resolution;
				UpdateDisplay();
			}, 256, 512, 1024);



			//MAP DISPLAY
			//draw a map display
			DrawMapDisplay(ref heightDisplace);

			//GENERATE MAP
			//draw a button that will entirely generate a new map using default info
			DrawButton(ref heightDisplace, colorMapGeneration, "Automatically Generate Level", "Generate a new map, this map will suffer 4 iterations and will have its holes removed", () => {

				//initialize the map
				Generator.NewMap(newMapSize, initialDensity, spreadCurve);

				//refine it some times
				for ( int i = 0; i < 3; i++ )
					Generator.Refine(minThreshold, maxThreshold);

				//remove disconected chambers
				Generator.RemoveDisconectedChambers();

				//remove weird pillars
		//		Generator.RemovePillars(maxPillarSize);
				//refine it some times
				for ( int i = 0; i < 3; i++ )
					Generator.Refine(minThreshold, maxThreshold);
				//-------
				Generator.InvertHeight();

				//update to display
				UpdateDisplay();
			});

			//RESTORE
			DrawButton(ref heightDisplace, colorMapGeneration, "Restore default variables", "",
				() => {
					newMapSize = 128;
					refinementSteps = 4;
					maxPillarSize = 20;
					initialDensity = 0.4f;
					minThreshold = 4;
					maxThreshold = 4;
					spreadCurve = new AnimationCurve(
						new Keyframe(0.0f, 0.534f),
						new Keyframe(0.662f, 0.487f),
						new Keyframe(1f, 0.8f));
				});
            
			//MAP SIZE
			//draw a slider that controlls thew size of the map
			DrawSlider(ref heightDisplace, colorMapGeneration, "Cave Map Size", "Change the size of the map that is going to be generated next", ref newMapSize, 15, 250);
			
			//DENSITY CURVE
			DrawCurve(ref heightDisplace, colorMapGeneration, "Density curve", "change how dots are spread", ref spreadCurve);

			//THRESHOLD SLIDER
			//Draw a dual slider that controls the birth/death values
			//	ThresholdSlider(ref heightDisplace, colorMapGeneration);

			//THRESHOLD MIN SIZE
			//draw a slider that controlls thew size of the map
			DrawSlider(ref heightDisplace, colorMapGeneration, "Min cell count to die", "If less than this ammount is near a cell it will die", ref minThreshold, 0, maxThreshold);

			//THRESHOLD MAX SIZE
			//draw a slider that controlls thew size of the map
			DrawSlider(ref heightDisplace, colorMapGeneration, "Max cell count to live", "If more than this ammount is near a cell it will live", ref maxThreshold, minThreshold, 8);


			//PILLAR SIZE SLIDER
			//draw a slider that controlls thew size of the map
			DrawSlider(ref heightDisplace, colorMapGeneration, "Max pillar size", "Black spots this size will be removed if 'remove' pillars is pressed", ref maxPillarSize, 1, 20);




			//NEW NOISE MAP
			//draw a button that generates a new noise map to be processed further
			DrawButton(ref heightDisplace, colorMapSetUpColor, "Generate New Noise Map", "Generates a brand new noise map to be further processed", () => {
				Generator.NewMap(newMapSize, initialDensity, spreadCurve);
				UpdateDisplay();
			});



			//ITERATE
			//Draw a button that will make the generator to iterate once
			DrawButton(ref heightDisplace, colorMapSetUpColor, "Iterate", "Force the celular automata to iterate once", () => {
				Generator.Refine(minThreshold, maxThreshold);
				UpdateDisplay();
			});

			//FILL HOLES
			//Draw a button that will remove all holes from the displayed map
			DrawButton(ref heightDisplace, colorMapSetUpColor, "Remove disconnected chambers", "Remove every 'hole' on the map", () => {
				Generator.RemoveDisconectedChambers();
				UpdateDisplay();
			});


			//REMOVE PILLARS
			DrawButton(ref heightDisplace, colorMapSetUpColor, "Remove Pillars", "Remove every pillar on the map", () => {
				Generator.RemovePillars(maxPillarSize);
				UpdateDisplay();
			});

			DrawButton(ref heightDisplace, colorMapSetUpColor, "Invert height map", "invert the values of the height map", () => {
				Generator.InvertHeight();
				UpdateDisplay();
			});

			//EXPORT
			//Draw a button that will export the generated map into a scriptable object
			DrawButton(ref heightDisplace, colorMapExportColor, "Export", "Export the map into a scriptable object to be used later",
			    () => Loader.Export(Generator.GetMapBufferCopy(),Generator.GetHeightBufferCopy()));
		}

	}
}
#endif
