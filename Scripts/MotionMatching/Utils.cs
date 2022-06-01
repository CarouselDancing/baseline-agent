using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel
{

namespace MotionMatching{
    
public class Utils
{

    public static float fast_negexpf(float x)
    {
        return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
    }


    public static float halflife_to_damping(float halflife, float eps = 1e-5f)
    {

        float LN2f = 0.69314718056f;
        return (4.0f * LN2f) / (halflife + eps);

    }
}

class Quat
{
    public static Quaternion quat_exp(Vector3 v, float eps = 1e-8f)
    {
        float halfangle = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);

        if (halfangle < eps)
        {
            return new Quaternion(v.x, v.y, v.z, 1.0f).normalized;
        }
        else
        {
            float c = Mathf.Cos(halfangle);
            float s = Mathf.Sin(halfangle) / halfangle;
            return new Quaternion(s * v.x, s * v.y, s * v.z, c);
        }
    }

    public static Vector3 quat_log(Quaternion q, float eps = 1e-8f)
    {
        float length = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);

        if (length < eps)
        {
            return new Vector3(q.x, q.y, q.z);
        }
        else
        {
            float halfangle = Mathf.Acos(Mathf.Clamp(q.w, -1.0f, 1.0f));
            return halfangle * (new Vector3(q.x, q.y, q.z) / length);
        }
    }

    public static Quaternion from_scaled_angle_axis(Vector3 v, float eps = 1e-8f)
    {
        return quat_exp(v / 2.0f, eps);
    }

    public static Vector3 to_scaled_angle_axis(Quaternion q, float eps = 1e-8f)
    {
        return 2.0f * quat_log(q, eps);
    }
}

[Serializable]
public class PoseState
{

    public Vector3 simulationPosition;
    public Vector3 simulationVelocity;
    public Vector3 simulationAcceleration;
    public Quaternion simulationRotation;
    public Vector3 simulationAV;
    public int nBones;
    public int[] boneParents;
    public Vector3[] bonePositions;
    public Quaternion[] boneRotations;
    public Vector3[] fkPositionBuffer;
    public Quaternion[] fkRotationBuffer;
    public float yOffset = 0;
    public PoseState(int nBones, int[] boneParents, float yOffset =0)
    {
        this.yOffset = yOffset;
        simulationPosition = new Vector3(0, yOffset, 0);
        this.boneParents = boneParents;
        this.nBones = nBones;
        bonePositions = new Vector3[nBones];
        boneRotations = new Quaternion[nBones];
        fkPositionBuffer = new Vector3[nBones];
        fkRotationBuffer = new Quaternion[nBones];
        simulationRotation = Quaternion.identity;
    }

    public PoseState(PoseState other)
    {
        this.yOffset = other.yOffset;
        simulationPosition = other.simulationPosition;
        simulationRotation = other.simulationRotation;
        simulationVelocity = other.simulationVelocity;
        simulationAcceleration = other.simulationAcceleration;
        this.boneParents = other.boneParents;
        this.nBones = other.nBones;
        bonePositions = new Vector3[nBones];
        boneRotations = new Quaternion[nBones];
        fkPositionBuffer = new Vector3[nBones];
        fkRotationBuffer = new Quaternion[nBones];
        for(int i =0; i < nBones; i++) {
            bonePositions[i] = other.bonePositions[i];
            boneRotations[i] = other.boneRotations[i];
            fkPositionBuffer[i] = other.fkPositionBuffer[i];
            fkRotationBuffer[i] = other.fkRotationBuffer[i];
        }
    }


    public void SetState(MMDatabase db, int frameIdx, bool useSim = true, bool setSimVelocity = false)
    {
        int i = 0;
        if (useSim)
        {
            bonePositions[0] = simulationPosition;
            boneRotations[0] = simulationRotation;
            i = 1;
        }
        for (; i < nBones; i++)
        {
            bonePositions[i] = db.bonePositions[frameIdx, i];
            boneRotations[i] = db.boneRotations[frameIdx, i];
        }
        if (useSim && setSimVelocity)
        {
            simulationVelocity = db.boneVelocities[frameIdx, 0];
            simulationAV = db.boneAngularVelocities[frameIdx, 0];
        }
        UpdateFKBuffer();
    }

    public void Interpolate(MMDatabase db, int frameIdx, bool useSim = true, bool setSimVelocity = false)
    {
        int i = 0;
        if (useSim)
        {
            bonePositions[0] = simulationPosition;
            boneRotations[0] = simulationRotation;
            i = 1;
        }
        for (; i < nBones; i++)
        {
            bonePositions[i] = db.bonePositions[frameIdx, i];
            boneRotations[i] = db.boneRotations[frameIdx, i];
        }
        if (useSim && setSimVelocity)
        {
            simulationVelocity = db.boneVelocities[frameIdx, 0];
            simulationAV = db.boneAngularVelocities[frameIdx, 0];
        }
        UpdateFKBuffer();
    }

    void UpdateFKBuffer()
    {
        for (int i = 0; i < nBones; i++)
        {
            ForwardKinematics(out fkPositionBuffer[i], out fkRotationBuffer[i], i);
        }
    }

    public void ForwardKinematics(out Vector3 pos, out Quaternion rot, int boneIdx)
    {

        if (boneParents[boneIdx] != -1)
        {
            Vector3 parentPos; Quaternion parentRot;
            ForwardKinematics(out parentPos, out parentRot, boneParents[boneIdx]);
            pos = parentRot * bonePositions[boneIdx] + parentPos;
            rot = parentRot * boneRotations[boneIdx];
        }
        else
        {
            pos = bonePositions[boneIdx];
            rot = boneRotations[boneIdx];
        }
    }

    public void Draw(float visScale)
    {
        for (int boneIdx = 0; boneIdx < nBones; boneIdx++)
        {
            var p = bonePositions[boneIdx];
            Vector3 pos; Quaternion rot;
            ForwardKinematics(out pos, out rot, boneIdx);
            Gizmos.DrawSphere(pos, visScale);
        }
    }

    public void Reset()
    {
        simulationPosition = new Vector3(0, yOffset, 0);
        simulationRotation = Quaternion.identity;
        simulationVelocity = Vector3.zero;
        simulationAV = Vector3.zero;
        simulationAcceleration = Vector3.zero;
    }


}



[Serializable]
public class MMControllerSettigs
{

    public float visScale = 0.05f;
    public float forceSearchTime = 0.1f;
    public float simulationVelocityHalflife = 0.27f;
    public float simulationRotationHalflife = 0.27f;
    public int predictionDistance = 20;
    public float maxSpeed = 2.5f;
    public int startFrameIdx = 0;
}

public abstract class PoseProvider : PoseProviderBase {

    public PoseState poseState;
    public MotionMatching mm;


    virtual public Quaternion GetGlobalRotation(int boneIdx)
    {
        return poseState.fkRotationBuffer[boneIdx];
    }

    virtual public Vector3 GetGlobalPosition(int boneIdx)
    {
        return poseState.fkPositionBuffer[boneIdx];
    }
    virtual public Quaternion GetGlobalRotation(HumanBodyBones bone)
    {
        var boneIdx = mm.database.boneIndexMap[bone];
        return poseState.fkRotationBuffer[boneIdx];
    }

    virtual public Vector3 GetGlobalPosition(HumanBodyBones bone)
    {
        var boneIdx = mm.database.boneIndexMap[bone];
        return poseState.fkPositionBuffer[boneIdx];
    }

    virtual public bool GetGlobalPosition(HumanBodyBones bone, out Vector3 p)
    {
        p = Vector3.zero;
        if (!mm.database.boneIndexMap.ContainsKey(bone)) return false;
        var boneIdx = mm.database.boneIndexMap[bone];
        p = poseState.fkPositionBuffer[boneIdx];
        return true;
    }

    virtual public bool GetGlobalRotation(HumanBodyBones bone, out Quaternion q)
    {
        q = Quaternion.identity;
        if (!mm.database.boneIndexMap.ContainsKey(bone)) return false;
        var boneIdx = mm.database.boneIndexMap[bone];
        q = poseState.fkRotationBuffer[boneIdx];
        return true;
    }

    public static void simulationRotationsUpdate(
    ref Quaternion rotations,
    ref Vector3 avs,
    Quaternion desiredRotation,
    float halflife, float dt)
    {
        float y = Utils.halflife_to_damping(halflife) / 2.0f;

        var inv_q = Quaternion.Inverse(desiredRotation);

        var delta_q = rotations * inv_q;
        if (delta_q.w < 0)
        {
            delta_q.w *= -1;
        }

        Vector3 j0 = Quat.to_scaled_angle_axis(delta_q);
        Vector3 j1 = avs + j0 * y;

        float eydt = Utils.fast_negexpf(y * dt);

        rotations = Quat.from_scaled_angle_axis(eydt * (j0 + j1 * dt)) * desiredRotation;
        avs = eydt * (avs - j1 * y * dt);
    }

    // Taken from https://theorangeduck.com/page/spring-roll-call#controllers

    public static void simulationPositionsUpdate(
    ref Vector3 position,
    ref Vector3 velocity,
    ref Vector3 acceleration,
    Vector3 desired_velocity,
   float halflife,
    float dt)
    {
        float y = Utils.halflife_to_damping(halflife) / 2.0f;
        Vector3 j0 = velocity - desired_velocity;
        Vector3 j1 = acceleration + j0 * y;
        float eydt = Utils.fast_negexpf(y * dt);

        Vector3 position_prev = position;
        position = eydt * (((-j1) / (y * y)) + ((-j0 - j1 * dt) / y)) +
            (j1 / (y * y)) + j0 / y + desired_velocity * dt + position_prev;
        velocity = eydt * (j0 + j1 * dt) + desired_velocity;
        acceleration = eydt * (acceleration - j1 * y * dt);
    }



    // Taken from https://theorangeduck.com/page/spring-roll-call#controllers
    public static void simulationPositionsUpdate(
        ref List<Vector3> position,
        ref List<Vector3> velocity,
        ref List<Vector3> acceleration,
        List<Vector3> desired_velocity,
        int index, float halflife,
        float dt)
    {
        float y = Utils.halflife_to_damping(halflife) / 2.0f;
        Vector3 j0 = velocity[index] - desired_velocity[index];
        Vector3 j1 = acceleration[index] + j0 * y;
        float eydt = Utils.fast_negexpf(y * dt);

        Vector3 position_prev = position[index];
        position[index] = eydt * (((-j1) / (y * y)) + ((-j0 - j1 * dt) / y)) +
            (j1 / (y * y)) + j0 / y + desired_velocity[index] * dt + position_prev;
        velocity[index] = eydt * (j0 + j1 * dt) + desired_velocity[index];
        acceleration[index] = eydt * (acceleration[index] - j1 * y * dt);
    }




    public static void simulationRotationsUpdate(
    ref List<Quaternion> rotations,
    ref List<Vector3> avs,
    List<Quaternion> desiredRotation,
    int index, float halflife, float dt)
    {
        float y = Utils.halflife_to_damping(halflife) / 2.0f;

        var inv_q = Quaternion.Inverse(desiredRotation[index]);

        var delta_q = rotations[index] * inv_q;
        if (delta_q.w < 0)
        {
            delta_q.w *= -1;
        }

        Vector3 j0 = Quat.to_scaled_angle_axis(delta_q);
        Vector3 j1 = avs[index] + j0 * y;

        float eydt = Utils.fast_negexpf(y * dt);

        rotations[index] = Quat.from_scaled_angle_axis(eydt * (j0 + j1 * dt)) * desiredRotation[index];
        avs[index] = eydt * (avs[index] - j1 * y * dt);
    }


};


class AnimatorPoseProvier : PoseProvider{

    Animator anim;
    public string idleStateName = "Idle";
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    override public Quaternion GetGlobalRotation(HumanBodyBones bone)
    {
        if (bone == HumanBodyBones.LastBone)
        {
            return transform.rotation;
        }
        return anim.GetBoneTransform(bone).rotation;
    }

    override public Vector3 GetGlobalPosition(HumanBodyBones bone)
    {
        if (bone == HumanBodyBones.LastBone)
        {
            return transform.position;
        }
        return anim.GetBoneTransform(bone).position;
    }

    override public bool GetGlobalPosition(HumanBodyBones bone, out Vector3 p)
    {
        if (bone == HumanBodyBones.LastBone)
        {
            p = transform.position;
            return true;
        }
        p = anim.GetBoneTransform(bone).position;
        return true;
    }
    override public bool GetGlobalRotation(HumanBodyBones bone, out Quaternion q)
    {
        if (bone == HumanBodyBones.LastBone)
        {
            q = transform.rotation;
            return true;
        }
        q = anim.GetBoneTransform(bone).rotation;
        return true;
    }

    override public void ResetToIdle()
    {
      
        anim.playbackTime = 0;
        anim.Play(idleStateName, -1, 0f);
        anim.Update(0);
        anim.speed = 0;
    }
};
}
}