using UnityEngine;

public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public TransformData(Transform transform, Space space = Space.Self)
    {
        if (space == Space.Self)
        {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
        }
        else
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }
    }

    public static TransformData Interpolate(Transform current, Transform target, float fac, Space space = Space.Self)
    {
        return Interpolate(new TransformData(current), new TransformData(target), fac);
    }

    public static TransformData Interpolate(Transform current, TransformData target, float fac, Space space = Space.Self)
    {
        return Interpolate(new TransformData(current), target, fac);
    }

    public static TransformData Interpolate(TransformData current, Transform target, float fac, Space space = Space.Self)
    {
        return Interpolate(current, new TransformData(target), fac);
    }

    public static TransformData Interpolate(TransformData current, TransformData target, float fac)
    {
        return new TransformData(
            Vector3.Lerp(current.position, target.position, fac),
            Quaternion.Slerp(current.rotation, target.rotation, fac),
            Vector3.Lerp(current.scale, target.scale, fac)
        );
    }

    public static void Apply(TransformData source, Transform target, Space space = Space.Self)
    {
        if (space == Space.Self)
        {
            target.localPosition = source.position;
            target.localRotation = source.rotation;
            target.localScale = source.scale;
        }
        else
        {
            target.position = source.position;
            target.rotation = source.rotation;
            target.localScale = source.scale;
        }
    }
}