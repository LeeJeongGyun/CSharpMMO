using System;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

public class ObjectManager
{
    private Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    public MyPlayerController MyPlayerController { get; set; }

    public void Add(ObjectInfo objectInfo, bool myPlayer = false)
    {
        GameObject go;
        if (objectInfo.ObjectType == ObjectType.Player)
        {
            if (myPlayer)
            {
                go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = objectInfo.Name;
                MyPlayerController mpc = go.GetComponent<MyPlayerController>();
                mpc.Info = objectInfo;
                MyPlayerController = mpc;
            }
            else
            {
                go = Managers.Resource.Instantiate("Creature/Player");
                go.name = objectInfo.Name;
                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Info = objectInfo;
            }
        }
        else if (objectInfo.ObjectType == ObjectType.Monster)
        {
            go = Managers.Resource.Instantiate("Creature/Monster");
            go.name = objectInfo.Name;
            BaseController bc = go.GetComponent<BaseController>();
            bc.Info = objectInfo;
        }
        else
        {
            go = Managers.Resource.Instantiate("Misc/Bullet");
            BaseController bc = go.GetComponent<BaseController>();
            bc.Info = objectInfo;
        }

        _objects.Add(objectInfo.ObjectId, go);
    }

    public void Remove(int objectId)
    {
        GameObject go = null;
        _objects.TryGetValue(objectId, out go);
        if (go != null)
        {
            Managers.Resource.Destroy(go);
            _objects.Remove(objectId);
        }
    }

    public void Clear()
    {
        foreach (var go in _objects.Values)
            Managers.Resource.Destroy(go);

        _objects.Clear();
    }

    public GameObject FindObject(int objectId)
    {
        GameObject go;
        _objects.TryGetValue(objectId, out go);
        return go;
    }

    public GameObject FindObject(Vector3Int pos)
    {
        foreach (var obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc != null)
            {
                if (cc.CellPos.Equals(pos))
                    return obj;
            }
        }

        return null;
    }

    public GameObject FindObject(Func<GameObject, bool> func)
    {
        foreach (var obj in _objects.Values)
        {
            if (func(obj))
                return obj;
        }

        return null;
    }
}
