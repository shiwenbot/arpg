using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    private const float threshold = 0.5f;
    /*
     * this�������������������Ҫ��չ����
     * ����������������ж�����Ƿ��ڹ���Ŀǰ�����120�����η�Χ��
     * **/
    public static bool isFacingTarget(this Transform transform, Transform target)
    {
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);
        return dot >= threshold;
    }
}
