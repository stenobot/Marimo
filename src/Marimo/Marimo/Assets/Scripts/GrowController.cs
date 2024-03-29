﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowController : MonoBehaviour
{
    /// <summary>
    /// A reference to the pipe tracer
    /// </summary>
    public PipeTracer PipeTracer;

    /// <summary>
    /// Muckle's current level of growth, where 0 is normal size and 1 is largest
    /// </summary>
    public float CurrentGrowthLevel;

    /// <summary>
    /// Inflate Muckle.
    /// </summary>
    /// <param name="anim">Muckle's primary Animator</param>
    public void Grow(Animator anim)
    {
        if (CurrentGrowthLevel < 0.05f)
        {
            anim.Play(Globals.ANIMSTATE_MUCKLE_GROW);
        }

        if (CurrentGrowthLevel > 0.95f)
        {
            CurrentGrowthLevel = 1;

            // Drain the pipes
            Vector2 pos = new Vector2(Mathf.CeilToInt(transform.position.x), Mathf.CeilToInt(transform.position.y));
            Vector2 direction = transform.lossyScale.x > 0 ? Vector2.right : Vector2.left;
            PipeNode pipe = PipeTracer.GetPipeNodeAtPosition(pos);
            if (pipe != null)
                pipe.Drain(direction);
        }
        else
        {
            CurrentGrowthLevel += (Time.deltaTime * 3);
        }

        anim.SetFloat(Globals.ANIM_PARAM_NORM_TIME, CurrentGrowthLevel);
    }

    /// <summary>
    /// Deflate Muckle.
    /// </summary>
    /// <param name="anim">Muckle's primary Animator</param>
    public void Shrink(Animator anim)
    {
        if (CurrentGrowthLevel <= 0)
        {
            anim.Play(Globals.ANIMSTATE_IDLE);
            return;
        }
        else if (CurrentGrowthLevel > 0 && CurrentGrowthLevel < 0.05f)
        {
            CurrentGrowthLevel = 0;
        }
        else
        {
            anim.Play(Globals.ANIMSTATE_MUCKLE_GROW);
            CurrentGrowthLevel -= (Time.deltaTime * 3);
        }

        anim.SetFloat(Globals.ANIM_PARAM_NORM_TIME, CurrentGrowthLevel);
    }
}