using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    public void Use(EntityStats source, EntityStats target);
}
