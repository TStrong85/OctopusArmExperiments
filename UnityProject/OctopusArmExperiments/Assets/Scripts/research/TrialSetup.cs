using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

using System.Text;

public class TrialSetup : MonoBehaviour
{
    public RandomArmPoser randomArmPoser;
    public GameObject foodObjectsGroup;
    public string trialName;

    public Material beforeExperimentMat;
    public Material duringExperimentMat;
    public Material afterExperimentMat;
    public Renderer[] armRenderers;

    public string filePathToExportTo;

    public ExperimentVariables experimentVars;
    private ExperimentVariables prevVars;

    //For exporting position data to a file
    StringBuilder coordList = new StringBuilder();


    public void UpdateExperimentVariables(ExperimentVariables newVars)
    {
        prevVars = experimentVars;
        experimentVars = newVars;

        if (prevVars.useFixedRandomSeed != experimentVars.useFixedRandomSeed || prevVars.randomSeed != experimentVars.randomSeed)
        {
            if (experimentVars.useFixedRandomSeed)
            {
                randomArmPoser.ReinitializeRandomNumberGenerator(experimentVars.randomSeed);
            } else
            {
                randomArmPoser.ReinitializeRandomNumberGenerator();
            }
        }


        foodObjectsGroup.SetActive(experimentVars.state == ExperimentVariables.ExperimentState.Running);

        switch (experimentVars.state)
        {
            case ExperimentVariables.ExperimentState.NotStarted:
                SetArmMaterials(beforeExperimentMat);
                break;
            case ExperimentVariables.ExperimentState.Running:
                SetArmMaterials(duringExperimentMat);
                break;
            case ExperimentVariables.ExperimentState.Ended:
                SetArmMaterials(afterExperimentMat);
                break;
            default:
                break;
        }
    }


    private void EndTrial()
    {
        var newExpVars = experimentVars;
        newExpVars.state = ExperimentVariables.ExperimentState.Ended;
        UpdateExperimentVariables(newExpVars);
        
    }

    public void Update() // Fixed Update() or add timestamp
    {
        coordList.Append((Time.time).ToString()); // adds time in seconds
        for (int i = 1; i < 17; i++) 
        {
            Vector3 rawCoords = this.gameObject.transform.GetChild(2).GetChild(0).GetChild(i).transform.position ;

            coordList.Append(rawCoords.ToString("F4"));
            coordList.Append(',');
        }
        //Debug.Log(coordList);
        coordList.Append(" | ");
    }

    public void OnFoodHit()
    {
        if(experimentVars.state == ExperimentVariables.ExperimentState.Running)
        {
            EndTrial();
            //Debug.Log("Food HIT!");

            #if UNITY_EDITOR
            // If the not set, default the filepath to be an additional folder in the projects folder
            if (filePathToExportTo.Length == 0)
            {
                string projectFolder = Application.dataPath.Remove(Application.dataPath.Length - 7); // Remove "\Assets" from the filepath
                filePathToExportTo = Path.Combine(projectFolder, "ExperimentData"); // Put the path together with OS-appropriate slashes in the filepath

                // Make the directory for saving the data if it doesn't already exist
                if (!Directory.Exists(filePathToExportTo))
                {
                    Directory.CreateDirectory(filePathToExportTo);
                }
            }
            #endif

            string dataFilePath = Path.Combine(filePathToExportTo, gameObject.name + '_' + System.DateTime.Now.ToString("yyy-MM-dd-HH-mm-ss") + ".txt");
            // Write file using StreamWriter  
            using (StreamWriter writer = new StreamWriter(dataFilePath))  
            {  
                writer.WriteLine(coordList);  
            }  
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // having this here will reload the scene once food is found
        }
    }

    public void SetArmMaterials(Material newMat)
    {
        foreach(var r in armRenderers)
        {
            r.material = newMat;
        }
    }

}