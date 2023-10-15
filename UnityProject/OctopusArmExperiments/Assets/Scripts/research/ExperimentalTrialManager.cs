using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

public class ExperimentalTrialManager : MonoBehaviour
{
    public static ExperimentalTrialManager etm;
    public TrialSetup[] trialSetups;
    //public RecorderSettings recorderSettings;

    //private RecorderWindow recorderWindow;

    [SerializeField] public bool simulatePhysics = true;
    [SerializeField] public float simSkipTime = 1;

    public ExperimentVariables experimentVars;

    [Header("GUI")]
    public bool drawGui = false;
    [HideInInspector]private bool expandGui = false;
    private float guiVerticalOffset = 0f;
    private float guiWidthPercentage = 0.3f;
    private float padding = 5;

    // Start is called before the first frame update
    void Awake()
    {
        experimentVars.state = ExperimentVariables.ExperimentState.Running;
        
        if (etm == null)
        {
            etm = this;
            DontDestroyOnLoad(this);
        } else if (etm != this)
        {
            // Switch the singleton to be this
            etm.experimentVars.CopyTo(ref this.experimentVars);
            //this.experimentVars.state = ExperimentVariables.ExperimentState.NotStarted; // remove this to allow reload scene to start with food

            this.simulatePhysics = etm.simulatePhysics;
            this.simSkipTime = etm.simSkipTime;
            this.expandGui = etm.expandGui;  

            ApplyChangedVars();

            Destroy(etm.gameObject);
            etm = this;
            DontDestroyOnLoad(this);
        }

        //recorderWindow = GetRecorderWindow();

        foreach (var t in trialSetups)
        {
            Debug.Log(this.experimentVars.randomSeed);
            t.UpdateExperimentVariables(experimentVars);
        }
    }

    private void SimulatePhysicsForTime(float targetTime) {
        float totaltime = 0;
        for (int i = 0; totaltime < targetTime && i < 1000000; i++)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            totaltime += Time.fixedDeltaTime;
        } 
    }


    #region GUI
    void ApplyChangedVars()
    {
        // Push changes to any other items that need them
        Debug.Log("Changes Detected");

        Physics.autoSimulation = simulatePhysics;
        foreach(var ts in trialSetups)
        {
            ts.UpdateExperimentVariables(experimentVars);
        }
    }

    private void OnGUI()
    {
        if (!drawGui) { return; } // Don't draw anything


        GUILayout.BeginArea(new Rect(
                padding, 
                padding + guiVerticalOffset, 
                Mathf.Min(Screen.width * guiWidthPercentage, Screen.width - padding), 
                (Screen.height - padding)));

        if (GUILayout.Button("Experimental Variables", GUILayout.ExpandWidth(false)))
        {
            expandGui = !expandGui;
        }

        if (expandGui) // Draw the rest of the input fields
        {
            GUILayout.BeginVertical();



            //if (GUILayout.Button("Run all Trials"))
            //{
            //    Debug.Log("pressed!");
            //}
            //if (GUILayout.Button("Run single Trial"))
            //{
            //    Debug.Log("pressed!");
            //}

            if (GUILayout.Button("Reload Scene"))
            {
                Debug.Log("RELOADING");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                experimentVars.state = ExperimentVariables.ExperimentState.Running; // start scene with food
            }

            switch (experimentVars.state)
            {
                case ExperimentVariables.ExperimentState.NotStarted:
                    if (GUILayout.Button("Start Experiment"))
                    {
                        experimentVars.state = ExperimentVariables.ExperimentState.Running;
                    }
                    break;
                case ExperimentVariables.ExperimentState.Running:
                    if (GUILayout.Button("Stop Experiment"))
                    {
                        experimentVars.state = ExperimentVariables.ExperimentState.Ended;
                    }
                    break;
                case ExperimentVariables.ExperimentState.Ended:
                    if (GUILayout.Button("Reset Experiment"))
                    {
                        experimentVars.state = ExperimentVariables.ExperimentState.NotStarted;
                    }
                    break;
                default:
                    break;
            }





            if (DrawInputButton("Manually Simulate Time", ref simSkipTime, true))
            {
                SimulatePhysicsForTime(simSkipTime);
            }
            if (GUILayout.Button("Toggle Physics Simulation"))
            {
                simulatePhysics = !simulatePhysics;
            }

            DrawInputField("Experiment Name", ref experimentVars.experimentName, true);
            DrawInputField("Number Of Trials", ref experimentVars.numberOfTrials, true);

            DrawInputField("Use Random Seed", ref experimentVars.useFixedRandomSeed, false);
            if (experimentVars.useFixedRandomSeed)
            {
                GUILayout.BeginHorizontal();
                DrawInputField("Random Seed", ref experimentVars.randomSeed, true);
                GUILayout.EndHorizontal();
            }


            GUILayout.EndVertical();

            ////////////////////////////////////////////

            if (GUI.changed)
            {
                ApplyChangedVars();
            }

        } 
        GUILayout.EndArea();

        if (expandGui)
        {
            GUILayout.BeginArea(new Rect(padding, Screen.height - 20f, Screen.width, 20));
            GUILayout.Label(string.Format("dataPath == {0}", Application.dataPath));
            GUILayout.EndArea();
        }
    }

    private void DrawInputField(string label, ref float varRef, bool expandWidth = false)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label + ": ", GUILayout.ExpandWidth(expandWidth));
        string inputText = GUILayout.TextField(varRef.ToString(), GUILayout.ExpandWidth(expandWidth));
        try
        {
            varRef = float.Parse(inputText);
        }
        catch
        {
            if(inputText.Length == 0) { 
                varRef = 0; 
            }
        }
        
        GUILayout.EndHorizontal();
    }

    private void DrawInputField(string label, ref int varRef, bool expandWidth = false)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label + ": ", GUILayout.ExpandWidth(expandWidth));
        string inputText = GUILayout.TextField(varRef.ToString(), GUILayout.ExpandWidth(expandWidth));
        try
        {
            varRef = int.Parse(inputText);
        }
        catch
        {
            if (inputText.Length == 0)
            {
                varRef = 0;
            }
        }

        GUILayout.EndHorizontal();
    }

    private void DrawInputField(string label, ref string varRef, bool expandWidth = false)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label + ": ", GUILayout.ExpandWidth(expandWidth));
        varRef = GUILayout.TextField(varRef.ToString(), GUILayout.ExpandWidth(expandWidth));

        GUILayout.EndHorizontal();
    }

    private void DrawInputField(string label, ref bool varRef, bool expandWidth = false)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label(label + ": ", GUILayout.ExpandWidth(expandWidth));
        varRef = GUILayout.Toggle(varRef, "", GUILayout.ExpandWidth(expandWidth));

        GUILayout.EndHorizontal();
    }

    private bool DrawInputButton(string label, ref float varRef, bool expandWidth = false)
    {
        GUILayout.BeginHorizontal();

        var buttonInput = GUILayout.Button(label + ": ", GUILayout.ExpandWidth(expandWidth));
        string inputText = GUILayout.TextField(varRef.ToString(), GUILayout.ExpandWidth(expandWidth));
        try
        {
            varRef = float.Parse(inputText);
        }
        catch
        {
            if (inputText.Length == 0)
            {
                varRef = 0;
            }
        }

        GUILayout.EndHorizontal();
        return buttonInput;
    }
    #endregion
}
