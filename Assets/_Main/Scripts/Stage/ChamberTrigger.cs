using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChamberTrigger : MonoBehaviour
{
   [SerializeField] private Transform _respawnTransform;
   [SerializeField] private Transform _chamberTransform;

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("StageCollider"))
      {
         Vector3 nextPosition = _chamberTransform.position;
         nextPosition.z = -10;
         
         Camera.main.transform.DOMove(nextPosition, 0.5f);
      }
   }
}
