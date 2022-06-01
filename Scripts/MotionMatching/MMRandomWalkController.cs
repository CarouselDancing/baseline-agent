using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Carousel
{

namespace MotionMatching{
public enum ControllerMode
{
    OLD,
    SYNC,
    INPUT,
    NONE,
    NONE2
}

class TrajectoryController
{
    List<Vector3> trajectoryDesiredVel = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    List<Quaternion> trajectoryDesiredRot = new List<Quaternion>() { new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1) };

    public List<Vector3> trajectoryPos = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    List<Vector3> trajectoryVel = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    List<Vector3> trajectoryAcc = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    public List<Quaternion> trajectoryRot = new List<Quaternion>() { new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1), new Quaternion(0, 0, 0, 1) };
    List<Vector3> trajectoryAV = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };

    List<Vector3> featureTrajectoryPos = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    float forwardSpeed;
    float sideSpeed;
    public Vector3 desiredVelocity;
    public Quaternion desiredRotation;
    Vector3 stickDir;



    public void Update(PoseState poseState,float predictionDt,  MMControllerSettigs settings, bool updateStickDir=false)
    {

        if (updateStickDir) { 
            var y = UnityEngine.Random.Range(-1f, 1f);
            var x = UnityEngine.Random.Range(-1f, 1f);
            forwardSpeed = Mathf.Abs(y) * settings.maxSpeed;
            sideSpeed = Mathf.Abs(x) * settings.maxSpeed;
            stickDir = new Vector3(x, 0, y).normalized;
        }

        desiredVelocity = UpdateDesiredVelocity(poseState, 0);

        desiredRotation = UpdateDesiredRotation(desiredVelocity, 0);


        UpdateDesiredRotationTrajectory(poseState, predictionDt);
        UpdatePredictedRotationTrajectory(poseState, predictionDt, settings.simulationVelocityHalflife);

        UpdateDesiredVelocityTrajectory(poseState, predictionDt);
        UpdatePredictedPositionTrajectory(poseState, predictionDt, settings.simulationVelocityHalflife);
    }

    void UpdateDesiredRotationTrajectory(PoseState poseState, float dt)
    {
        trajectoryDesiredRot[0] = poseState.simulationRotation;
        trajectoryAV[0] = poseState.simulationAV;
        for (int i = 1; i < trajectoryVel.Count; i++)
        {
            trajectoryDesiredRot[i] = UpdateDesiredRotation(desiredVelocity, dt * i);
        }
    }


    void UpdateDesiredVelocityTrajectory(PoseState poseState, float dt)
    {
        trajectoryDesiredVel[0] = desiredVelocity;
        for (int i = 1; i < trajectoryVel.Count; i++)
        {
            trajectoryDesiredVel[i] = UpdateDesiredVelocity(poseState, i * dt);
        }
    }
    void UpdatePredictedRotationTrajectory(PoseState poseState, float dt, float halflife)
    {
        for (int i = 0; i < trajectoryPos.Count; i++)
        {
            trajectoryRot[i] = poseState.simulationRotation;
            trajectoryAV[i] = poseState.simulationAV;
        }

        for (int i = 1; i < trajectoryPos.Count; i++)
        {
            PoseProvider.simulationRotationsUpdate(ref trajectoryRot, ref trajectoryAV, trajectoryDesiredRot, i, halflife, i * dt);
        }
    }

    void UpdatePredictedPositionTrajectory(PoseState poseState, float dt, float halflife)
    {
        trajectoryPos[0] = poseState.simulationPosition;
        trajectoryVel[0] = poseState.simulationVelocity;
        trajectoryAcc[0] = poseState.simulationAcceleration;
        for (int i = 1; i < trajectoryPos.Count; i++)
        {
            trajectoryPos[i] = trajectoryPos[i - 1];
            trajectoryVel[i] = trajectoryVel[i - 1];
            trajectoryAcc[i] = trajectoryAcc[i - 1];

            PoseProvider.simulationPositionsUpdate(ref trajectoryPos, ref trajectoryVel, ref trajectoryAcc, trajectoryDesiredVel, i, halflife, dt);

        }
    }

    Vector3 UpdateDesiredVelocity(PoseState poseState, float dt)
    {
        var cameraRot = Quaternion.AngleAxis(0, new Vector3(0, 1, 0));
        var globalStickDir = cameraRot * stickDir;
        var localStickDir = Quaternion.Inverse(poseState.simulationRotation) * globalStickDir;
        // Scale stick by forward, sideways and backwards speeds
        if (forwardSpeed >= sideSpeed)
        {
            localStickDir *= forwardSpeed;
        }
        else
        {
            localStickDir *= sideSpeed;
        }
        return poseState.simulationRotation * localStickDir;
    }

    Quaternion UpdateDesiredRotation(Vector3 desiredVelocity, float dt)
    {
        if (Vector3.Magnitude(desiredVelocity) > 0)
        {
            var direction = desiredVelocity.normalized;
            var a = Mathf.Rad2Deg * Mathf.Atan2(direction.x, direction.z);
            return Quaternion.AngleAxis(a, new Vector3(0, 1, 0));
        }
        else
        {

            var cameraRot = Quaternion.AngleAxis(0, new Vector3(0, 1, 0));
            return cameraRot;
        }

    }
}

public class MMRandomWalkController : PoseProvider
{

    PoseState oldPose;
    PoseState refPose;
    public MMControllerSettigs settings;
    public bool prediction;
    public int frameIdx = 0;
    public float FPS = 60;
    public bool active = true;
    public float syncTimer = 0;
    float forceSearchTimer = 0.1f;
    public int nnDistributionSize = 100;
    public float interval;
    public float dt;
    public ControllerMode mode;
    public bool drawQuery;
    public float yOffset;
    public bool setGlobalPose = false;
    public bool useTrajectory = false;
    public bool resetActive = false;
    TrajectoryController trajectoryController;

    public GameObject MotionMatchingPrefab;

    public Vector3 intitialOffset;// at the start of the episode
    void Awake()
    {
       
        trajectoryController = new TrajectoryController();

        if (mm==null || !mm.initialized) Initialize();
    }
    public void Initialize()
    {
        intitialOffset = transform.position;
        var o = GameObject.Find("MMDatabase");
        if (o == null){
            o = GameObject.Instantiate(MotionMatchingPrefab);
            o.name = "MMDatabase";
            mm = o.GetComponent<MotionMatching>();
            mm.Load();
        }else{
            mm = o.GetComponent<MotionMatching>();
        }
        poseState = new PoseState(mm.initialState);
        poseState.yOffset = yOffset;
        poseState.Reset();
        refPose = new PoseState(mm.database.nBones, mm.database.boneParents, yOffset);
        oldPose = new PoseState(mm.database.nBones, mm.database.boneParents, yOffset);
        mm.ComputeFeatures();

        interval = 1.0f / FPS;
        syncTimer = interval;
        prediction = false;
        frameIdx = settings.startFrameIdx;
    }

    void Update()
    {
        if (mm == null || !mm.initialized) return;
        dt = Time.deltaTime;
        if (!active) return;
        interval = 1.0f / FPS;
        switch (mode)
        {
            case ControllerMode.SYNC:
                if (syncTimer > 0)
                {
                    syncTimer -= Time.deltaTime;
                    return;
                }
                syncTimer = interval;
                Step();
                break;
            case ControllerMode.OLD:
                Step();
                break;
            case ControllerMode.INPUT:
                if(Input.GetKeyDown(KeyCode.Space)) Step();
                break;
            case ControllerMode.NONE:
                return;
            case ControllerMode.NONE2:
                break;

        }
        SetPose();
        if (setGlobalPose) { 
            transform.position = poseState.simulationPosition + intitialOffset;
            transform.rotation = poseState.simulationRotation;
        }
    }

   

    public void Step()
    {
        if (forceSearchTimer > 0)
        {
            forceSearchTimer -= interval;
        }
        bool end_of_anim = false;
        try
        {
            end_of_anim = mm.trajectoryIndexClamp(frameIdx, 1) == frameIdx;
        }
        catch (Exception e)
        {

        }

        Vector3 desiredVelocity = mm.database.boneVelocities[frameIdx, 0];
        Quaternion desiredRotation = mm.database.boneRotations[frameIdx, 0];

        
        if (useTrajectory) {
            float predictionDt = interval * settings.predictionDistance;
            bool updateStickDir = end_of_anim || forceSearchTimer <= 0.0f;
            trajectoryController.Update(poseState, predictionDt, settings, updateStickDir);
            //desiredVelocity = trajectoryController.desiredVelocity;
            //desiredRotation = trajectoryController.desiredRotation;
        }

        simulationPositionsUpdate(ref poseState.simulationPosition, ref poseState.simulationVelocity, ref poseState.simulationAcceleration, desiredVelocity, settings.simulationVelocityHalflife, interval);
        simulationRotationsUpdate(ref poseState.simulationRotation, ref poseState.simulationAV, desiredRotation, settings.simulationRotationHalflife, interval);

        if (end_of_anim || forceSearchTimer <= 0.0f)
        {
            FindTransition();
            forceSearchTimer = settings.forceSearchTime;
            prediction = true;
        }
        else
        {
            prediction = false;
        }
        frameIdx++;//prevents getting stuck
        if (frameIdx >= mm.database.nFrames)
        {
            frameIdx = 0;
        }
    }


    public void FindTransition()
    {
        int oldFrameIdx = frameIdx;
        //frameIdx = mm.FindTransition(poseState, frameIdx);
        var result = new SearchResult(nnDistributionSize);
        if (useTrajectory) { 
            
            mm.FindTransition(poseState, frameIdx, trajectoryController.trajectoryPos, trajectoryController.trajectoryRot, ref result);
        }
        else
        {
            mm.FindTransition(poseState, frameIdx, ref result);
        }
        

        //frameIdx = result.sortedList.Values[0];
        var bestIdx = result.GetBestIndex();
        frameIdx = result.GetRandomIndex(oldFrameIdx);
        //Debug.Log("new frame idx"+ frameIdx.ToString() + " " + bestIdx.ToString());
        if (drawQuery)
        {
            oldPose.SetState(mm.database, oldFrameIdx, false);
            refPose.SetState(mm.database, frameIdx, false);
        }

        if (frameIdx >= mm.database.nFrames)
        {
            frameIdx = 0;
        }
    }


    public void SetPose()
    {
        if (!mm.initialized) return;
        poseState.SetState(mm.database, frameIdx, true, false);
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (prediction)
        {
            Gizmos.color = Color.red;
        }
        if (poseState != null) poseState.Draw(settings.visScale);
        if (drawQuery) { 
            Gizmos.color = Color.yellow;
            if (refPose != null) refPose.Draw(settings.visScale);
            Gizmos.color = Color.red;
            if (oldPose != null) oldPose.Draw(settings.visScale);
        }
    }

    override public void ResetToIdle()
    {
        if (mm== null || !mm.initialized) Initialize();
        if (resetActive) { 
            frameIdx = settings.startFrameIdx;
            poseState.Reset();
            SetPose();
            syncTimer = interval;
            forceSearchTimer = settings.forceSearchTime;
        }
    }
}
}
}
