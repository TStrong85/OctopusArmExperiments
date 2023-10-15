using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ExperimentVariables
{
    /// <summary>
    /// How should the experiment be refered to when naming files?
    /// </summary>
    public string experimentName;


    /// <summary>
    /// If false, generates a different random seed everytime one is needed.
    /// </summary>
    public bool useFixedRandomSeed;

    /// <summary>
    /// A random seed to allow for getting consistent motion while varying other variables.
    /// </summary>
    public int randomSeed;

    /// <summary>
    /// how many times should trials be repeated once the experiment begins?
    /// </summary>
    public int numberOfTrials;

    #region placeholderVars
    /// <summary>
    /// How many segments should be used?
    /// </summary>
    //int numberOfArmSegments;

    /// <summary>
    /// How long should the entire arm be?
    /// </summary>
    //float armLength;

    /// <summary>
    /// How much should the arm's position be shifted from it's default?
    /// </summary>
    //Vector3 armPositionOffset;
    #endregion

    /// <summary>
    /// Constant acceleration applied to the arm. Can be used to simulate gravity, buoyancy, or a consistent current.
    /// </summary>
    public Vector3 artificialGravityVector;

    public enum ExperimentState { NotStarted, Running, Ended}
    public ExperimentState state;

    public void RestoreDefaults()
    {
        experimentName = "boxSearchRecreation";
        useFixedRandomSeed = false;
        randomSeed = 1;
        numberOfTrials = 3;
        artificialGravityVector = Vector3.zero;
    }

    public void CopyTo(ref ExperimentVariables other)
    {
        other.experimentName = this.experimentName;
        other.useFixedRandomSeed = this.useFixedRandomSeed;
        other.randomSeed = this.randomSeed;
        other.numberOfTrials = this.numberOfTrials;
        other.artificialGravityVector = this.artificialGravityVector;
        other.state = this.state;
    }
}
