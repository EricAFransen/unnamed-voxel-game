using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;
using World;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(IBlock));
        List<Type> BlockTypes = assembly.GetTypes().Where(t =>
           t != typeof(IBlock) &&
           typeof(IBlock).IsAssignableFrom(t) &&
           !t.IsInterface && !t.IsAbstract).ToList();
        foreach (Type BlockType in BlockTypes)
        {
            object block = Activator.CreateInstance(BlockType);
            UnityEngine.Debug.Log(BlockType);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
