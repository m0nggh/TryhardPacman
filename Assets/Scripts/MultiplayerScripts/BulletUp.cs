using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletUp : Bullet
{
    // this bullet goes to the left
    public override void SetDirection() {
        vertical = 1;
    }
}
