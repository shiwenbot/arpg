using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    private const float threshold = 0.5f;
    /*
     * this后面跟的类就是这个方法要扩展的类
     * 这个方法的作用是判断玩家是否在怪物目前朝向的120度扇形范围内
     * **/
    public static bool isFacingTarget(this Transform transform, Transform target)
    {
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);
        return dot >= threshold;
    }
}
