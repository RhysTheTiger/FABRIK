using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class PoleDemo : MonoBehaviour
{
    #region Variables.

    public Transform[] bones;
    public Transform pole;
    public Transform plane;

    private Transform first;
    private Transform second;

    private Vector3 point1;
    private Vector3 point2;

    private int draw;

    private bool complete;

    private Plane _plane;
    private Plane _plane1;

    #endregion

    #region Methods.

    private void Demo()
    {
        Sequence sequence = DOTween.Sequence();

        float y = 0;
        Tweener tweener = DOTween.To(() => y, value => y = value, 10f, 1f).SetDelay(1)
            .OnComplete(() =>
            {
                plane.position = bones[3].position;
                plane.LookAt(bones[1]);
            });
        sequence.Append(tweener);

        Plane plane1 = new Plane(bones[1].position - bones[3].position, bones[3].position);
        _plane = plane1;
        point1 = plane1.ClosestPointOnPlane(bones[2].position);
        point2 = plane1.ClosestPointOnPlane(pole.position);
        
        float x = 0;
        Tweener tweener0 = DOTween.To(() => x, value => x = value, 10f, 2f).SetDelay(1)
            .OnComplete(() =>
            {
                draw = 1;
            });
        sequence.Append(tweener0);
        
        float targetAngle = Vector3.SignedAngle(point1 - bones[3].position,
            point2 - bones[3].position, plane1.normal);
        float angle = 0f;
        Vector3 temp = bones[2].position;

        Tweener tweener1 = DOTween.To(() => angle, value => angle = value, targetAngle, 4f).SetDelay(2)
            .OnUpdate(() =>
            {
                bones[2].position = Quaternion.AngleAxis(angle, plane1.normal) * (temp - bones[3].position) +
                                    bones[3].position;
            })
            .OnComplete(() =>
            {
                draw = 2;
            });
        sequence.Append(tweener1);

        float z = 0;
        Tweener tweener2 = DOTween.To(() => z, value => z = value, 10f, 2f).SetDelay(1)
            .OnComplete(() =>
            {
                plane.position = bones[2].position;
                plane.LookAt(bones[0]);
            });
        sequence.Append(tweener2);
        
        Plane plane2 = new Plane(bones[0].position - bones[2].position, bones[2].position);
        _plane1 = plane2;
        point1 = plane2.ClosestPointOnPlane(bones[1].position);
        point2 = plane2.ClosestPointOnPlane(pole.position);

        float j = 0;
        Tweener tweener3 = DOTween.To(() => j, value => j = value, 10f, 2f).SetDelay(1)
            .OnComplete(() =>
            {
                draw = 3;
            });
        sequence.Append(tweener3);
        
        float targetAngle1 = Vector3.SignedAngle(point1 - bones[2].position,
            point2 - bones[2].position, plane2.normal);
        float angle1 = 0f;
        Vector3 temp1 = bones[1].position;
        
        Tweener tweener4 = DOTween.To(() => angle1, value => angle1 = value, targetAngle1, 4f).SetDelay(2)
            .OnUpdate(() =>
            {
                bones[1].position = Quaternion.AngleAxis(angle1, plane2.normal) * (temp1 - bones[2].position) +
                                    bones[2].position;
            })
            .OnComplete(() =>
            {
                draw = 4;
                plane.position = new Vector3(0.0f, 100.0f, 0.0f);
            });
        sequence.Append(tweener4);

        complete = true;
    }

    #endregion

    #region Life-cycle Callbacks.

    private void Awake()
    {
        first = bones[3];
        second = bones[1];

        draw = 0;
        complete = false;
    }

    private void Update()
    {
        if (!complete)
        {
            Demo();
        }
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
        
        if (pole != null)
        {
            Handles.color = Handles.zAxisColor;
            Handles.SphereHandleCap(0, pole.position, pole.rotation, 0.1f, EventType.Repaint);
        }

        switch (draw)
        {
            case 0:
                Handles.color = Color.white;
                Handles.DrawDottedLine(bones[3].position, bones[1].position, 0.1f);
                break;
            case 1:
                Handles.color = Handles.xAxisColor;
                Handles.SphereHandleCap(0, _plane.ClosestPointOnPlane(bones[2].position), Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Handles.zAxisColor;
                Handles.SphereHandleCap(0, _plane.ClosestPointOnPlane(pole.position), Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.DrawDottedLine(bones[2].position, _plane.ClosestPointOnPlane(bones[2].position), 0.1f);
                Handles.DrawDottedLine(pole.position, _plane.ClosestPointOnPlane(pole.position), 0.1f);
                break;
            case 2:
                Handles.color = Color.white;
                Handles.DrawDottedLine(bones[2].position, bones[0].position, 0.1f);
                break;
            case 3:
                Handles.color = Handles.xAxisColor;
                Handles.SphereHandleCap(0, _plane1.ClosestPointOnPlane(bones[1].position), Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Handles.zAxisColor;
                Handles.SphereHandleCap(0, _plane1.ClosestPointOnPlane(pole.position), Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.DrawDottedLine(bones[1].position, _plane1.ClosestPointOnPlane(bones[1].position), 0.1f);
                Handles.DrawDottedLine(pole.position, _plane1.ClosestPointOnPlane(pole.position), 0.1f);
                break;
            default:
                break;
        }
    }

    #endregion
}
