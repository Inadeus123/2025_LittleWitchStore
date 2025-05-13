using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : MonoBehaviour
{
   [SerializeField] public CatAnimationData animationData;

   private void Awake()
   {
      animationData.Initialize();
   }
   
}
