﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScriptableObjects;

namespace ScriptableObjects
{
    public class _ItemSlot : MonoBehaviour
    {
        public Image icon;
        _Item item;

        public void AddItem(_Item newItem)
        {
            item = newItem;
            icon.sprite = item.icon;
            icon.enabled = true;
        }

        public void ClearSlot()
        {
            item = null;

            icon.sprite = null;
            icon.enabled = false;
        }
    }
}