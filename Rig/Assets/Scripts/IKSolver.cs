using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    #region Variables.

    /// <summary>
    /// IK chain length.
    /// </summary>
    public int chainLength = 2;

    /// <summary>
    /// IK chain.
    /// </summary>
    public Transform[] bones;

    /// <summary>
    /// IK target and pole.
    /// </summary>
    public Transform target;
    public Transform pole;

    /// <summary>
    /// Solver iterations per update.
    /// </summary>
    public int iterations = 10;
    public float tolerance = 0.001f;

    [Range(0, 1)] public float snapBackStrength = 1f;


    private Vector3[] _positions;
    private float[] _boneLengths;
    private float _totalLength;

    #endregion

    #region Methods.

    private void Init()
    {
        _positions = new Vector3[chainLength + 1];
        _boneLengths = new float[chainLength];
        _totalLength = 0;

        for (int i = 0; i < bones.Length; ++i)
        {
            _positions[i] = bones[i].position;
        }

        for (int i = 0; i < bones.Length - 1; ++i)
        {
            _boneLengths[i] = Vector3.Distance(bones[i].position, bones[i + 1].position);
            _totalLength += _boneLengths[i];
        }
    }

    private void UpdateIK()
    {
        // No IK target available.
        if (target == null)
        {
            return;
        }

        for (int it = 0; it < iterations; ++it)
        {
            // Find the distance between root bone and target.
            float distBetweenRootAndTarget = Vector3.Distance(bones[0].position, target.position);

            // Beyond reach.
            if (_totalLength <= distBetweenRootAndTarget)
            {
                for (int i = 0; i < bones.Length - 1; ++i)
                {
                    Vector3 boneToTarget = target.position - bones[i].position;
                    float distBetweenBoneAndTarget = Vector3.Distance(bones[i].position, target.position);
                    float t = _boneLengths[i] / distBetweenBoneAndTarget;
                
                    // Find the new position.
                    bones[i + 1].position = bones[i].position + boneToTarget * t;
                }
            }
            else
            {
                // The initial position. 
                Vector3 initPos = bones[0].position;

                // Find the distance between end effector and target.
                float distBetweenEndAndTarget = Vector3.Distance(bones[^1].position, target.position);

                // If the distance is greater than the tolerance.
                while (distBetweenEndAndTarget > tolerance)
                {
                    #region Forward reaching.

                    bones[^1].position = target.position;

                    for (int i = bones.Length - 2; i >= 0; --i)
                    {
                        float distBetweenBoneAndTarget = Vector3.Distance(bones[i + 1].position, bones[i].position);
                        float t = _boneLengths[i] / distBetweenBoneAndTarget;
                    
                        // Find the new position.
                        bones[i].position = (1 - t) * bones[i + 1].position + t * bones[i].position;
                    }

                    #endregion

                    #region Backward reaching.

                    bones[0].position = initPos;

                    for (int i = 0; i < bones.Length - 1; ++i)
                    {
                        float distBetweenBones = Vector3.Distance(bones[i].position, bones[i + 1].position);
                        float t = _boneLengths[i] / distBetweenBones;

                        bones[i + 1].position = (1 - t) * bones[i].position + t * bones[i + 1].position;
                    }

                    #endregion
                
                    distBetweenEndAndTarget = Vector3.Distance(bones[^1].position, target.position);

                }
            }

            if (pole != null)
            {
                for (int i = bones.Length - 1; i > 1; --i)
                {
                    Plane plane = new Plane(bones[i].position - bones[i - 2].position, bones[i - 2].position);
                    Vector3 poleProjected = plane.ClosestPointOnPlane(pole.position);
                    Vector3 boneProjected = plane.ClosestPointOnPlane(bones[i - 1].position);
                    float angle = Vector3.SignedAngle(boneProjected - bones[i - 2].position,
                        poleProjected - bones[i - 2].position, plane.normal);
                    bones[i - 1].position =
                        Quaternion.AngleAxis(angle, plane.normal) * (bones[i - 1].position - bones[i - 2].position) +
                        bones[i - 2].position;
                }
            }
        }
    }

    #endregion

    #region Life-cycle callbacks.

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        UpdateIK();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < bones.Length - 1; ++i)
        {
            if (bones[i] != null && bones[i + 1] != null)
            {
                Handles.color = Color.white;
                Handles.DrawLine(bones[i].position, bones[i + 1].position);
            }
        }

        foreach (Transform bone in bones)
        {
            if (bone != null)
            {
                Handles.color = Handles.xAxisColor;
                Handles.SphereHandleCap(0, bone.position, bone.rotation, 0.1f, EventType.Repaint);
            }
        }

        if (target != null)
        {
            Handles.color = Handles.yAxisColor;
            Handles.SphereHandleCap(0, target.position, target.rotation, 0.1f, EventType.Repaint);
        }

        if (pole != null)
        {
            Handles.color = Handles.zAxisColor;
            Handles.SphereHandleCap(0, pole.position, pole.rotation, 0.1f, EventType.Repaint);
        }
    }

    #endregion
}
