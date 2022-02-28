using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Color = UnityEngine.Color;

public struct Points
{
    public Vector3 from;
    public Vector3 to;
    public Color color;
    public bool solid;
}

public class IKDemo : MonoBehaviour
{
    #region Variables.

    public Transform[] bones;
    public Transform target;
    public Transform end;

    public Transform[] bonesSpare;
    private Vector3[] _bones;
    private float[] _lengths;
    private float _totalLength;
    
    private Vector3 _move;
    private Vector3 _from;
    private List<Points> _lines;
    private Points _points;

    private bool _draw;
    private bool _complete;

    #endregion

    #region Methods.

    private void Init()
    {
        _bones = new Vector3[bones.Length];
        _lengths = new float[bones.Length - 1];
        _totalLength = 0;
        _lines = new List<Points>();
        _points = new Points();
        _draw = false;
        _complete = false;

        for (int i = 0; i < bones.Length; ++i)
        {
            bonesSpare[i].position = bones[i].position;
            _bones[i] = bones[i].position;
        }

        for (int i = 0; i < bones.Length - 1; ++i)
        {
            _lengths[i] = Vector3.Distance(bones[i].position, bones[i + 1].position);
            _totalLength += _lengths[i];
        }
    }

    private void BeyondReachDemo()
    {
        Sequence sequence = DOTween.Sequence();
        
        // Move the target.
        Tweener tweener0 = target.transform.DOMove(end.position, 1f).SetDelay(1f);
        sequence.Append(tweener0);
        
        // Root bone to the target.
        _points.from = bonesSpare[0].position;
        _points.to = end.position;
        _points.color = Color.red;
        _points.solid = false;
        
        _move = bonesSpare[0].position;
        _draw = true;
        
        Tweener tweener1 = DOTween.To(() => _move, value => _move = value, _points.to, 1f).SetDelay(1f)
            .OnComplete(() =>
            {
                _draw = false;
                Points points = _points;
                _lines.Add(points);
            });
        sequence.Append(tweener1);

        // Move bones.
        for (int i = 0; i < bones.Length - 1; ++i)
        {
            float dist = Vector3.Distance(bonesSpare[i].position, end.position);
            float t = _lengths[i] / dist;
            bonesSpare[i + 1].position = (1 - t) * bonesSpare[i].position + t * end.position;

            Tweener tweener = bones[i + 1].DOMove(bonesSpare[i + 1].position, 1f).SetDelay(1f);
            sequence.Append(tweener);
        }
        _complete = true;
    }

    private void WithinReachDemo()
    {
        Sequence sequence = DOTween.Sequence();
        
        // Move the target.
        Tweener tweener0 = target.transform.DOMove(end.position, 1f).SetDelay(1f);
        sequence.Append(tweener0);
        
        // Move end bone to the target.
        Tweener tweener1 = bones[3].DOMove(end.position, 1f).SetDelay(1f);
        bonesSpare[3].position = end.position;
        sequence.Append(tweener1);

        Vector3 initPos = bones[0].position;

        List<Points> points = new List<Points>();
        points.Add(new Points());
        points.Add(new Points());
        points.Add(new Points());

        for (int i = bones.Length - 2; i >= 0; --i)
        {
            Points point = new Points();
            point.from = bonesSpare[i].position;
            point.to = bonesSpare[i + 1].position;
            point.color = Color.red;
            point.solid = false;
            _points.from = bonesSpare[i].position;
            _points.to = bonesSpare[i + 1].position;
            _points.color = Color.red;
            _points.solid = false;

            //_move = point.from;

            Tweener tweener2 = DOTween.To(() => _move, value => _move = value, point.to, 0.1f).SetDelay(1f)
                .OnStart(() =>
                {
                    _move = point.from;
                    _from = point.from;
                    _draw = true;
                })
                .OnComplete(() =>
                {
                    _draw = false;

                    _lines.Add(point);
                });
            sequence.Append(tweener2);
            
            float distBetweenBoneAndTarget = Vector3.Distance(bonesSpare[i + 1].position, bonesSpare[i].position);
            float t = _lengths[i] / distBetweenBoneAndTarget;
                    
            // Find the new position.
            bonesSpare[i].position = (1 - t) * bonesSpare[i + 1].position + t * bonesSpare[i].position;

            Tweener tweener = bones[i].DOMove(bonesSpare[i].position, 1f).SetDelay(1f);
            sequence.Append(tweener);
        }

        Vector3[] pos = {initPos, bonesSpare[1].position, bonesSpare[2].position, bonesSpare[3].position};

        Tweener tweener3 = bones[0].DOMove(pos[0], 1f).SetDelay(3f);
        sequence.Append(tweener3);

        for (int i = 0; i < bones.Length - 1; ++i)
        {
            float distBetweenBones = Vector3.Distance(pos[i], pos[i + 1]);
            float t = _lengths[i] / distBetweenBones;

            pos[i + 1] = (1 - t) * pos[i] + t * pos[i + 1];

            Tweener tweener = bones[i + 1].DOMove(pos[i + 1], 1f).SetDelay(1f);
            sequence.Append(tweener);
        }

        _complete = true;
    }

    private void DrawInitialBones()
    {
        Handles.color = Color.white;
        
        for (int i = 0; i < _bones.Length; ++i)
        {
            Handles.DrawWireDisc(_bones[i], Vector3.forward, 0.05f);
        }

        for (int i = 0; i < _bones.Length - 1; ++i)
        {
            Handles.DrawDottedLine(_bones[i], _bones[i + 1], 0.1f);
        }
    }

    private void DrawIKBones()
    {
        for (int i = 0; i < bones.Length - 1; ++i)
        {
            if (bones[i] == null || bones[i + 1] == null)
            {
                continue;
            }

            Handles.color = Color.white;
            Handles.DrawLine(bones[i].position, bones[i + 1].position);
        }
        
        for (int i = 0; i < bones.Length; ++i)
        {
            if (bones[i] == null)
            {
                continue;
            }
            Handles.color = Handles.xAxisColor;
            Handles.SphereHandleCap(0, bones[i].position, bones[i].rotation, 0.1f, EventType.Repaint);
        }
    }

    #endregion

    #region Life-cycle Callbacks.

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (!_complete)
        {
            //BeyondReachDemo();
            WithinReachDemo();
        }
    }

    private void OnDrawGizmos()
    {
        if (_bones != null)
        {
            DrawInitialBones();
        }

        if (_lines != null)
        {
            foreach (Points points in _lines)
            {
                Handles.color = points.color;
                if (points.solid)
                {
                    Handles.DrawLine(points.from, points.to);
                }
                else
                {
                    Handles.DrawDottedLine(points.from, points.to, 0.1f);
                }
            }
        }

        DrawIKBones();
        
        if (_draw)
        {
            Handles.color = _points.color;
            if (_points.solid)
            {
                //Handles.DrawLine(_from, _move);
                //Handles.DrawLine(_points.from, _move);
            }
            else
            {
                //Handles.DrawDottedLine(_from, _move, 0.1f);
                //Handles.DrawDottedLine(_points.from, _move, 0.1f);
            }
        }

        if (target != null)
        {
            Handles.color = Handles.zAxisColor;
            Handles.SphereHandleCap(0, target.position, target.rotation, 0.1f, EventType.Repaint);
        }
    }

    #endregion
}
