using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate float FloatParameter();

public delegate bool BoolParameter();

public class Conditions : MonoBehaviour {
}

public interface ICondition
{
    bool Test();

    bool ForceBool(bool Boolean);
}

public class FloatCondition : ICondition
{
    public float minValue;
    public float maxValue;

    public FloatParameter TestValue;

    public bool Test()
    {
        return minValue <= TestValue() && TestValue() <= maxValue;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class AndCondition : ICondition
{
    public ICondition A;
    public ICondition B;

    public bool Test()
    {
        return A.Test() && B.Test();
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class OrCondition : ICondition
{
    public ICondition A;
    public ICondition B;

    public bool Test()
    {
        return A.Test() || B.Test();
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class NotCondition : ICondition
{
    public ICondition condition;
    public bool Test()
    {
        return !condition.Test();
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class ALessThanB : ICondition
{
    public float A = 0;
    public float B = 0;

    public bool Test()
    {
        return A < B;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class AGreaterThanB : ICondition
{
    public float A;
    public float B;

    public bool Test()
    {
        return A > B;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class NullGameobject : ICondition
{
    public GameObject obj;

    public bool Test()
    {
        if (obj == null)
            return true;
        else
            return false;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class BoolCondition : ICondition
{
    public BoolParameter A;

    public BoolCondition() { }

    public BoolCondition(BoolParameter condition)
    {
        A = condition;
    }

    public bool Test()
    {
        return A();
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class ListHasDataCond<T> : ICondition
{
    public List<T> A;
    public bool Test()
    {
        if (A.Count > 0)
            return true;
        else
            return false;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class ListNotNullCond<T> : ICondition
{
    public List<T> list;

    public bool Test()
    {
        bool trigger = true;
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == null)
                {
                    trigger = false;
                    break;
                }
            }
            return trigger;
        }
        return false;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}

public class ListAllTrueCond: ICondition
{
    public List<bool> list;

    public bool Test()
    {
        if (list.Contains(false))
            return false;
        else
            return true;
    }

    public bool ForceBool(bool Boolean)
    {
        return Boolean;
    }
}